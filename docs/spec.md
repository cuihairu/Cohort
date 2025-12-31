# Cohort 规格草案（以帧同步为核心）

本规格用于指导把旧框架抽象为可开源的“直播互动弹幕游戏框架”。

## 1. 目标与非目标

### 目标

- 支持 1 个或多个主播客户端（直播伴侣/PC 客户端）连接到 Cohort Server。
- 接入抖音/快手等平台的互动事件（弹幕、礼物、点赞、关注等），转为统一的 `AudienceEvent` 输入流。
- 以“可复用的帧同步/状态同步架构”为核心，支撑：
  - 单房间（单主播）玩法
  - 多房间对抗（多主播阵营/PK）
- 强可观测性：能解释“为什么不同步”，并能自动自愈（快照追赶、降采样等）。

### 非目标（开源版本不做）

- 不包含任何具体玩法（数值、英雄、技能、美术、配置表等）。
- 不包含平台的私有抓包/逆向能力；只提供“适配器接口 + 示例 stub（或文档）”。

## 2. 总体架构

### 2.1 组件

- **Client Gateway（WebSocket）**
  - 主播客户端连接入口
  - 会话管理（鉴权、重连、订阅）
  - 下发 tick/快照/状态
- **Platform Ingress（HTTP/Webhook）**
  - 接收平台推送（或由适配器拉取）
  - 校验签名/去重
  - 转换为统一 `AudienceEvent`
- **Session Engine（Actor per Session）**
  - 每个 Session/Match 一个单线程 actor（邮箱模型），保证顺序与可推理性
  - 输入缓冲、节拍控制、背压、快照、回放
- **Game Module（插件接口）**
  - 开源仅提供接口与一个最小示例（例如计数器/回声）用于验证帧同步链路

### 2.2 数据模型（概念）

- `Session`：一场直播对应的游戏会话（单主播）。
- `Match`：多 Session 组成的对抗（例如 2 个主播阵营战）。
- `Faction`：阵营（通常与主播绑定）。
- `Audience`：观众玩家（平台 userId）。

## 3. 帧同步设计（推荐：服务器权威 tick + 每客户端追赶）

> 关键原则：**服务器永远推进，不为慢端暂停；慢端用快照追赶。**

### 3.1 Tick 时钟

- 固定 tick：`tickDurationMs`（默认建议 50ms 或 100ms，取决于玩法负载）。
- 服务器为每个 Session 维护单调递增 `tickId`（从 0 开始）。
- 所有下发数据都带 `tickId` 与 `serverTimeMs`（用于客户端测漂移）。

### 3.2 输入缓冲（处理平台抖动/乱序）

平台事件到达存在抖动与乱序，建议引入输入延迟窗口：

- `inputDelayTicks`（例如 2~6 tick）
- 服务器对事件打上 `ingestTimeMs`，并在 `tickId = nowTick - inputDelayTicks` 时统一结算该 tick 的事件集合。
- 事件排序规则必须明确且稳定（例如：`(ingestTimeMs, platformEventId)`）。

这样可以在不追求毫秒级实时的前提下，换取稳定性与一致性。

### 3.3 下行数据类型

开源框架至少支持两种下行策略（后续可同时支持）。本仓库 **v1 先实现 A：权威状态同步**。

#### A) 权威状态同步（v1 采用）

- 服务器运行 Game Module（或其“最小核心逻辑”），每 tick 更新状态。
- 下行：
  - `StateSnapshot`（周期性，例如每 20 tick 一次）包含完整可序列化状态
  - `StateDelta`（可选）或仅发送 snapshot（简单但带宽更高）
- 客户端只负责渲染/特效，不承担一致性责任。

优点：从根上解决“客户端不同步”。

#### B) 输入帧广播（后续可选）

- 服务器每 tick 下发 `TickInputBatch`：
  - `{tickId, events:[...], checksum?}`
- 客户端按 tickId 严格推进模拟，不允许按本地帧“自由跑”。
- 服务器维护：
  - 环形缓冲保存最近 `N` tick 的 input batch
  - `Snapshot`（即使只做输入广播，也建议保存“关键状态快照”用于快速追赶）

### 3.4 每客户端 ACK 与追赶

每个客户端维护 `lastAppliedTickId`，并周期性发送：

- `ClientAck { sessionId, lastAppliedTickId, rttMs?, clientTimeMs? }`

服务器侧策略：

- 不暂停推进；如果检测某客户端落后超过阈值（例如 `> maxLagTicks`）：
  - 直接下发 `StateSnapshot`（或从最近快照 + 回放少量 tick）
  - 客户端收到后“跳到 tickId = snapshot.tickId”，再继续追

### 3.5 背压与降采样（直播弹幕场景必须有）

定义“高峰保护”机制，避免帧爆炸：

- 每 tick 事件上限：`maxEventsPerTick`
- 超限策略（按玩法选择）：
  - 合并：同一用户的同类事件合并（点赞合并、礼物叠加）
  - 降采样：低价值事件抽样保留
  - 分层：核心事件（礼物/关键指令）优先，聊天类可丢弃或转为“展示用”通道

## 4. 客户端协议（草案）

WebSocket 消息统一 JSON（开源版先易用，后续可支持 MessagePack）。

### 4.1 连接与会话

- `hello`：客户端 -> 服务器，包含 clientVersion、token、desiredSessionId（可选）
- `welcome`：服务器 -> 客户端，返回 sessionId、tickDurationMs、inputDelayTicks、serverTimeMs、capabilities
- `start` / `ready`：开始/准备（取决于玩法）
- `ack`：上报 lastAppliedTickId

### 4.2 帧/状态

- `tick`：下发 tick（输入批或状态增量）
- `snapshot`：下发状态快照（用于重连或追赶）
- `resyncRequest`：客户端请求（极少需要，优先 server push）

补充：当服务器检测到客户端落后超过 `maxLagTicks` 时，会对该客户端额外推送带 `forced=true`、`reason="lag"` 的 `snapshot`，用于提示客户端立即以该快照为准对齐 tick。

## 5. 可观测性（必须落地的指标）

每个 session/match 至少输出：

- `tickId`、tick 处理耗时、事件数、合并/丢弃数
- 每客户端：ack 延迟、落后 tick 数、重连次数、快照追赶次数
- 队列长度（ingress 队列、session mailbox、发送队列）
