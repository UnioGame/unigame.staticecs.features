# UniGame Static ECS Features

Reviewed gameplay features built on Static ECS.

Each feature is redesigned before implementation with explicit review of:

- convenience and readability;
- performance and hot paths;
- allocation profile;
- Static ECS-native data model;
- demo scene validation.

## Capabilities

| Slice | Status | Public surface |
| ----- | ------ | -------------- |
| Characteristics | Done | `CharacteristicFeature<TWorld, TStat>`, scalar marker types (`HealthCharacteristic`, `ManaCharacteristic`, `SpeedCharacteristic`, `ShieldCharacteristic`, …), `CharacteristicComponent<TStat>`, `CharacteristicChangedEvent<TStat>`, `CharacteristicOperations`. |
| Modifiers | Done | `CharacteristicModifierEntry<TStat>`, `CharacteristicModifierExtensions.ApplyModifier/Remove…/RecomputeValue`, `ModifierBackRefFeature`, source-cleanup via `ModifierSourceTracker.OnDelete` + bitmask `CharacteristicFlag`. |
| Mana regen | Done | `ManaFeature` registers `ManaRegenSystem<TWorld>` on the update group. |
| Stun | Done | `StunFeature`, `StunSource` multi-component, `StunActiveTag`, `StunOperations`, multi-source counter with auto-cleanup. |
| Damage | Done | `DamageFeature` + filter chain (Dodge → Block → ArmorResist → Critical → Shield) + `ApplyDamageSystem`, combat-stat marker types (`BlockChance`, `DodgeChance`, `ArmorResist`, `CriticalChance`, `CriticalMultiplier`), `DamageOperations.RaiseDamage/RaiseHealing`, `IDamageRng` resource, `DeathPendingTag`. |
| Effects framework | Done | `EffectFeature<TWorld, TEffect>` over `EffectComponent<TEffect>`, `EffectRosterEntry` multi for UI, `IEffectHandler<TWorld, TEffect>` resource, `EffectConfig<TWorld, TEffect>` (stacking + refresh), `EffectTickSystem<TWorld, TEffect>` driven by `EcsTime`, `EffectOperations.Apply/Remove/Has/GetTimeLeft/GetStacks/RemoveAll/RemoveByMask`. |
| Effect back-refs / cleanup | Done | `EffectFlag` (`ulong` bitmask) + `[EffectFlag]` attribute + `EffectFlagOf<T>` cache; `EffectBackRef` multi + `EffectSourceTracker` OnDelete hook + `EffectRegistry` slot table — destroying a source synchronously strips every effect it had applied to every target, no per-tick `Source.Status` polling required. |
| HealOverTime effect | Done | `HealOverTimeFeature` + `HealOverTimeOperations.Apply` — periodic healing through `DamageOperations.RaiseHealing`, scales by stacks. |
| Stun effect | Done | `StunEffectFeature` — bridges effect lifetime to `StunOperations` source counter; respects manual stun sources. |
| Modification effect | Done | `ModificationEffectFeature<TStat>` + `ModificationEffectOperations.Apply` — installs a time-bounded characteristic modifier keyed by source; auto-removes via `ModifierBackRef` on source destroy. |
| TargetSelection | Done | `TargetSelectionFeature<TWorld>`, `TargetableTag`, `ITargetIndex<TWorld>`, managed `KdTreeTargetIndex<TWorld>` v1, `TargetIndexRebuildSystem`; used by ability AoE queries and reusable by AI/projectiles. |
| Ability | Done | Data-driven step pipeline: `AbilityRegistry<TWorld>` stores `AbilityDefinition` + root `IAbilityStepConfig`; `AbilityAsset`, `AbilityDatabase`, `AbilityDatabaseFeature<TWorld>` for ScriptableObject authoring; `AbilityCastSystem`, `AbilityWaitSystem`, `AbilityStepProgressionSystem`; leaves (`Wait`, `ApplyDamage`, `AoeQuery`, `SetPrimaryTargetFromAoe`, `ApplyEffect`), composites (`Sequence`, `Parallel`, `Conditional`, `Repeat`), conditions (`Always`, `Never`, `AoeNonEmpty`, `PrimaryTargetAlive`). |
| Movement | Done | `MovementOperations` and `MovementDestinationComponent`, with optional Unity NavMesh and A* Pathfinding Project 5.x adapters. |
| Death | Pending | Owned by the [Death slice](../../../docs/context/static-ecs-feature-death.md) — `ApplyDamageSystem` only sets `DeathPendingTag`. |

## Usage

Register the slices a project needs in its world feature, in dependency order:

