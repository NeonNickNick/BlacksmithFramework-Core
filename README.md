# HighPerformanceBlacksmith

围绕《打铁》(Blacksmith) 规则构建的双人同步回合制对战框架，基于 .NET 8。采用 **AnalyzableData + AnalyzerRegistry** 架构，将数据与行为解耦，在Release模式下约有 **9-10 倍**提速。

## 项目结构

| 项目 | 说明 |
|---|---|
| `Clap/ClapSourceGenerators` | Roslyn 增量源生成器，编译时生成技能配对和分析器注册代码 |
| `BlacksmithCore` | 核心引擎。领域模型、技能 DSL、判定引擎、动态规则、AI 策略、Mod 加载器、基础单元（时钟、Mark 系统等） |
| `BlacksmithClient` | ASP.NET Core 本地站点，托管前端页面，通过 `/api/*` 端点操作同进程 `GameInstance` |
| `BlacksmithServer` | 多人服务器 |
| `ModExamples` | 示例 Mod —— 圣书、幻书、炼药锅、弩、武僧、预言者、酒杯 |

所有项目目标框架 `net8.0`。

目录结构：

```
Project/
├── Clap/
│   └── ClapSourceGenerators/   # Roslyn 增量源生成器
├── Blacksmith/
│   ├── BlacksmithClient/       # Web 宿主（入口）
│   ├── BlacksmithCore/         # 游戏引擎
│   │   ├── Infra/
│   │   │   ├── Unit/           # ClapRoundClock, ClapSharedFlag
│   │   │   ├── Utils/          # DllLoader, ModLoader
│   │   │   ├── Models/         # Body, Community, Health, Skill, TurnContext 等
│   │   │   │   └── Components/
│   │   │   │       ├── AnalyzableDatas/  # Attack/Defense/Resource/Effect 数据
│   │   │   │       └── AnalyzedObjects/  # DefenseEntity, EffectEntity
│   │   │   ├── DSL/            # DSLforSkillLogic, AnalyzerRegistry, StandardAnalyzers
│   │   │   ├── Judgement/      # JudgeRuleManager, JudgeStage, ModifierCallback 等
│   │   │   ├── Profession/     # SkillPackageBase, MainProfession, ProfessionModifier
│   │   │   ├── Attributes/     # SkillMetadata, Analyzer, BlacksmithEnum, Profession
│   │   │   └── Enum/           # BlacksmithEnum, BlacksmithEnumRegistry
│   │   ├── Specific/           # 内置职业 & 防御
│   │   │   ├── BuiltInProfessions/  # Common, Cannon, Driver, Warlock, BloodSigil, Alchemy, Lancer
│   │   │   └── Defense/             # CommonArmor, CommonReduction, ThornReduction 等
│   │   └── Driver/             # GameInstance, AI 策略
│   ├── BlacksmithClient/
│   ├── BlacksmithServer/
│   └── ModExamples/            # 示例 Mod
```

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

## 文档导航

| 文档 | 面向 |
|---|---|
| [项目架构](./Documents/项目架构.md) | 维护者、Mod 开发者——完整架构说明 |
| [判定流程](./Documents/判定流程.md) | 了解从技能声明到结算完毕的完整执行链路 |
| [Mod 基础指南](./Documents/Mod基础指南/引言.md) | 新手 Mod 开发者——枚举扩展、职业创建、DSL 用法 |
| [Mod 进阶指南](./Documents/Mod进阶指南/引言.md) | 进阶 Mod 开发者——动态注册、判定实现 |
| [高级技能模式](./Documents/高级技能模式.md) | 复杂技能实现——Mark 系统、数据冒险、自定义效果/防御 |
| [Blacksmith 规则](./Documents/规则/BlacksmithRuleCN.md) | 游戏规则与职业数据 |
