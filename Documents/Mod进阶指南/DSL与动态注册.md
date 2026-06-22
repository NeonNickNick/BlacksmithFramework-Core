# DSL与动态注册
[返回](./引言.md)

专门解释 `RegistCallbackOnJudge` 方法——技能如何向判定管线动态注册回调。它向特定 `JudgeStage` 注册 `ICallbackOnJudge` 的通用入口。机制原理见[判定实现](./判定实现.md)。

## 方法做了什么

```csharp
// SourceFile 内部
_callbacksOnCompile.Add(callbacks);
```

`Compile(judgeRuleManager)` 时才真正注册：

```csharp
// Compile 末尾追加的委托中
judgeRuleManager.AddJudgeRule(callbacks);
```

即：书写阶段记下来 → 编译阶段按施法者专门化 → 加入本回合判定链。

## 核心类型

| 类型 | 说明 |
|---|---|
| `ICallbackOnJudge` | 接口，定义 `AnalyzerKey`（string）、`Stage`（`JudgeStage.CEValue`）、`Clock`（`ClapRoundClock`）、`IsPlayer`（bool） |
| `ModifierCallback` | 实现 `ICallbackOnJudge`，额外携带 `ModifierOrder`（`Before` / `After`），作为阶段核心规则的修饰器 |
| `OverrideCallback` | 实现 `ICallbackOnJudge`，直接替换阶段的默认核心规则 |

## 典型调用

### ModifierCallback（修饰器）

```csharp
// 1. 定义回调方法
[IsAnalyzer(AnalyzerType.JudgeCallback)]
public static void ReflectBeforeApplyingEffect(Community player, Community enemy)
{
    var playerData = player.Focus.Get<TurnContext>().Get<EffectAnalyzableData>();
    var enemyData = enemy.Focus.Get<TurnContext>().Get<EffectAnalyzableData>();
    
    var reflect = enemyData
        .Where(e => e.TargetType == EffectTargetType.Instance.Enemy() && e.Clock.IsRinging)
        .ToList();
    enemyData.RemoveAll(reflect.Contains);
    foreach (var e in reflect) { e.Clock.SetDelay(1); }
    playerData.AddRange(reflect);
}

// 2. 在 DSL 中注册
Pen pen = sf => sf
    .UseResource(2, ResourceType.Instance.Space())
    .RegistCallbackOnJudge(
        new()
        {
            new ModifierCallback()
            {
                AnalyzerKey = nameof(ReflectBeforeApplyingEffect),
                Stage = JudgeStage.Instance.OnApplyingEffect(),
                Clock = new(),
                IsPlayer = sc.Self.IsPlayer,
                ModifierOrder = ModifierOrder.Before
            }
        });

return DSL.CreateBy(pen);
```

### OverrideCallback（覆盖默认规则）

```csharp
// 1. 定义覆盖方法
[IsAnalyzer(AnalyzerType.JudgeCallback)]
public static void MyOverride(Community player, Community enemy)
{
    // 完全替代该阶段的默认行为
}

// 2. 注册覆盖
.RegistCallbackOnJudge(new()
{
    new OverrideCallback()
    {
        AnalyzerKey = nameof(MyOverride),
        Stage = JudgeStage.Instance.OnEnd(),
        Clock = new(),
        IsPlayer = sc.Self.IsPlayer
    }
})
```

### 混合使用

同一 `RegistCallbackOnJudge` 调用中可混合 `ModifierCallback` 和 `OverrideCallback`：

```csharp
.RegistCallbackOnJudge(new()
{
    new ModifierCallback()
    {
        AnalyzerKey = nameof(ReflectBeforeApplyingEffect),
        Stage = JudgeStage.Instance.OnApplyingEffect(),
        Clock = new(),
        IsPlayer = sc.Self.IsPlayer,
        ModifierOrder = ModifierOrder.Before
    },
    new ModifierCallback()
    {
        AnalyzerKey = nameof(ReflectAfterAttackCanceling),
        Stage = JudgeStage.Instance.OnAttackCanceling(),
        Clock = new(),
        IsPlayer = sc.Self.IsPlayer,
        ModifierOrder = ModifierOrder.After
    }
});
```

## ICallbackOnJudge 字段

| 字段 | 说明 |
|---|---|
| `AnalyzerKey` | string，指向 `AnalyzerRegistry.JudgeCallback` 中的注册方法 |
| `Stage` | 挂到哪个 `JudgeStage` |
| `Clock` | `ClapRoundClock`，统一管理 `delayRounds`、`remainingRounds`、`isInfinite`、`forceKill` |
| `IsPlayer` | bool，执行时决定参数顺序——`true` 则 `AnalyzerRegistry.JudgeCallback.Get(key)(player, enemy)`，`false` 则参数对调 |

`ModifierCallback` 额外字段：

| 字段 | 说明 |
|---|---|
| `ModifierOrder` | `Before` 或 `After`（核心规则的前/后） |

## ICallbackOnJudge 内可做的事

操作 AnalysableData 列表、组件，甚至临时编译 DSL：

```csharp
[IsAnalyzer(AnalyzerType.JudgeCallback)]
public static void MyCallback(Community player, Community enemy)
{
    if (enemy.Focus.Get<TurnContext>().Get<AttackAnalyzableData>()
        .Find(a => a.Clock.IsRinging) == null) return;

    DSL.CreateBy(sf => sf
        .WriteAttack(10, AttackType.Instance.Magical()))
        .Compile().Execute(player);
}
```

在回调内部调用时直接 `Compile().Execute(player)`（不需要 `judgeRuleManager` 参数）。

## 常见模式：下回合检查 + 本回合触发

```csharp
new()
{
    // 触发规则：本阶段立刻生效
    new ModifierCallback()
    {
        AnalyzerKey = nameof(MyTrigger),
        Stage = JudgeStage.Instance.OnAttackCanceling(),
        Clock = new(),
        IsPlayer = sc.Self.IsPlayer,
        ModifierOrder = ModifierOrder.Before
    },
    // 清理/重置：下回合开始执行
    new ModifierCallback()
    {
        AnalyzerKey = nameof(MyCleanup),
        Stage = JudgeStage.Instance.OnBegin(),
        Clock = new(delayRounds: 1),
        IsPlayer = sc.Self.IsPlayer,
        ModifierOrder = ModifierOrder.Before
    }
}
```

## 为什么用动态注册

有的效果不是"当前技能一放就结算完"——它依赖对手行为、挂在特定阶段、持续到下回合、要插在默认规则前/后。这类逻辑放到 `ICallbackOnJudge` 里比塞进普通 DSL 更自然可控。

## 编写建议

1. 回调方法标记 `[IsAnalyzer(AnalyzerType.JudgeCallback)]`——由源生成器自动注册到 `AnalyzerRegistry.JudgeCallback`
2. 在规则内制造即时攻击时，调用简短 DSL 即可
3. 规则依赖技能类字段时，控制好重置时机
4. 先写最小可运行版本，再通过 `ClapRoundClock` 补持续/延迟回合和 `forceKill` 条件
5. 修饰器用 `ModifierCallback` + `ModifierOrder`，替换默认规则用 `OverrideCallback`
6. 使用 `IsPlayer` 标志而非依赖参数顺序——`IsPlayer` 决定执行时 player/enemy 的传递顺序
7. 跨回合状态优先使用 Mark 系统（`AddMark`/`TakeMark`/`CountMark`），而非在回调中直接操作状态

参考：`Project/Blacksmith/BlacksmithCore/Specific/BuiltInProfessions/Common.cs` 中的 Reflect 实现（完整的 ModifierCallback + AnalyzerKey 示例）。
