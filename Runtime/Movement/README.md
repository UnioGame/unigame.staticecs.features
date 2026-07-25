# Movement

## Capabilities

Movement provides destination state plus independently composable NavMesh and A*
drivers. A* adds graph, obstacle, and agent synchronization when its optional
dependency is available.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | Speed characteristic for speed sync | `NavMeshMovementConfig` or `AstarMovementConfig` |
| Features | Characteristics | destination and driver data |

## Usage

```csharp
var asset = ScriptableObject.CreateInstance<NavMeshMovementFeatureAsset>();
asset.feature.registerMovementSystem = true;
asset.feature.movementOrder = NavMeshMovementFeature.DefaultMovementOrder;
```

Concrete destination, driver, link, and event markers are auto-registered.
The A* adapter also includes the base Movement feature assembly, so
`MovementDestinationComponent` is registered without a separate Movement asset.
Resources, systems, groups, assets, converters, open and closed generic
constructions, abstract types, and disabled feature assemblies are not.
Built-in Movement needs no closed-generic registrar.

## Configuration

NavMesh defaults to Update order `0`; A* graph synchronization defaults to
`-100`. Features publish configuration with `SetResource` and parameterless systems
read it with `GetResource<T>()`. A* validation runs only when
`STATIC_ECS_ASTAR` and its backend are available. Asynchronous resource waits use
`Resource<T>.GetAsync(lifeTime)` with a 5-second Editor timeout and a 10-second
Player timeout.
