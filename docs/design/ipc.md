---
title: 通信抽象（进程内/进程间）
---

# 通信抽象（进程内/进程间）

你提到的思路是正确的：把“进程内通信/进程间通信”统一封装为一个 `channel`（消息队列），在此之上提供 Actor（单线程邮箱）模型，上层只面对 `Tell/Publish`，从而屏蔽底层细节。

## 统一抽象

- **Channel（消息投递）**：负责把 `Envelope` 投递到某个“目的地”
- **Actor（顺序处理）**：每个 `sessionId` 对应一个 actor，保证同一 session 的消息串行处理
- **Router（路由）**：决定同一个 `sessionId` 的消息应落到哪个 actor/哪个进程

## 语义差异（必须显式处理）

即便抽象统一，进程间通信也会带来不可避免的语义差异：

- **投递语义**：IPC/网络一般是 at-least-once（可能重复）→ 必须幂等/去重
- **有序性**：要靠 `orderingKey=sessionId` 做分区/路由，否则会乱序
- **背压**：队列积压时，必须有限流/降采样/快照追赶策略

## 传输层优先级（本机优先）

按“实现复杂度 vs 性能收益”建议分层（你提到的顺序也合理）：

1. **进程内队列（Channel）**：最快、最可靠（Monolith 默认）
2. **本机 IPC（Unix Domain Socket / NamedPipe）**：跨进程但仍是本机，运维简单
3. **localhost TCP**：跨进程/跨容器更通用
4. **远程 TCP/消息队列**：多机部署
5. **共享内存（MemoryMappedFile + RingBuffer + FlatBuffers）**：吞吐最高，但实现与调试成本最大（建议在有明确性能瓶颈后再上）

## 当前仓库状态

- v1 目前的 Cohort 是 Monolith（同进程内存通信）
- 同时开始引入 `Cohort.Messaging` 作为“channel 抽象”的起点：
  - `InProcMessageBus`：进程内队列
  - `NamedPipeMessageBus`：本机进程间 IPC（Windows 友好）
  - `UnixDomainSocketMessageBus`：本机进程间 IPC（Linux/macOS，Windows 在较新版本也支持 AF_UNIX；实际落地建议保留 fallback）

## 关于 Windows “支持 Unix Socket”

Windows 的 AF_UNIX 支持在系统版本与运行环境（例如容器、权限、路径规则）上仍有差异；因此框架层面建议：

- 优先：Linux/macOS 使用 Unix Domain Socket
- Windows：默认 NamedPipe，提供 UDS 作为可选项
