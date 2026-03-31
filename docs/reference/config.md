---
title: 配置参考
---

# 配置参考

目前配置通过 `Cohort.Engine.Session.SessionConfig` 提供默认值（后续会做 appsettings 映射）。

当前默认值：

```text
tickDurationMs = 100
inputDelayTicks = 2
snapshotEveryTicks = 1
maxEventsPerTick = 200
maxLagTicks = 50
resyncCooldownMs = 2000
```

关键参数：

- `tickDurationMs`
- `inputDelayTicks`
- `snapshotEveryTicks`
- `maxEventsPerTick`
- `maxLagTicks`
- `resyncCooldownMs`

## 参数说明

### `tickDurationMs`

含义：

- 服务器权威逻辑 tick 周期，单位毫秒

默认值：

- `100`

影响：

- 值越小，同步越细，但服务端和客户端压力越大
- 值越大，系统更稳，但输入生效和恢复粒度更粗

建议：

- 直播互动场景通常建议落在 `50~66`
- 当前默认值 `100` 更偏保守和易于打通链路，不一定适合作为最终线上参数

### `inputDelayTicks`

含义：

- 输入被调度到未来多少 tick 后统一结算

默认值：

- `2`

影响：

- 值越大，越能吸收抖动和乱序
- 值越小，体感更快，但更容易受网络波动影响

### `snapshotEveryTicks`

含义：

- 周期快照发送频率

默认值：

- `1`

影响：

- `1` 表示每 tick 都会发送快照
- 值越大，带宽和序列化成本越低，但恢复会变慢

建议：

- 当前默认值适合开发期快速验证
- 线上通常需要结合快照体积和带宽成本调整到 `2~5`

### `maxEventsPerTick`

含义：

- 单 tick 最大进入权威逻辑的事件数

默认值：

- `200`

影响：

- 是防止“单帧爆炸”的安全阀
- 超限后会触发 reducer 的裁剪、合并或降采样效果

### `maxLagTicks`

含义：

- 客户端落后多少 tick 后触发强制快照恢复

默认值：

- `50`

影响：

- 值越大，系统对短期抖动更宽容
- 值越大，也意味着慢端会在错误状态停留更久

建议：

- 当前默认值偏保守，适合避免开发环境频繁 resync
- 线上通常要明显收紧，否则恢复不够积极

### `resyncCooldownMs`

含义：

- 同一客户端两次强制快照之间的最小间隔

默认值：

- `2000`

影响：

- 防止短时间内反复 resync
- 值太小会导致抖动式恢复
- 值太大又会让问题客户端长时间无法重新对齐

## 推荐阅读

- [参数调优](./tuning.md)
- [故障排查](./troubleshooting.md)
- [帧同步最佳实践](../design/frame-sync-best-practices.md)
