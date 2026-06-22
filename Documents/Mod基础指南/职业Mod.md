# 职业Mod
[返回](./引言.md)

面向希望添加新职业、扩展已有职业技能或编写职业获得方式的开发者。

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

系统自动收集方法配对：

- `XxxCheck(ISkillContext)` → `bool`
- `Xxx(ISkillContext)` → `IDSLSourceFile`

方法可以是 `private` 实例方法或 `private static` 方法。技能名转小写。**必须返回 `IDSLSourceFile`，不能返回具体类 `DSL.SourceFile`**——否则源生成器报编译错误。

## SkillMetadata 技能元数据

元数据属性位于 `BlacksmithCore.Infra.Attributes.SkillMetadata`：

```csharp
using BlacksmithCore.Infra.Attributes.SkillMetadata;

[IsProfessionSkill]  // 标记为转职入口技能
private IDSLSourceFile HolyBook(ISkillContext sc) { ... }

[IsEquipmentSkill]   // 标记为装备技能
private IDSLSourceFile StarRifle(ISkillContext sc) { ... }

[HasAttack(3)]       // 标记为攻击技能，携带攻击力信息
[Labels(Impression.Aggressive, Strength.Strong)]  // 技能标签
private IDSLSourceFile Slash(ISkillContext sc) { ... }
```

完整属性列表：

| 属性 | 说明 |
|---|---|
| `[HasAttack(float power)]` | 标记为攻击技能 |
| `[HasDefense]` | 标记为防御技能 |
| `[HasResource]` | 标记为资源技能 |
| `[HasRecovery]` | 标记为恢复技能 |
| `[HasBuff]` | 标记为 Buff 技能 |
| `[IsProfessionSkill]` | 标记为转职入口技能 |
| `[IsEquipmentSkill]` | 标记为装备技能 |
| `[Labels(Impression, Strength)]` | 技能标签（印象 + 强度） |
| `[IsInfinite]` | 标记为无限持续技能 |

---

## DSL 参考

### Pen 模式

```csharp
using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
using DSL = BlacksmithDSL;

private IDSLSourceFile SomeSkill(ISkillContext sc)
{
    Pen pen = sf => sf
        .UseResource(1, ResourceType.Instance.Iron())
        .WriteAttack(3, AttackType.Instance.Physical());

    return DSL.CreateBy(pen);
}
```

`DSL.CreateBy(pen)` 等价于 `pen(new SourceFile())`。

### Write 语句完整参考

#### WriteAttack

```csharp
public AttackFile WriteAttack(
    int power,                    // 攻击力（整数）
    AttackType.CEValue attackType,
    int delayRounds = 0,
    float APFactor = 1f,         // 穿甲因子
    string analyzerKey = nameof(StandardAnalyzers.DefaultAttack),
    Func<bool>? ifUndo = null    // 撤销条件
)
```

#### WriteDefense

```csharp
public DefenseFile WriteDefense(
    DefenseEntity defense,       // 防御实体（Power 在实体上设置）
    int delayRounds = 0,
    string analyzerKey = nameof(StandardAnalyzers.DefaultDefense),
    Func<bool>? ifUndo = null
)
```

注意：`power` 直接在 `DefenseEntity` 上设置（如 `new CommonArmor { Power = 5 }`）。

#### WriteResource

```csharp
public ResourceFile WriteResource(
    float power,                 // 资源量（浮点）
    ResourceType.CEValue type,
    int delayRounds = 0,
    string analyzerKey = nameof(StandardAnalyzers.DefaultResource),
    Func<bool>? ifUndo = null
)
```

#### WriteEffect

```csharp
public EffectFile WriteEffect(
    EffectType.CEValue type,           // 触发阶段（如 AfterAnalyzableDataWritten）
    EffectTargetType.CEValue targetType, // 己方或敌方
    ClapRoundClock entityClock,        // 效果实体的生命周期
    string analyzerKey,                // 分析器键（无默认值，必须指定）
    int delayRounds = 0,
    float power = 0,
    Func<bool>? ifUndo = null
)
```

#### WriteRecovery

