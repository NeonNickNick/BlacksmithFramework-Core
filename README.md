# BlacksmithFrame-Core

围绕《打铁》(Blacksmith) 规则构建的双人同步回合制专用对战框架核心，基于 .NET 8。采用将数据与行为完全解耦的架构，相比于过往项目有**9-10 倍**提速。

## 项目结构

| 项目 | 说明 |
|---|---|
| `SourceGenerators/BlacksmithSourceGenerators` | Roslyn 增量源生成器，编译时生成技能配对和分析器注册代码 |
| `BlacksmithCore` | 核心引擎。领域模型、技能 DSL、判定引擎、动态规则、AI 策略、Mod 加载器、基础单元（时钟、Mark 系统等） |
| `BlacksmithClient` | ASP.NET Core 本地站点，托管前端页面，通过 `/api/*` 端点操作同进程 `GameInstance` |
| `BlacksmithServer` | 多人服务器 |
| `ModExamples` | （尚待迁移）示例 Mod —— 圣书、幻书、炼药锅、弩、武僧、预言者、酒杯 |

所有项目目标框架 `net8.0`。

## 运行方式

```powershell
# 发布纯净版（不含 Mod）
.\BlacksmithPure.cmd

# 发布带示例 Mod 版
.\BlacksmithWithMods.cmd

# 运行
.\BlacksmithPure\BlacksmithClient.exe
# 或
.\BlacksmithWithMods\BlacksmithClient.exe
```

```bash
# Linux 服务器
bash BlacksmithServer.sh
```

`.cmd` 脚本执行 `dotnet publish -c Release`。Mod DLL 通过 `.blacksmith/mod.json` 配置加载。

## 对战模式

| 模式 | 说明 |
|---|---|
| **Manual** | 双方技能均由前端手动输入 |
| **BloodSigil** | 启发式规则 AI |
| **General** | 基于 MCTS 搜索的通用 AI |

## 内置职业

| 职业 | 说明 |
|---|---|
| **Common** | 通用技能，可转职到所有其他职业 |
| **Cannon** | 钢炮。高物理伤害，穿甲弹，三连击叠加 |
| **Driver** | 驱动器。被动真实伤减，时空资源转换 |
| **Warlock** | 术士。魔法职业，多回合延迟攻击 |
| **BloodSigil** | 鲜血印记。以生命换取伤害与吸血 |
| **Alchemy** | 炼金（装备技能）。Iron 转 Gold_Iron |
| **Lancer** | 战矛。纹章系统，蓄力反击 |

`ModExamples/` 提供了 7 个示例 Mod 供参考。