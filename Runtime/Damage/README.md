# Damage

## Capabilities

Damage processes incoming damage through dodge, block, armor, critical, and
shield filters. It reports both the filtered requested amount and the actual
health delta after clamping.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | characteristic data | `DamageConfig`, `IDamageRng`, `DamageFilterChain<TWorld>` |
| Features | Characteristics | damage tags, events, and apply system |

## Usage

Add Characteristics before Damage:

```csharp
var asset = ScriptableObject.CreateInstance<DamageFeatureAsset>();
asset.registerApplySystem = true;
asset.registerDefaultChain = true;
asset.applyOrder = DamageFeature.DefaultApplyOrder;
```

Concrete damage tags and events are auto-registered. Resources, systems, groups,
assets, converters, open and closed generic constructions, abstract types, and
disabled feature assemblies are not. Damage owns no closed-generic registrar.

## Configuration

The default Update order is `100`. Publish a custom RNG, configuration, or filter
chain with `SetResource`; systems read it with `GetResource<T>()`. Features await
asynchronous resources through `Resource<T>.GetAsync(lifeTime)` for up to 5
seconds in the Editor or 10 seconds in a Player.