```csharp
public RecoveryFile WriteRecovery(
    int power,
    Func<bool>? ifUndo = null
)
```

直接回复 HP（编译时执行，不经过分析器）。

### WriteFree 与便捷方法

```csharp
public SourceFile WriteFree(Action<Community> action, Func<bool>? ifUndo = null)
```

以下便捷方法内部实现为 `WriteFree`：

| 方法 | 说明 |
|---|---|
| `UseResource(need, type)` | 消耗资源 |
| `LoseHP(loss)` | 扣除 HP |
| `LoseMHP(loss)` | 扣除最大 HP |
| `AddMark(markName)` | 写入标记 |
| `TakeMark(markName, out Lazy<int>)` | 取出标记并计数 |
| `CountMark(markName, out Lazy<int>)` | 只计数不移除 |
| `RegistCallbackOnJudge(callbacks)` | 注册动态判定回调 |

### AttackFile 链式方法

#### WithModify

```csharp
public AttackFile WithModify(Action<AttackAnalyzableData> modifier)
```

修改同一回合最近写入的 `AttackAnalyzableData`。作为修辞挂载到对应 WriteAttack 之后执行。

#### WithCallback

```csharp
public AttackFile WithCallback(AttackStage.CEValue stage, string analyzerKey)
```

在攻击结算的特定阶段触发分析器。三个阶段：

| AttackStage | 触发时机 |
|---|---|
| `OnHitArmorFirstTime` | 首次命中护甲时 |
| `OnHitBody` | 穿透所有防御后、扣血前 |
| `OnEnd` | 攻击结算完毕后 |

#### WithBloodSuck

```csharp
public AttackFile WithBloodSuck(float percent)
```

攻击结算后按 `TotalDamage * percent` 吸血。等价于 `WithModify` + `WithCallback(OnEnd, DefaultBloodSuck)`。

#### WithInterupt

```csharp
public AttackFile WithInterupt()
```

首次命中护甲时移除对手的 Iron/Gold_Iron/Magic 资源数据。等价于 `WithCallback(OnHitArmorFirstTime, DefaultInterupt)`。

### ifUndo 撤销机制

所有 Write 方法和便捷方法都接受可选的 `ifUndo` 参数。当 `Intent.Execute` 执行到该 Sentence 时，若 `ifUndo()` 返回 `true`，该语句被跳过。

```csharp
.WriteAttack(3, AttackType.Instance.Physical(), ifUndo: () => someCondition)
```

---

## Mark 系统

Mark 是存储于 Effect 组件的无限期 `EffectEntity`（`IsMark = true`），按 `AnalyzerKey`（即 markName）标识。它是跨回合状态管理的核心机制，替代了传统的状态变量。

### DSL 链式方法（声明时注册，执行时生效）

**AddMark** — 写入一个标记：
```csharp
sf.AddMark("MyMark")
```

**TakeMark** — 取出所有同名标记并返回数量：
```csharp
sf.TakeMark("MyMark", out Lazy<int> layerNum)

// 批量版本
sf.TakeMark(new HashSet<string> { "MarkA", "MarkB" }, out Lazy<IReadOnlyDictionary<string, int>> layerNums)
```

**CountMark** — 只计数不移除：
```csharp
sf.CountMark("MyMark", out Lazy<int> layerNum)
```

### Body 扩展方法（立即执行）

在 Analyzer 或 JudgeCallback 内部（已在执行阶段）可使用 Body 扩展方法：

```csharp
body.AddMark("MyMark")                      // 立即写入
int count = body.CountMark("MyMark")        // 立即计数
int taken = body.TakeMark("MyMark")         // 立即取出
```

### ★ 数据冒险

`out Lazy<T>` 是一个延迟求值包装器。其值仅在 `Intent.Execute` 执行到达对应 Sentence 时才可用。**在 Pen 声明阶段或 Sentence 尚未执行时访问 `.Value` 会抛出 `InvalidOperationException`。**

**错误**——在声明阶段访问：
```csharp
Pen pen = sf =>
{
    sf.TakeMark("myMark", out var layerNum);
    int count = layerNum.Value;  // ❌ 抛出异常：标记数据尚未计算
    return sf.WriteAttack(count, AttackType.Instance.Physical());
};
```

