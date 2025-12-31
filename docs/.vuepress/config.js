module.exports = {
  title: "Cohort",
  description: "直播弹幕/礼物驱动的互动对战游戏框架（服务器权威 Tick + 状态快照）",
  base: process.env.VUEPRESS_BASE || "/",
  themeConfig: {
    logo: "/logo.svg",
    repo: "cuihairu/Cohort",
    docsDir: "docs",
    nav: [
      { text: "指南", link: "/guide/getting-started.html" },
      { text: "设计", link: "/design/architecture.html" },
      { text: "参考", link: "/reference/protocol.html" }
    ],
    sidebar: {
      "/guide/": ["/guide/getting-started.md", "/guide/split-mode.md"],
      "/design/": [
        "/design/architecture.md",
        "/design/frame-sync.md",
        "/design/ipc.md",
        "/design/spec.md",
        "/design/legacy-real-server-analysis.md"
      ],
      "/reference/": ["/reference/protocol.md", "/reference/config.md", "/reference/operations.md"]
    }
  },
  plugins: ["mermaidjs"]
};
