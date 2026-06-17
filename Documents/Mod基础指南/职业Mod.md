# 职业Mod
[返回](./引言.md)

面向希望添加新职业、扩展已有职业技能或编写职业获得方式的开发者。

> **HPB 注意**：本文档描述 HPB 的 DSL 签名。与 BF 的关键差异：`WriteFree` → `WriteCompileTime`、`WriteEffect` 使用 `analyzerKey` + `ClapRoundClock`、防御使用 `DefenseEntity`、自定义效果/防御需通过 `[IsAnalyzer]` 注册。迁移指南见[架构升级说明](../架构升级说明.md#8-mod-开发者迁移指南)。

## 总体流程

1. 创建 `.NET 8` 类库项目，引用 `BlacksmithCore`
2. 编写 `MainProfession` 或 `ProfessionModifier`
3. 构建脚本发布 → 编译好的 DLL 放入子目录 → `mod.json` 声明路径 → 启动

## 核心类型

### ISkillContext

```csharp
public interface ISkillContext
{
    ISudoOperations SudoOperations { get; }
    string SkillName { get; }
    Community Self { get; }
    int Param { get; }           // 前端 -p 标志
    string StringParam { get; }  // 前端 -s 标志
    IReadOnlyList<(ISkillContext, ISkillContext)> SkillHistory { get; }
    IGameMetadata GameMetadata { get; }
}
public interface ISudoOperations
{
    GameInstance DeepCopy(int preRounds = 0);
    bool IsPlayer(Community community);
    IGameMetadata GameMetadata { get; }
}
public interface IGameMetadata
{
    IReadOnlySet<string> MainProfessionSkillNames { get; }
    IReadOnlySet<string> EquipmentSkillNames { get; }
}
```

常用：`sc.Self`（`Community`）、`sc.Self.Focus`（`Body`）、`sc.Param`、`sc.StringParam`、`sc.SudoOperations`、`sc.GameMetadata`。

### Community 与 Body

`Body` 通过 `Get<T>()` 访问各组件（详见[项目架构 - 组件一览](../项目架构.md#组件一览)）：

- `Get<Health>().HP` / `.MHP` / `.GainHP()` / `.LoseHP()`
- `Get<Resource>().Check(type, need)` / `.Use()` / `.Gain()` / `.Query()` / `.QueryAll()`
- `Get<Skill>().AddPackage()` / `.AddSkill()` / `.RemoveSkill()`

## 技能配对规则

系统自动收集 `private` 实例方法配对：

- `XxxCheck(ISkillContext)` → `bool`
- `Xxx(ISkillContext)` → `IDSLSourceFile`

技能名转小写。**必须返回 `IDSLSourceFile`，不能返回具体类 `DSL.SourceFile`**——否则编译报错。

## SkillMetadata 技能元数据

HPB 提供更丰富的元数据属性（位于 `BlacksmithCore.Infra.Attributes.SkillMetadata`）：

```csharp
using BlacksmithCore.Infra.Attributes.SkillMetadata;

[IsProfessionSkill]  // 标记为职业技能——通常是转职入口
private IDSLSourceFile HolyBook(ISkillContext sc) { ... }

[IsEquipmentSkill]   // 标记为装备技能
private IDSLSourceFile StarRifle(ISkillContext sc) { ... }

[HasAttack(3)]       // 标记为攻击技能，携带攻击力信息
[Labels(Impression.Aggressive, Strength.Strong)]  // 技能标签
private IDSLSourceFile Slash(ISkillContext sc) { ... }
```

HPB 新增属性：`[HasAttack(float)]`、`[HasDefense]`、`[HasResource]`、`[HasRecovery]`、`[HasBuff]`、`[Labels(Impression, Strength)]`、`[IsInfinite]`。

## DSL 基础用法

```csharp
using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;

private IDSLSourceFile SomeSkill(ISkillContext sc)
{
    Pen pen = sf => sf
        .UseResource(1, ResourceType.Instance.Iron())
        .WriteAttack(3, AttackType.Instance.Physical());

    return DSL.Create(sc.Self, pen);
}
```

常用语句：

| 语句 | 签名（HPB） | 说明 |
|---|---|---|
| `WriteAttack` | `(int power, AttackType.CEValue, int delayRounds=0, float aPFactor=1f, string analyzerKey=...)` | 攻击 |
| `WriteDefense` | `(int power, DefenseEntity defense, int delayRounds=0, string analyzerKey=...)` | 防御 |
| `WriteResource` | `(float power, ResourceType.CEValue type, int delayRounds=0, string analyzerKey=...)` | 资源 |
| `WriteRecovery` | `(int power)` | 回复 HP |
| `WriteEffect` | `(EffectType.CEValue, EffectTargetType.CEValue, ClapRoundClock entityClock, string analyzerKey, int delayRounds=0, float power=0)` | 效果 |
| `WriteCompileTime` | `(Action<Community> action)` | 编译时自由动作 |
| `UseResource` | `(float need, ResourceType.CEValue type, bool ifCommonOnly=false)` | 消耗资源 |
| `LoseHP` / `LoseMHP` | `(int loss)` | 扣血/扣最大 HP |
| `AddMark` | `(EffectEntity entity)` | 添加标记效果 |
| `WithBloodSuck` | `(float percent)` | 攻击吸血 |
| `WithInterupt` | `()` | 攻击打断（移除铁/金铁/魔力资源数据） |
| `WithRuntime` | `(AttackStage.CEValue stage, string analyzerKey)` | 攻击阶段钩子 |
| `WithComplieTime` | `(Action<AttackAnalyzableData> modifier)` | 编译时修改攻击数据 |

### WriteCompileTime 与 Move()

> **HPB 变更**：`WriteFree` 被 `WriteCompileTime` 替代。语义保持一致——编译时执行的自由动作。

`SourceFile.Move(newOwner, filter)`：更换 `_owner`，按 `SentenceType` 过滤剥离指定类型的句子。用于 Association 模式——将目标技能的 DSL 转移到自己的 SourceFile 中。

`UseResource`、`LoseHP`、`LoseMHP` 内部是 `WriteCompileTime(...)`。

### 自定义效果（HPB 新增）

```csharp
// 1. 定义 Analyzer
[IsAnalyzer(AnalyzerType.DSL)]
public static void MyEffect(Community player, Community enemy, IAnalyzableData data)
{
    var effect = (EffectAnalyzableData)data;
    // 效果逻辑：例如对 enemy 造成伤害
    enemy.Focus.Get<Health>().LoseHP((int)effect.Power);
}

// 2. 在 WriteEffect 中引用
Pen pen = sf => sf.WriteEffect(
    EffectType.Instance.AfterAnalyzableDataWritten(),
    EffectTargetType.Instance.Enemy(),
    new ClapRoundClock(remainingRounds: 3),  // entityClock
    analyzerKey: nameof(MyEffect),
    power: 5
);
```

### 自定义防御（HPB 新增）

```csharp
// 1. 定义 DefenseEntity 子类（仅数据声明）
public class MyCustomDefense : DefenseEntity
{
    public override string AnalyzerKey { get; init; } = nameof(MyDefenseAnalyzer);
    public override DefenseType.CEValue Type { get; init; } = MyDefenseType.Instance.Value;
    public override int Power { get; set; } = 0;
    public override ClapRoundClock Clock { get; init; } = new(isInfinite: true);
}

// 2. 注册 Analyzer
[IsAnalyzer(AnalyzerType.Defense)]
public static void MyDefenseAnalyzer(Community player, Community enemy, 
    DefenseEntity defense, AttackAnalyzableData attackData)
{
    var damage = Math.Min(attackData.Power, defense.Power);
    attackData.Power -= damage;
    attackData.TotalDamage += damage;
}

// 3. 在 DSL 中使用
Pen pen = sf => sf.WriteDefense(5, new MyCustomDefense());
```

## 示例一：最小主职业

```csharp
public class MyProfession : MainProfession
{
    private bool JokeCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Get<Health>().HP > 5
            && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
    }

    private IDSLSourceFile Joke(ISkillContext sc)
    {
        Pen pen = sf => sf
            .UseResource(1, ResourceType.Instance.Iron())
            .WriteRecovery(1)
            .WriteAttack(3, AttackType.Instance.Physical())
            .WriteAttack(3, AttackType.Instance.Magical())
                .WithBloodSuck(0.5f);

        return DSL.Create(sc.Self, pen);
    }
}
```

## 示例二：Common 修改器提供转职入口

```csharp
[IsProfessionModifier(nameof(Common))]
public partial class CommonModifier : ProfessionModifier
{
    private bool MyProfessionCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
    }

    [IsProfessionSkill]
    private IDSLSourceFile MyProfession(ISkillContext sc)
    {
        sc.Self.Focus.Get<Skill>().AddPackage(new MyProfession());

        Pen pen = sf => sf
            .UseResource(2, ResourceType.Instance.Iron())
            .WriteCompileTime(source => Common.ExcludeAllProfessions(source));

        return DSL.Create(sc.Self, pen);
    }
}
```

> **HPB 变更**：`WriteFree(action, canMove: false)` → `WriteCompileTime(action)`。`WriteCompileTime` 在 `Move()` 时默认随 filter 剥离。

## Modifier 访问目标职业私有状态（UnsafeAccessor）

机制不变——`ModifierBindingGenerator` 两阶段源生成器使 Modifier 零开销直接读写目标 MainProfession 的私有字段。

```csharp
[IsProfessionModifier(nameof(Common))]
public partial class CommonModifier : ProfessionModifier
{
    // _pending 是 Driver/Cannon 的 private 字段！
    // 源生成器自动生成了对应的 public ref 字段和 Bind() 实现

    [IsProfessionSkill]
    private IDSLSourceFile WineGlass(ISkillContext sc)
    {
        sc.Self.Focus.Get<Skill>().AddPackage(new(new WineGlass()));
        return DSL.Create(sc.Self, pen);
    }
}
```

## 被动技能

重写 `MainProfession` 的 `PassiveSkillImpl(ISkillContext sc)` 方法（`virtual`）。`PassiveSkill` 已改为 `sealed override`，内部调用 `PassiveSkillImpl` 后自动设置 `IsPassive = true`。

```csharp
public override IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
{
    Pen pen = sf => sf.WriteDefense(1, new RealReduction());
    return DSL.Create(sc.Self, pen);
}
```

## 注意事项

1. `AttackType` 法术攻击是 `Magical()`，不是 `Magic()`
2. 通过 `body.Get<Health>()` 操作生命值，不是直接在 Body 上调用
3. 资源和攻击类型通过 `Instance` 获取
4. 技能名全小写，手动 `AddSkill`/`RemoveSkill` 也用全小写
5. 多个 Mod 同名技能 → 后写覆盖；同名职业 → 抛异常
6. `Common` 是真实职业包，修改它是提供转职入口的最常见方式
7. **HPB**：攻击/防御 Power 为 `int`，资源 Power 为 `float`
8. **HPB**：自定义防御继承 `DefenseEntity`（不是 `DefenseBase`），不需要实现 `Work()` 方法
9. **HPB**：自定义效果使用 `[IsAnalyzer(AnalyzerType.DSL)]` 注册，`WriteEffect` 引用 `analyzerKey`
10. **HPB**：`Compile(JudgeRuleManager?)` 替代 `Compile(Judger?)`

## 参考

- `Project/Blacksmith/ModExamples/HolyBookMod/`
- `Project/Blacksmith/ModExamples/PhantomBookMod/` — Association 模式
- [架构升级说明 - Mod 开发者迁移指南](../架构升级说明.md#8-mod-开发者迁移指南) — BF → HPB 迁移模式