**正确**——在延迟执行的 lambda 中访问：
```csharp
Pen pen = sf => sf
    .TakeMark(nameof(TripleStrike), out var layerNum)
    .WriteAttack(11, AttackType.Instance.Physical())
        .WithModify(last => last.Power += layerNum.Value);  // ✅ 执行到达此处时 TakeMark 已完成
```

`WithModify` 作为修辞被插入到 WriteAttack 之后执行。在 `WithModify` 的 lambda 中，`TakeMark` 的 Sentence 已经执行完毕，`layerNum.Value` 安全可用。

同理，`ifUndo` 的 lambda 也在执行阶段求值，可安全访问 Mark 数据：

```csharp
.TakeMark(new HashSet<string> { "FireMark", "IceMark" }, out var layerNums)
.WriteAttack(3, AttackType.Instance.Physical(),
    ifUndo: () => layerNums.Value["FireMark"] <= 0)   // ✅ 执行时求值
```

---

## 自定义效果

效果通过 `[IsAnalyzer(AnalyzerType.DSL)]` 注册，在 `WriteEffect` 中通过 `analyzerKey` 引用：

```csharp
// 1. 定义 Analyzer
[IsAnalyzer(AnalyzerType.DSL)]
public static void MyEffect(Community player, Community enemy, IAnalyzableData data)
{
    var effect = (EffectAnalyzableData)data;
    // 效果逻辑：例如对 enemy 造成持续伤害
    enemy.Focus.Get<Health>().LoseHP((int)effect.Power);
}

// 2. 在 DSL 中引用
Pen pen = sf => sf.WriteEffect(
    EffectType.Instance.AfterAnalyzableDataWritten(),
    EffectTargetType.Instance.Enemy(),
    new ClapRoundClock(remainingRounds: 3),  // entityClock：持续 3 回合
    analyzerKey: nameof(MyEffect),
    power: 5
);
```

`entityClock` 控制效果的持续时间。效果触发阶段由 `EffectType` 决定（如 `AfterAnalyzableDataWritten`、`AfterTransport`、`AfterResult` 等）。`TargetType` 决定作用对象（`Self` 或 `Enemy`）。

## 自定义防御

防御通过 `DefenseEntity` 子类（纯数据声明）+ `[IsAnalyzer(AnalyzerType.Defense)]` 注册：

```csharp
// 1. 扩展 DefenseType 枚举（如果需要新类型）
[IsBlacksmithEnumModifier]
public static class DefenseExtension
{
    [IsBlacksmithEnumMember(64)]
    public static DefenseType.CEValue MyArmor(this DefenseType dt) => DefenseType.GetCEValue();
}

// 2. 定义 DefenseEntity 子类（仅数据声明——无需 Work() 方法）
public class MyCustomDefense : DefenseEntity
{
    public override string AnalyzerKey { get; init; } = nameof(MyDefenseAnalyzer);
    public override DefenseType.CEValue Type { get; init; } = MyArmorType;
    public override int Power { get; set; } = 0;
    public override ClapRoundClock Clock { get; init; } = new(isInfinite: true);
}

// 3. 注册防御分析器
[IsAnalyzer(AnalyzerType.Defense)]
public static void MyDefenseAnalyzer(Community player, Community enemy, 
    DefenseEntity defense, AttackAnalyzableData attackData)
{
    var damage = Math.Min(attackData.Power, defense.Power);
    attackData.Power -= damage;
    attackData.TotalDamage += damage;
}

// 4. 在 DSL 中使用
Pen pen = sf => sf.WriteDefense(new MyCustomDefense { Power = 5 });
```

内置防御分析器参考（可直接复用）：

| AnalyzerKey | 行为 |
|---|---|
| `nameof(StandardAnalyzers.DefaultArmor)` | 护甲吸收，值同步减少 |
| `nameof(StandardAnalyzers.DefaultReduction)` | 减伤，值不减少 |
| `nameof(StandardAnalyzers.ThornReduction)` | 吸收 + 反弹 50% 魔法伤害 |
| `nameof(StandardAnalyzers.MagicalImmunity)` | 魔法免疫 |
| `nameof(StandardAnalyzers.PhysicalImmunity)` | 物理免疫 |
| `nameof(StandardAnalyzers.PercentageReduction)` | 百分比减伤 |

