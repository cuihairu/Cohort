---
title: 运维与观测
---

# 运维与观测

## 指标端点

- `GET /sessions`：返回会话与客户端指标（tick、事件计数、lag、ack age、resync 次数）

## CI / 覆盖率

- GitHub Actions：`.github/workflows/ci.yml`
- 覆盖率：CI 生成 `coverage.cobertura.xml` 并上传到 Codecov（README badge 展示）

## 发布（Tag）

推送 `v*` tag 会触发自动创建 GitHub Release：

```bash
git tag v0.1.0
git push origin v0.1.0
```
