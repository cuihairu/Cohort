---
title: 协议参考
---

# 协议参考（v1）

## hello（client -> server）

```json
{"type":"hello","sessionId":"optional","clientId":"optional"}
```

## welcome（server -> client）

字段说明：`tickDurationMs`、`inputDelayTicks`、`snapshotEveryTicks`。

## snapshot（server -> client）

当 `forced=true && reason="lag"` 时表示服务器判定客户端落后，需要立刻以该快照对齐。

## ack（client -> server）

```json
{"type":"ack","sessionId":"...","clientId":"...","lastAppliedTickId":123}
```

