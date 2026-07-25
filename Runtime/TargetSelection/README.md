# Target Selection

## Capabilities

Target Selection indexes targetable transforms and exposes unordered-radius and
bounded-nearest queries. Nearest results are ordered by distance and stable
`EntityGID`.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | none | `ITargetIndex<TWorld>`, `TargetSelectionConfig` |
| Features | Unity transform binding | target data and rebuild system |

## Usage

```csharp
var asset = ScriptableObject.CreateInstance<TargetSelectionFeatureAsset>();
asset.registerRebuildSystem = true;
asset.rebuildOrder = TargetSelectionFeature.DefaultRebuildOrder;
```

Concrete target tags, bindings, and events are auto-registered. Resources,
systems, groups, assets, converters, open and closed generic constructions,
abstract types, and disabled feature assemblies are not. This family needs no
closed-generic registrar.

## Configuration

The default index is `KdTreeTargetIndex<TWorld>` and the default Update order is
`50`. Publish a custom index with `SetResource`; systems read it with
`GetResource<T>()`. Features await asynchronous resources through
`Resource<T>.GetAsync(lifeTime)`; waiting defaults to 5 seconds in the Editor and
10 seconds in a Player.
