# 什亭之匣·回响

<p align="center">
  <strong>Shittim Chest: Echo</strong>
</p>

<p align="center">
  一款基于 C#、WPF 与 .NET 10 开发的 Windows 桌面项目
</p>

<p align="center">
  <img src="./ShittimLoadingLogo.png" alt="什亭之匣·回响 Logo" width="180">
</p>

<h1 align="center">什亭之匣·回响</h1>

<p align="center">
  <strong>Shittim Chest: Echo</strong>
</p>

<p align="center">
  一个基于 <strong>C# · WPF · .NET 10</strong> 开发的 Windows 桌面项目
  <br>
  灵感来源于《蔚蓝档案》中的「什亭之匣」
</p>

<p align="center">
  <a href="https://github.com/你的GitHub用户名/ShittimEcho">
    <img src="https://img.shields.io/github/stars/你的GitHub用户名/ShittimEcho?style=for-the-badge&logo=github&label=Stars" alt="GitHub Stars">
  </a>
  <a href="https://github.com/你的GitHub用户名/ShittimEcho">
    <img src="https://img.shields.io/github/forks/你的GitHub用户名/ShittimEcho?style=for-the-badge&logo=github&label=Forks" alt="GitHub Forks">
  </a>
  <img src="https://img.shields.io/badge/C%23-.NET%2010-512BD4?style=for-the-badge&logo=csharp&logoColor=white" alt="C# .NET 10">
  <img src="https://img.shields.io/badge/WPF-Windows-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="WPF Windows">
</p>

<p align="center">
  <a href="https://github.com/你的GitHub用户名/ShittimEcho">
    <img src="https://img.shields.io/badge/GitHub-项目主页-181717?style=flat-square&logo=github&logoColor=white" alt="GitHub 项目主页">
  </a>
</p>

<br>

<p align="center">
  <strong>💬 官方 QQ 群</strong>
  <br>
  什亭之匣回响项目部
  <br>
  群号：1125620277
</p>

<p align="center">
  <a href="https://qm.qq.com/q/v2rWqjpYkg">
    <img src="https://img.shields.io/badge/👉%20点击加入官方群聊-12B7F5?style=for-the-badge&logo=tencentqq&logoColor=white" alt="点击加入官方群聊">
  </a>
</p>

<br>

<p align="center">
  <strong>❤️ 爱发电主页</strong>
  <br>
  项目作者的爱发电主页导航
</p>

<p align="center">
  <a href="https://afdian.com/a/soymilk520">
    <img src="https://img.shields.io/badge/❤️%20访问我的爱发电主页-F36C6C?style=for-the-badge&logo=afdian&logoColor=white" alt="访问我的爱发电主页">
  </a>
</p>

<br>

---

## 📖 项目简介

**什亭之匣·回响（Shittim Chest: Echo）** 是一个基于 **C# + WPF + .NET 10** 开发的 Windows 桌面项目。

本项目以《蔚蓝档案》中的「什亭之匣」为灵感，希望将其具有代表性的系统体验逐步融入 Windows 桌面环境。

项目目前处于**第一代版本**。

第一代版本主要用于实现 Windows 启动、锁屏以及登录后的基础音频体验，为后续的交互系统、角色系统以及更加完整的什亭之匣桌面体验奠定基础。

---
## 📖 项目简介

**什亭之匣·回响（Shittim Chest: Echo）** 是一个基于 **C# + WPF + .NET 10** 开发的 Windows 桌面项目。

本项目以《蔚蓝档案》中的「什亭之匣」为灵感，希望将其具有代表性的系统体验逐步融入 Windows 桌面环境。

项目目前处于**第一代版本**。

第一代版本主要用于实现项目最基础的 Windows 启动与登录音频体验，为后续的交互系统、角色系统以及更加完整的什亭之匣桌面体验奠定基础。

---

# ✨ 当前已实现功能

目前已经实际实现并完成测试的功能主要包括以下两项。

## 🔒 Windows 开机 / 锁屏音乐

Windows 启动后，程序可以在用户进入 Windows 桌面之前运行，并在 Windows 登录 / 锁屏界面阶段播放指定音乐。

当前启动流程：

```text
Windows 启动
     │
     ▼
Windows 登录 / 锁屏界面
     │
     ▼
什亭之匣·回响启动
     │
     ▼
播放锁屏音乐
```

该功能是第一代版本目前最核心的功能之一。

---

## 🔊 进入 Windows 桌面后的欢迎语音

当用户完成 Windows 登录并进入桌面后，程序可以检测到用户从锁屏状态进入 Windows 桌面的状态变化。

