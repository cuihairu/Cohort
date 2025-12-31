---
home: true
title: Cohort
heroImage: /logo.svg
heroText: Cohort
tagline: 面向直播弹幕/礼物驱动的互动对战游戏框架（服务器权威 Tick + 状态快照）
actions:
  - text: 快速开始
    link: /guide/getting-started.html
    type: primary
  - text: 架构设计
    link: /design/architecture.html
    type: secondary
features:
  - title: 服务器权威 Tick
    details: 服务器持续推进逻辑帧，不再“等最慢端”，客户端以快照对齐。
  - title: 弹幕/礼物输入缓冲
    details: 通过 inputDelayTicks 抵抗平台事件抖动/乱序，并提供降采样与合并策略。
  - title: 可观测性
    details: 内置会话与客户端 lag/ack 指标，定位“不同步”问题可解释、可复现。
footer: Apache-2.0 License
---