---

## Dynamic Registration 动态注册

技能可以向判定管线注册临时回调，在特定阶段插入额外逻辑：

```csharp
[IsAnalyzer(AnalyzerType.JudgeCallback)]
public static void MyCallback(Community player, Community enemy) { ... }

Pen pen = sf => sf
    .RegistCallbackOnJudge(new()
    {
        new ModifierCallback()
        {
            AnalyzerKey = nameof(MyCallback),
            Stage = JudgeStage.Instance.OnApplyingEffect(),
            Clock = new(),                    // 本回合生效
            IsPlayer = sc.Self.IsPlayer,
            ModifierOrder = ModifierOrder.Before
        }
    });
```

详见 [DSL与动态注册](../Mod进阶指南/DSL与动态注册.md)。

---

## 完整示例

### 示例一：最小主职业

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

        return DSL.CreateBy(pen);
    }
}
```

### 示例二：Common 修改器提供转职入口

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
            .WriteFree(source => Common.ExcludeAllProfessions(source));

        return DSL.CreateBy(pen);
    }
}
```

### 示例三：使用 Mark 系统的技能（钢炮三连击模式）

```csharp
private static bool TripleStrikeCheck(ISkillContext sc)
{
    return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
}

private static IDSLSourceFile TripleStrike(ISkillContext sc)
{
    Pen pen = sf => sf
        .UseResource(3, ResourceType.Instance.Iron())
        .TakeMark(nameof(TripleStrike), out var layerNum)   // 取出之前的叠层
        .WriteAttack(11, AttackType.Instance.Physical())
            .WithModify(last => last.Power += layerNum.Value)  // 攻击力 + 叠层数
        .WriteResource(0.5f, ResourceType.Instance.Iron())
        .AddMark(nameof(TripleStrike));                       // 重新写入叠层

    return DSL.CreateBy(pen);
}
```

## 被动技能

重写 `MainProfession` 的 `PassiveSkillImpl(ISkillContext sc)` 方法（`virtual`）。`PassiveSkill` 已改为 `sealed override`，内部调用 `PassiveSkillImpl` 后自动设置 `IsPassive = true`。

```csharp
public override IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
{
    Pen pen = sf => sf.WriteDefense(new RealReduction { Power = 1 });
    return DSL.CreateBy(pen);
}
```


## 注意事项

1. `AttackType` 法术攻击是 `Magical()`，不是 `Magic()`
2. 通过 `body.Get<Health>()` 操作生命值，不是直接在 Body 上调用
3. 资源和攻击类型通过 `Instance` 获取
4. 技能名全小写，手动 `AddSkill`/`RemoveSkill` 也用全小写
5. 多个 Mod 同名技能 → 后写覆盖；同名职业 → 抛异常
6. `Common` 是真实职业包，修改它是提供转职入口的最常见方式
7. 攻击/防御 Power 为 `int`，资源 Power 为 `float`
8. 自定义防御继承 `DefenseEntity`，不需要实现 `Work()` 方法
9. 自定义效果使用 `[IsAnalyzer(AnalyzerType.DSL)]` 注册，`WriteEffect` 引用 `analyzerKey`
10. Mark 的 `out Lazy<T>.Value` 只能在执行阶段的延迟 lambda 中访问，声明阶段不可用
11. `[HasAttack]` 的 power 参数为 `float` 类型

## 参考

- `Project/Blacksmith/ModExamples/HolyBookMod/`
- `Project/Blacksmith/ModExamples/PhantomBookMod/` — 子职业/装备技能模式
- `Project/Blacksmith/BlacksmithCore/Specific/BuiltInProfessions/Cannon.cs` — Mark 系统完整示例
- `Project/Blacksmith/BlacksmithCore/Specific/BuiltInProfessions/Lancer.cs` — 复杂 Mark + ifUndo 模式
