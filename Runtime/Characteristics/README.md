# Characteristics

## Capabilities

Characteristics provide clamped values, typed modifiers, source back-references,
change events, health, mana, speed, shield, combat statistics, and optional mana
regeneration.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | none | `ModifierRegistry`, `ManaRegenConfig` |
| Features | none | characteristic and modifier data |

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

`ManaRegenConfig` controls the optional Update system; its default order is `0`.
Features use `SetResource`, systems use `GetResource<T>()`, and asynchronous
dependencies use `Resource<T>.GetAsync(lifeTime)`. Waiting defaults to 5 seconds
in the Editor and 10 seconds in a Player.
