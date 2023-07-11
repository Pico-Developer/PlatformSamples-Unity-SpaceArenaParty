<h1 align="center">
<span>太空竞技场</span>
</h1>
<p align="center">
    <a href="./README.md">English</a> | 中文介绍
</p>

太空竞技场（Space Arena Party）是一个多人社交游戏 Demo，集成了 PICO 平台服务中的 “好友” 服务、“社交互动” 服务以及 “房间&匹配” 服务。你可以在 Demo 中体验以下能力：
- Avatar 和角色移动
- 创建和加入虚拟房间
- 实时多人互动
- 好友系统，包括查看好友列表、邀请好友
 
![image](https://github.com/Pico-Developer/PlatformSamples-Unity-SpaceArenaParty/assets/110143438/d2239b63-e0e7-4a67-9d2b-0cdb3bde2c3f)<h1 align="center">

## 环境要求

Unity 编辑器为 2021.3.22 版本，2022 版本暂不支持。

## 视频展示
以下为 Demo 展示视频。你可以通过视频浏览 Demo 场景并了解 Demo 的主要功能：

https://p9-arcosite.byteimg.com/obj/tos-cn-i-goo7wpa0wc/8dca694b2ba1435e9488902e407290e8

## 主要模块
Demo 主要由以下模块组成：
| 模块 | 说明 |
|---|---|
| Avatar | 用户在游戏内展示的虚拟形象。在 Demo 中，Avatar 可根据头戴和手柄的数据输入来展示不同的姿态，例如抬起手臂、转动头部等。此外，你也可以改变 Avatar 的皮肤颜色。 |
| 虚拟房间 | Demo 提供三个房间场景，分别是 Lobby、Blue Room 和 Orange Room，三个房间场景仅颜色不同，其他配置均相同。其中，Lobby 为启动游戏后用户默认加入的私人大厅，用户在体验过程中可加入或创建不同的房间。 |
| 控制面板 | 用户可以使用 LaunchPad 创建房间、加入房间、查看好友列表等。 |
| 多人游戏 | 用户可以在 Demo 内体验多人游戏能力，例如加入好友所在房间。 |
| 社交系统 | Demo 接入了 PICO “好友” 服务。用户登录后，可在 LaunchPad 上查看好友列表、查看好友在线状态、邀请好友。 |

## 体验流程
以下为 Demo 体验流程概览：
1. 将 Demo 项目下载至本地。
2. 创建 PICO 开发者帐号、组织和应用。
3. 开通匹配服务并创建匹配池。
4. 创建 Destinations。
5. 设置 App ID。
6. 完成 PC 端调试配置。
7. 在 Unity 编辑器内或头戴端体验 Demo。

## 调试多人游戏能力
以下为多人游戏能力调试方案概览:
- 使用 Unity 编辑器和 PICO 设备
- 使用多台 PICO 设备

## 了解更多
若想了解更多 Space Arena Party 项目的信息，包括详细的体验步骤说明、多人游戏调试方案说明、游戏流程等，参阅《[社交互动 - Demo](https://developer-cn.pico-interactive.com/document/unity/social-interaction-demo/)》文档。

## 贡献你的想法
欢迎为此 Demo 贡献你的想法。步骤如下：

1. Fork 该仓库。
2. 为你的功能或 Bug 修复创建一个新的分支。
3. 将你的改动提交至该分支。
4. 创建 Merge Request 并提供改动说明。
