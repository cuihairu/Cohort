---
title: 快速开始
---

# 快速开始

## 环境

- .NET SDK：`9.0.x`（见 `global.json`）
- Node.js：建议 `>= 20`（用于构建 VuePress 文档站）

## 运行服务器（本地）

```bash
dotnet run --project src/Cohort.Server
```

服务默认监听 `http://localhost:5000`：

- `GET /health`：健康检查
- `GET /sessions`：会话/客户端指标
- `POST /ingress/test`：注入测试事件（模拟平台事件）
- `WS /ws`：主播客户端连接入口

## WebSocket：连接与收快照

连接：`ws://localhost:5000/ws`

第一条消息必须发送 `hello`：

```json
{"type":"hello"}
```

服务器会返回 `welcome`，其中包含 `sessionId` 与 tick 参数。

之后服务器会持续下发 `snapshot`（当前默认每 tick 一次），客户端应在应用快照后回 `ack`：

```json
{"type":"ack","sessionId":"<sessionId>","clientId":"<clientId>","lastAppliedTickId":123}
```

当客户端落后超过 `maxLagTicks`，服务器会额外推送 `snapshot(forced=true, reason=\"lag\")`，提示客户端立即以该快照对齐 tick。

## 注入测试事件（模拟平台）

```bash
curl -X POST http://localhost:5000/ingress/test \
  -H 'content-type: application/json' \
  -d '{"sessionId":"<sessionId>","platform":"test","userId":"u1","kind":"Like"}'
```

## 构建文档站（静态页面）

```bash
npm install
npm run docs:build
```

产物目录：`docs/.vuepress/dist`