随后停止或淡出锁屏音乐，并播放预先配置的欢迎语音。

当前流程：

```text
用户登录 Windows
       │
       ▼
检测进入桌面
       │
       ▼
停止 / 淡出锁屏音乐
       │
       ▼
播放欢迎语音
       │
       ▼
进入 Windows 桌面
```

该功能用于模拟「进入什亭之匣系统后获得语音欢迎」的体验。

---

# 🖥️ 第一代版本启动流程

目前第一代版本的完整启动体验可以概括为：

```text
┌──────────────────────┐
│      Windows 启动     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│  Windows 登录 / 锁屏  │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│     播放锁屏音乐      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│      用户登录         │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   停止 / 淡出音乐     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│      播放欢迎语音     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│     Windows 桌面      │
└──────────────────────┘
```

目前第一代版本主要围绕这套启动体验进行开发。

---

# 🚧 当前开发状态

## 第一代版本

**状态：核心功能已完成**

| 功能               | 状态    |
| ---------------- | ----- |
| Windows 开机启动     | ✅ 已实现 |
| Windows 锁屏阶段音乐播放 | ✅ 已实现 |
| Windows 登录状态检测   | ✅ 已实现 |
| 进入桌面后的欢迎语音       | ✅ 已实现 |
| Windows 启动集成     | ✅ 已实现 |

以上功能均已经在开发环境中进行测试。

---

# ⚠️ 尚未实现的功能

以下功能目前**尚未实现**，属于项目后续开发计划。

### 🎵 音频系统

* 音频库自动扫描
* 音乐库管理
* 语音库管理
* 更完整的音频管理系统
* 更完善的音量控制

### 🖥️ 什亭之匣界面

* 认证 / 登录界面
* 启动加载界面
* 什亭之匣主界面
* 设置界面
* 更多什亭之匣相关 UI 元素
* 更多系统控制功能

### 💬 交互系统

* 实时交互
* 实时对话
* 实时聊天系统
* 桌面交互系统
* 角色回应系统

### 👤 角色系统

* 阿罗娜角色模型
* 普拉娜角色模型
* 人物模型动画
* 人物模型表情管理
* 角色状态系统

### 📱 桃信 / 学生系统

* Momotalk / 桃信系统
* 学生消息系统
* 学生交流功能
* 角色通信功能

### 📚 其他功能

* 《蔚蓝档案》Wiki 相关功能
* 更多《蔚蓝档案》相关资源整合
* 更多什亭之匣风格的 Windows 控制功能

> 以上内容均属于未来计划，目前不代表已经实现。

---

# 🗺️ 开发路线图

## 第一阶段 —— 基础启动系统

* [x] Windows 开机启动
* [x] Windows 锁屏音乐
* [x] Windows 登录状态检测
* [x] 桌面欢迎语音
* [x] Windows 启动集成

**当前第一代版本主要完成这一阶段。**

---

## 第二阶段 —— 音频与程序系统

计划进一步完善：

* [ ] 音频库扫描
* [ ] 音乐库管理
* [ ] 语音库管理
* [ ] 音量控制
* [ ] 更完整的音频配置
* [ ] 音频播放管理

---

## 第三阶段 —— 交互系统

计划加入更加完整的交互能力：

* [ ] 实时交互系统
* [ ] 实时聊天
* [ ] 桌面交互
* [ ] 角色回应
* [ ] 用户与角色之间的实时交互

---

## 第四阶段 —— 人物模型系统

计划加入角色模型以及更加丰富的表现：

* [ ] 阿罗娜模型
* [ ] 普拉娜模型
* [ ] 人物动画
* [ ] 人物动作
* [ ] 人物模型表情管理
* [ ] 根据交互内容产生不同表情

---

## 第五阶段 —— 桃信 / 学生系统

计划进一步扩展角色交流：

* [ ] 桃信 / Momotalk 系统
* [ ] 学生消息系统
* [ ] 学生通信
* [ ] 角色对话
* [ ] 学生互动功能

---

## 第六阶段 —— 什亭之匣生态扩展

最终计划逐步加入：

* [ ] 更多什亭之匣 UI
* [ ] 更多系统控制元素
* [ ] 《蔚蓝档案》Wiki 相关功能
* [ ] 更多角色相关功能
* [ ] 更多 Windows 桌面整合功能

> 路线图会随着项目开发情况进行调整，具体功能与开发顺序可能发生变化。

---

# 🛠️ 技术栈

| 项目      | 技术      |
| ------- | ------- |
| 开发语言    | C#      |
| UI 框架   | WPF     |
| .NET 版本 | .NET 10 |
| 运行平台    | Windows |
| 音频框架    | NAudio  |
| OGG 支持  | NVorbis |