```csharp
public sealed class GameEcsFeature : StaticEcsFeature<Main> {
    public override void RegisterTypes(World<Main>.TypeRegistrar types) {
        new ModifierBackRefFeature().RegisterTypes(types);
        new HealthFeature().RegisterTypes(types);
        new ShieldFeature().RegisterTypes(types);

        new CharacteristicFeature<Main, BlockChanceCharacteristic>().RegisterTypes(types);
        new CharacteristicFeature<Main, DodgeChanceCharacteristic>().RegisterTypes(types);
        new CharacteristicFeature<Main, ArmorResistCharacteristic>().RegisterTypes(types);
        new CharacteristicFeature<Main, CriticalChanceCharacteristic>().RegisterTypes(types);
        new CharacteristicFeature<Main, CriticalMultiplierCharacteristic>().RegisterTypes(types);

        new DamageFeature().RegisterTypes(types);
    }
}
```

Raising damage from gameplay code:

```csharp
DamageOperations.RaiseDamage(attacker, victim, amount: 25f, type: DamageType.Physical);
DamageOperations.RaiseHealing(healer, ally, amount: 10f);
```

Applying status effects:

```csharp
HealOverTimeOperations.Apply(target: ally, source: caster, healPerTick: 5f, duration: 5f, period: 1f);
EffectOperations.Apply<StunEffect>(target: enemy, source: caster, duration: 3f);
ModificationEffectOperations.Apply<SpeedCharacteristic>(
    target: ally, source: caster, op: CharacteristicModifierOp.Mul, value: 1.5f, duration: 5f);
```

Registering A* movement when `com.arongranberg.astar` 5.x is installed:

```csharp
new SpeedFeature().RegisterTypes(types);
var astar = new AstarMovementFeature();
astar.RegisterTypes(types);

astar.RegisterSystems(new StaticEcsSystemsBuilder<Main, StaticEcsUpdateSystems>());
MovementOperations.SetDestination(playerGid, destination);
```

The graph host uses `StaticEcsEntityProvider`, `AstarPath`, and
`AstarGridGraphConverter`. Obstacle hosts use their own providers and
`AstarObstacleConverter` linked to the graph provider. Agent entities use an
`IAstarAI` implementation such as `AIPath`, the A* `Seeker`, and
`AstarMovementConverter` linked to the same graph provider. ECS creates and
validates the grid before agents can request paths, updates dynamic obstacle
bounds, assigns the owned graph to `Seeker.graphMask`, disables automatic
repath, and then synchronizes destination, stop state, and speed.

Registering authored ability assets:

```csharp
public sealed class GameEcsFeature : StaticEcsFeature<Main> {
    private readonly AbilityDatabase _abilityDatabase;

    public GameEcsFeature(AbilityDatabase abilityDatabase) {
        _abilityDatabase = abilityDatabase;
    }

    public override void RegisterTypes(World<Main>.TypeRegistrar types) {
        new AbilityFeature(registerSystems: false).RegisterTypes(types);
        new AbilityDatabaseFeature(_abilityDatabase).RegisterTypes(types);
    }
}
```

`AbilityDatabaseFeature<TWorld>` instantiates each `AbilityAsset` by default before
registering its `AbilityDefinition` and root `IAbilityStepConfig`. This keeps
runtime state isolated from the source ScriptableObject asset and avoids per-step
clone methods.

Code-defined abilities remain supported for tests, generated content, and small
bootstrap scenarios:

```csharp
var registry = World<Main>.GetResource<AbilityRegistry<Main>>();
registry.Register(
    new AbilityDefinition(DemoAbilityIds.StunNova),
    new SequenceStepConfig(new IAbilityStepConfig[] {
        new WaitStepConfig(0.4f),
        new AoeQueryStepConfig(radius: 4f, maxTargets: 16, excludeCaster: true),
        new ApplyEffectStepConfig(effectIds.Get<StunEffect>(), AbilityTargetMode.AoeBroadcast, duration: 2f),
        new WaitStepConfig(0.3f),
    }));
```

The `Main`-default aliases (`HealthFeature`, `DamageFeature`, …) follow the [world-default aliases convention](../../../docs/knowledge/static-ecs/conventions/world-default-aliases.md). Explicitly typed generic forms (`HealthFeature<TWorld>`, `DamageFeature<TWorld>`) stay available for tests and multi-world scenarios.

## Configuration

Required co-registrations:

