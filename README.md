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
| Death | Pending | Owned by the [Death slice](../../../docs/context/static-ecs-feature-death.md) — `ApplyDamageSystem` only sets `DeathPendingTag`. |
| Effects | Pending | `EffectFeature<TWorld, TEffect>` and concrete effects (Heal, Stun, ModificationEffect) — see roadmap §5. |

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

The `Main`-default aliases (`HealthFeature`, `DamageFeature`, …) follow the [world-default aliases convention](../../../docs/knowledge/static-ecs/conventions/world-default-aliases.md). Explicitly typed generic forms (`HealthFeature<TWorld>`, `DamageFeature<TWorld>`) stay available for tests and multi-world scenarios.

## Configuration

Required co-registrations:

- `ModifierBackRefFeature` next to any `CharacteristicFeature<TWorld, TStat>` you want modifiers on (already pulled in by `HealthFeature`, `ShieldFeature`, etc.).
- `HealthFeature` and `ShieldFeature` (or equivalent `CharacteristicFeature<TWorld, …>` registrations) before `DamageFeature` — the apply step and the shield filter read these components.
- A custom `IDamageRng` registered before `DamageFeature.RegisterTypes` if the project needs a deterministic RNG (otherwise `UnityDamageRng` is registered by default).
- A custom `DamageFilterChain<TWorld>` registered before `DamageFeature.RegisterTypes`, or `new DamageFeature(registerDefaultChain: false)` to opt out of the default chain.

Documentation rules for the package live in [AGENTS.md](AGENTS.md) and [docs/knowledge/static-ecs/conventions/documentation.md](../../../docs/knowledge/static-ecs/conventions/documentation.md).
