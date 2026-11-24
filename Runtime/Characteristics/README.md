# Characteristics

## Capabilities

Characteristics provide clamped values, typed modifiers, source back-references,
change events, health, mana, mana regeneration, speed, shield, and combat statistics.
`ManaRegenCharacteristic` is the mana restored per second; `ManaRegenSystem` applies it by
`EcsTime.DeltaTime`. A consumer pauses regeneration of an entity with `ManaRegenBlockedTag`
(for example a dead one) and decides which clock drives `EcsTime`.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | `EcsTime` for mana regeneration | `ModifierRegistry`, `ManaRegenConfig` |
| Features | `EcsTimeFeature<TWorld>` when regeneration is installed | characteristic and modifier data |

## Usage

Add `CharacteristicsFeatureAsset` before consumers:

```csharp
var asset = ScriptableObject.CreateInstance<CharacteristicsFeatureAsset>();
asset.registerManaRegen = true;
asset.manaRegenOrder = ManaFeature.DefaultRegenOrder;
```

Concrete markers and ordinary modifier data are auto-registered. Closed
`CharacteristicComponent<T>`, `CharacteristicChangedEvent<T>`, and modifier
`Multi<T>` constructions are not. The Main assembly registrar installs the
built-in characteristic types. Isolated worlds register only what they use:

```csharp
CharacteristicTypeRegistration.Register<TWorld, SpeedCharacteristic>(types);
```

Resources, systems, groups, assets, converters, open generics, abstract types,
and disabled feature assemblies are not auto-registered.

## Configuration

`ManaRegenConfig` controls the optional Update system; its default order is `0`. Add
`ManaRegenBlockedTag` to an entity to suspend its regeneration and remove it to resume; one system of the
consumer should own the tag. Features use `SetResource`, systems use `GetResource<T>()`, and asynchronous
dependencies use `Resource<T>.GetAsync(lifeTime)`. Waiting defaults to 5 seconds
in the Editor and 10 seconds in a Player.