- `ModifierBackRefFeature` next to any `CharacteristicFeature<TWorld, TStat>` you want modifiers on (already pulled in by `HealthFeature`, `ShieldFeature`, etc.).
- `HealthFeature` and `ShieldFeature` (or equivalent `CharacteristicFeature<TWorld, …>` registrations) before `DamageFeature` — the apply step and the shield filter read these components.
- A custom `IDamageRng` registered before `DamageFeature.RegisterTypes` if the project needs a deterministic RNG (otherwise `UnityDamageRng` is registered by default).
- A custom `DamageFilterChain<TWorld>` registered before `DamageFeature.RegisterTypes`, or `new DamageFeature(registerDefaultChain: false)` to opt out of the default chain.
- `EcsTimeFeature<TWorld>` (from `unigame.staticecs`) before any effect feature — `EffectTickSystem` reads `EcsTime.DeltaTime`. Add `EcsTimeUpdateSystem<TWorld>` to the update group as well so the resource is refreshed each frame.
- For `StunEffectFeature` — register `StunFeature` first (the effect handler delegates to `StunOperations`).
- For `ModificationEffectFeature<TStat>` — register the matching `CharacteristicFeature<TWorld, TStat>` and `ModifierBackRefFeature` first.
- Each `EffectFeature<TWorld, TEffect>` registers exactly one `IEffectHandler<TWorld, TEffect>` resource. Concrete effect features set the default handler; project code may override by calling `World<TWorld>.SetResource<IEffectHandler<TWorld, TEffect>>(custom)` before the feature registers.
- `EffectFeature(maxStacks, refreshOnReapply, tickOrder, registerTickSystem)` ctor controls stacking policy and tick ordering. Pass `registerTickSystem: false` if the project wires systems imperatively and prefers to add `EffectTickSystem<TWorld, TEffect>` by hand.
- Every concrete effect-type struct must declare `[EffectFlag(EffectFlag.X)]` with a single-bit value. The bit is reserved by the framework (one slot per effect family) and consumed by `EffectBackRef` + `EffectSourceTracker` for source-destroy cleanup. Generic `ModificationEffect<TStat>` shares one flag for every TStat closure — granular removal still goes through typed `EffectOperations.Remove<TWorld, ModificationEffect<TStat>>`. Reserved bits `Reserved0..Reserved3` are available for project- or test-side effects.
- `AbilityFeature<TWorld>` requires `EcsTimeFeature<TWorld>` for `WaitStepConfig` and timed effects. Ability AoE leaves require `TargetSelectionFeature<TWorld>` and entities with `TransformBindingComponent`.
- `AbilityDatabaseFeature<TWorld>` registers ScriptableObject-authored abilities into `AbilityRegistry<TWorld>`. Register `AbilityFeature<TWorld>` first so the registry and default activators exist.
- `AbilityEffectDispatchRegistry<TWorld>` maps `EffectId` to concrete effect operations for `ApplyEffectStepConfig`. Register custom dispatchers after effect features register their `EffectId`.
- Cooldown and resource-cost validation are intentionally outside the ability runtime. Business/gameplay code checks `CooldownOperations` or resource state before calling `AbilityOperations.TryStartCast`; the ability slice executes once it receives a cast request.
- `AstarMovementFeature<TWorld>` requires A* Pathfinding Project 5.x and is compiled only with `STATIC_ECS_ASTAR`. Register `SpeedFeature<TWorld>` to drive agent speed. Its default system registration runs `AstarGraphSystem<TWorld>` before `AstarMovementSystem<TWorld>`; constructor flags allow either system to be wired manually.

## Ability Pipeline

Ability runtime is a "dumb executor" over a tree of `IAbilityStepConfig`.
The registry stores data, not handlers:

```csharp
registry.Register(new AbilityDefinition(abilityId), rootStepConfig);
```

`AbilityCastSystem` spawns a cast-entity, then `AbilityStepProgressionSystem`
walks the step tree:

- `Sequence` executes children in order.
- `Conditional` evaluates an `IAbilityStepCondition` and descends into the selected branch.
- `Repeat` runs a body up to `MaxIterations`, optionally guarded by a condition.
- `Parallel` spawns branch cast-entities with `AbilityInlineRootConfig` and joins them through `AbilityBranchCompletedEvent` using `AllSuccess` or `AnySuccess`.

Leaf steps are activated through `AbilityStepActivatorRegistry<TWorld>`.
Long-running leaves write state components such as `AbilityWaitState`; synchronous
leaves return `StepStatus.Success` or `StepStatus.Failed` immediately.

The ScriptableObject authoring layer stores the same step tree in an `AbilityAsset`
using `[SerializeReference]`. An `AbilityDatabase` groups assets for registration,
and the non-generic `AbilityDatabaseFeature` alias targets the default `Main` world.

## Tests

EditMode tests live in `Tests/Editor` and compile into
`unigame.staticecs.features.tests`.

For Unity Test Runner discovery in this project:

- `GameClient/Packages/manifest.json` includes `com.unigame.staticecs.features` in `testables`.
- `Tests/Editor/unigame.staticecs.features.tests.asmdef` uses `optionalUnityReferences: ["TestAssemblies"]`.
- Test classes are marked with `[TestFixture]`.
- Tests that create entities with `TransformBindingComponent` must register it explicitly in the test world before `World<TWorld>.Initialize()`.

Current EditMode coverage includes characteristics, modifiers, damage, effects,
target selection, ability operations, leaf steps, composite steps, ScriptableObject
ability database registration, movement operations, A* agent synchronization,
and ability pipeline smoke tests. Run `unigame.staticecs.features.tests` and the
conditional `unigame.staticecs.features.movement.astar.tests` assembly from the
Unity Test Runner.

Documentation rules for the package live in [AGENTS.md](../../../AGENTS.md) and [docs/knowledge/static-ecs/conventions/documentation.md](../../../docs/knowledge/static-ecs/conventions/documentation.md).
