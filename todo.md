# TODO（按实现顺序）

## 0. 仓库基础

- [x] 建立 solution：`Cohort.sln`（当前目标框架为 `net9.0`；如需 LTS 可改为 `net8.0`）
- [x] 建立包结构（建议）：
  - `src/Cohort.Server`（HTTP + WebSocket + DI）
  - `src/Cohort.Engine`（Session actor、tick、buffer、backpressure）
  - `src/Cohort.Protocol`（消息与序列化）
  - `src/Cohort.Adapters.Abstractions`（平台适配器接口）
  - `src/Cohort.SampleGame`（最小示例 Game Module，用于验证帧同步链路）
  - `tests/...`（至少覆盖 tick/缓冲/降采样）
- [x] 加入 `docs/` 的目录结构与索引（已完成初稿）

## 1. Tick/帧同步（第一阶段：打通闭环，v1=权威状态快照）

- [x] 实现 `SessionActor`（单线程邮箱模型 + 固定 tick）
- [x] 实现 `AudienceEvent` 统一模型 + 稳定排序规则
- [x] 实现 `inputDelayTicks` 缓冲（按 tick 结算事件）
- [x] 实现 `maxEventsPerTick` + 合并/降采样策略（简单合并：同用户同类型累加）
- [x] 实现 WebSocket 协议：`hello/welcome/ack/snapshot`
- [x] 实现 per-client ack 跟踪与落后检测（`maxLagTicks`）与指标输出（`/sessions`）
- [x] 实现快照机制（全量 snapshot；序列化用 JSON）

## 2. 平台接入（开源只做接口与最小 stub）

- [x] `IPlatformEventMapper` / `IPlatformEventVerifier` 接口
- [x] `Platform Ingress`：HTTP endpoint 接收 event（示例 payload + 验签 stub）
- [x] 去重机制（基于 platformEventId + TTL）

## 3. Match/阵营对抗抽象

- [ ] `Match` / `Faction` / `Session` 的关系建模
- [ ] 多客户端订阅同一 `Match` 的状态（两个主播客户端看到同一局）
- [ ] “观众加入阵营”的通用规则接口（弹幕口令/礼物映射）

## 3.5 通信与部署模式

- [ ] 抽象 `MessageBus/ActorRef/Router`（支持 Monolith 与拆分）
- [x] 提供 InProc bus（进程内）
- [x] 提供 NamedPipe bus（本机 IPC 示例）
- [ ] 提供 TCP/HTTP transport（localhost / 远程）
- [ ] 评估并规划共享内存 transport（MemoryMappedFile + FlatBuffers）

## 4. 可观测性与压测

- [ ] 指标：tick 耗时、事件数、丢弃/合并数、ack 延迟、落后 tick
- [ ] 日志结构化（最少：sessionId、tickId、clientId）
- [ ] 提供一个本地压测脚本（模拟 N 个观众事件 + 2 个客户端）

## 5. 文档定稿

- [ ] 把 `docs/spec.md` 定稿为“第一版可实现规格”（根据你确认的取舍：权威状态同步 vs 输入帧广播）
- [ ] 增加 `docs/protocol.md`（协议字段与示例）
- [ ] 增加 `docs/ops.md`（部署与参数调优：tick/inputDelay/maxEvents 等）

## 6. 文档站（VuePress）

- [x] 建立 VuePress 文档站（`docs/.vuepress` + `npm run docs:build`）
- [x] 整理架构/帧同步/旧服分析文档到 `docs/`
- [x] GitHub Pages 自动部署（`docs.yml`）

## 7. CI / 覆盖率 / 发布

- [x] CI：build + test（`ci.yml`）
- [x] 覆盖率上传（Codecov）
- [x] Tag 自动 Release（`release.yml`）
