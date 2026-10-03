# 咸鱼喵喵·喵露露终端

<p align="center">
  <strong>Nyaruru Fishy Fight</strong>
</p>

<p align="center">
  基于 C#、WPF 与 .NET 10 开发的 Windows 桌面锁屏音乐工具
</p>

<p align="center">
  <img src="./ShittimLoadingLogo.png" alt="咸鱼喵喵 Logo" width="200">
</p>

<h1 align="center">咸鱼喵喵·喵露露终端</h1>

<p align="center">
  <strong>Nyaruru Fishy Fight</strong>
</p>

<p align="center">
  一个基于 <strong>C# · WPF · .NET 10</strong> 开发的 Windows 桌面项目
  <br>
  魔改自 <a href="https://github.com/Soymilk-520/ShittimEcho">什亭之匣·回响</a>，主题替换为《咸鱼喵喵》
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-.NET%2010-512BD4?style=for-the-badge&logo=csharp&logoColor=white" alt="C# .NET 10">
  <img src="https://img.shields.io/badge/WPF-Windows-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="WPF Windows">
  <img src="https://img.shields.io/badge/BGM-60%20tracks-FF69B4?style=for-the-badge" alt="60 BGM tracks">
</p>

<br>

---

## 📖 项目简介

**咸鱼喵喵·喵露露终端** 是基于 **什亭之匣·回响（Shittim Chest: Echo）** 魔改的 Windows 桌面工具。

将原项目的蔚蓝档案/什亭之匣主题全部替换为《咸鱼喵喵》（Nyaruru Fishy Fight）主题：

- 🎨 粉色系 UI 主题（原冰蓝色）
- 🖼️ 喵露露主背景、水晶 CG 终端背景、Q版图标与头像
- 🎵 60 首游戏原版 BGM（从 APK 解密提取）
- 📝 全部界面文字替换为咸鱼喵喵相关

核心功能保持不变：Windows 锁屏音乐 + 登录欢迎语音。

---

## ✨ 功能

### 🔒 Windows 锁屏音乐

Windows 启动后，程序在登录/锁屏界面阶段播放指定音乐。

```text
Windows 启动 → 登录/锁屏界面 → 播放锁屏音乐
```

### 🔊 登录欢迎语音

用户登录进入桌面后，停止锁屏音乐并播放欢迎语音。

```text
用户登录 → 检测进入桌面 → 淡出音乐 → 播放欢迎语音
```

### 🎵 60 首游戏 BGM

内置 60 首《咸鱼喵喵》原版 OGG 音频，包括：

- **场景音乐**：喵露镇、浮游高塔、黑森林、冰原、失落世界、魔法学校、星之海…
- **Boss 战**：喵露露、莉莉亚、璃音（多阶段）、德古拉、樱花、Theia…
- **其他**：主题曲、回忆、睡前故事、小游戏、DLC、俄罗斯方块彩蛋…

---

## 🚀 快速开始

### 下载预编译版本

1. 打开 [Actions 页面](https://github.com/lelecz/ShittimEcho/actions)
2. 点击最新一次成功的 run
3. 页面底部下载 `ShittimEcho-Release` 压缩包
4. 解压后运行 `ShittimEcho.exe`

### 配置音乐目录

程序默认从 `%LOCALAPPDATA%\ShittimEcho\Music` 读取音乐。60 首 BGM 已打包在 exe 同目录的 `Music/` 文件夹中，需要手动指定：

1. 打开程序 → 设置
2. 将音乐目录改为 exe 旁边的 `Music` 文件夹
3. 保存，锁屏时即会随机播放

---

## 🛠️ 从源码编译

### 环境要求

- Windows 10/11
- .NET 10 SDK
- Visual Studio（含 .NET 桌面开发组件）

### 编译步骤

```bash
git clone https://github.com/lelecz/ShittimEcho.git
cd ShittimEcho
dotnet restore
dotnet build -c Release
```

### GitHub Actions 自动编译

项目配置了 `.github/workflows/build.yml`，每次 push 到 main 分支会自动：

1. 编译 Release 版本
2. 冒烟测试（启动 exe，等待 10 秒确认不崩溃）
3. 上传编译产物

---

## 📁 项目结构

```text
ShittimEcho/
├─ Core/
│  ├─ Audio/          # 音频引擎、解码器、音乐管理
│  ├─ Music/          # 音乐库扫描、路径管理
│  ├─ Voice/          # 语音库
│  └─ Diagnostics/    # 启动日志、系统检测
├─ Windows/           # 会话/电源监控
├─ Music/             # 60 首 BGM（OGG）
├─ App.xaml / .cs     # 应用入口
├─ MainWindow.xaml    # 主窗口（终端界面）
├─ SettingsWindow.xaml# 设置窗口
├─ ShittimMainBackground.jpg   # 主背景（喵露露宣传图）
├─ ShittimBackground.png       # 终端背景（水晶 CG）
├─ ShittimLoadingLogo.png      # 加载 Logo（双角色主视觉）
├─ ShittimEchoIcon.ico         # 应用图标（Q版喵露露）
├─ CreatorAvatar.jpeg          # 制作人头像
└─ ShittimEcho.csproj
```

---

## 🎨 魔改内容

| 项目 | 原版（什亭之匣） | 魔改版（咸鱼喵喵） |
|------|-----------------|-------------------|
| 主背景 | 蔚蓝档案风格 | 喵露露粉色宣传图 1280×720 |
| 终端背景 | 深蓝科技风 | 紫色水晶 CG 1280×720 |
| 加载 Logo | 什亭之匣标识 | 咸鱼喵喵双角色主视觉 500×266 |
| 应用图标 | 原版图标 | Q版喵露露生无可恋脸（6尺寸 ICO） |
| 制作人头像 | 原版 | Q版喵露露脸 400×400 |
| 主题色 | 冰蓝/深蓝 | 粉色/玫红系（218 处替换） |
| 程序名 | 什亭之匣·回响 | 咸鱼喵喵·喵露露终端 |
| 英文名 | Shittim Chest: Echo | Nyaruru Fishy Fight |
| BGM | 无（需自行添加） | 60 首游戏原版 |

---

## 🔧 技术栈

| 项目 | 技术 |
|------|------|
| 开发语言 | C# |
| UI 框架 | WPF |
| .NET 版本 | .NET 10 |
| 运行平台 | Windows |
| 音频框架 | NAudio 3.1.0 |
| OGG 解码 | NAudio.Vorbis 3.0.0 |

---

## ⚠️ 版权与免责声明

**咸鱼喵喵·喵露露终端是一个非官方同人魔改项目。**

- 本项目基于 [什亭之匣·回响](https://github.com/Soymilk-520/ShittimEcho) 魔改，原项目版权归原作者所有
- 《咸鱼喵喵》（Nyaruru Fishy Fight）相关的角色、名称、商标、游戏素材、音乐、图片等版权归各自权利持有人所有
- 本项目仅用于学习与个人使用，不用于商业用途
- 内置 BGM 从游戏 APK 中解密提取，仅供个人欣赏

---

## 🤝 原项目

- **原项目**：[Soymilk-520/ShittimEcho](https://github.com/Soymilk-520/ShittimEcho)
- **原作者**：Soymilk-520（Irst_春川）
- **魔改**：lelecz

<p align="center">
  <strong>咸鱼喵喵·喵露露终端</strong>
  <br>
  Nyaruru Fishy Fight
  <br>
  <em>基于什亭之匣·回响魔改</em>
</p>