---

# 💻 开发环境要求

如果希望从源码运行本项目，需要准备：

* Windows
* .NET 10 SDK
* Visual Studio
* .NET 桌面开发相关组件
* WPF 开发环境

---

# 🚀 开始使用

## 1. 克隆项目

```bash
git clone https://github.com/<你的用户名>/ShittimEcho.git
```

进入项目目录：

```bash
cd ShittimEcho
```

---

## 2. 还原依赖

```bash
dotnet restore
```

---

## 3. 编译项目

```bash
dotnet build
```

---

## 4. 运行项目

```bash
dotnet run
```

> 由于本项目涉及 Windows 启动、登录状态以及系统音频等功能，实际运行效果可能受到 Windows 系统环境、权限以及本地配置影响。

---

# 📁 项目结构

项目目前采用较为清晰的功能模块划分。

```text
ShittimEcho/
│
├─ Core/
│  ├─ Audio/
│  ├─ Music/
│  └─ Voice/
│
├─ Windows/
│
├─ App.xaml
├─ App.xaml.cs
├─ MainWindow.xaml
├─ MainWindow.xaml.cs
├─ SettingsWindow.xaml
├─ SettingsWindow.xaml.cs
│
├─ ShittimEcho.csproj
└─ README.md
```

随着项目继续开发，项目结构也会根据功能模块进一步调整。

---

# 📸 截图与演示

项目的截图以及演示视频将在后续开发过程中逐步补充。

目前第一代版本主要展示：

```text
Windows 开机
     ↓
Windows 锁屏
     ↓
锁屏音乐
     ↓
用户登录
     ↓
音乐停止 / 淡出
     ↓
欢迎语音
     ↓
Windows 桌面
```

后续随着 UI、交互系统以及人物模型等功能加入，项目展示内容也会进一步扩展。

---

# 🔮 项目愿景

**什亭之匣·回响**并不只是一个简单的 Windows 音频播放器。

项目的长期目标，是逐步将「什亭之匣」的系统体验融入 Windows 桌面环境。

整体发展方向计划为：

```text
Windows 启动体验
        │
        ▼
Windows 桌面整合
        │
        ▼
音频系统
        │
        ▼
什亭之匣 UI
        │
        ▼
交互系统
        │
        ▼
人物模型
        │
        ▼
人物表情与动作
        │
        ▼
实时通信
        │
        ▼
完整的什亭之匣式 Windows 桌面体验
```

第一代版本只是整个项目的起点。

未来希望逐步让 Windows 不只是一个操作系统桌面，而能够拥有更加完整的「什亭之匣」式交互体验。

---

# 🤝 参与贡献

欢迎对本项目感兴趣的开发者提出：

* Bug 反馈
* 功能建议
* UI 设计建议
* 技术方案讨论
* Pull Request
* 项目改进建议

如果发现问题，建议在 GitHub Issues 中提供以下信息：

* Windows 版本
* .NET 版本
* 问题发生步骤
* 错误信息
* 相关日志
* 截图或视频

如果准备进行较大规模的功能修改，建议先通过 Issue 进行讨论。

---

# 📜 开源协议

本项目源代码计划采用 **MIT License** 开源。

详细内容请查看：

```text
LICENSE
```

---

# ⚠️ 版权与免责声明

**什亭之匣·回响（Shittim Chest: Echo）是一个非官方同人项目。**

本项目的创作灵感来源于《蔚蓝档案》以及其中的「什亭之匣」概念。

本项目与 **NEXON、NEXON Games、《蔚蓝档案》官方及相关权利持有者不存在官方关联、授权、赞助或合作关系。**

《蔚蓝档案》相关的：

* 角色
* 名称
* 商标
* 游戏素材
* 音乐
* 配音
* 人物模型
* 图片
* 其他相关内容

其版权及相关权利均归各自的权利持有人所有。

本仓库主要用于发布本项目自身的源代码以及原创开发内容。

本项目不会通过开源代码授予任何第三方素材的再分发权利。

如果用户自行向程序中添加音乐、语音、模型、图片或其他第三方资源，应确保相关资源来源合法，并拥有相应的使用权限。

---

# 💙 什亭之匣·回响

<p align="center">
  <strong>Shittim Chest: Echo</strong>
</p>

<p align="center">
  什亭之匣·回响
</p>

<p align="center">
  第一代版本已经完成核心启动功能。
</p>

<p align="center">
  未来还将继续扩展。
</p>

<p align="center">
  <strong>这只是开始。</strong>
</p>
