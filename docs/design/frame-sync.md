---
title: 帧同步（权威 Tick + 快照）
---

# 帧同步（权威 Tick + 快照）

## Tick 时钟

- 服务器维护 `tickId` 单调递增
- tick 周期：`tickDurationMs`
- 平台事件进入后会被调度到 `tickId + inputDelayTicks`，用于吸收抖动/乱序

## 快照下发

服务器在每 `snapshotEveryTicks` 生成一次 `snapshot`：

- `snapshot.tickId`：快照对应的权威 tick
- `snapshot.state`：可序列化状态（v1 为 JSON）

## ACK 与追赶

客户端应在应用快照后发送：

- `ack.lastAppliedTickId`

服务器检测客户端落后（`serverTick - lastAckTick > maxLagTicks`）时：

- 下发 `snapshot(forced=true, reason="lag")` 给该客户端
- 限流：`resyncCooldownMs` 防止短时间反复强制快照

