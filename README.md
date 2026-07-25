# UniGame Static ECS Features

## Capabilities

This package provides feature-first gameplay families for Characteristics, Stun,
Damage, Effects, Target Selection, Movement, Game Actions, and Ability execution.
Each family owns its ECS data, resource contracts, systems, Unity authoring, and
focused tests.

See the [shared Static ECS documentation](../../../docs/knowledge/static-ecs/).

Features are parameterless. Configuration, registries, random sources, filters,
handlers, and resolved services live in ECS Resources.

## Usage

Compose feature assets in dependency order:

1. Time and RNG;
2. Characteristics and Stun;
3. Target Selection and Movement;
4. Damage and Effects;
5. Game Actions and Ability;
6. Ability Database.

The C# feature owns serialized configuration, resources, dependencies, and
systems. Its asset is a declarative adapter:

```csharp
[Serializable]
public sealed class CombatFeature : StaticEcsFeature<Main>
{
    public short updateOrder = 100;

    public override async UniTask InitializeAsync(ILifeTime lifeTime)
    {
        World<Main>.Resource<EffectIdRegistry> effectIds = default;
        await effectIds.GetAsync(lifeTime);

        var configuration = new FeatureConfiguration();
        World<Main>.SetResource(configuration);
        World<Main>.Systems<StaticEcsUpdateSystems>.Add(
            new FeatureSystem(),
            updateOrder);
    }
}

public sealed class CombatFeatureAsset :
    StaticEcsMainFeatureAsset<CombatFeature>
{
}
```

Concrete marker types in enabled asset assemblies and their programmatic
feature inheritance assemblies are automatic:

| Marker | Automatic registration |
|---|---|
| `IComponent` | component |
| `ITag` | tag |
| `IEvent` | event |
| `ILinkType` | `Link<T>` |
| `ILinksType` | `Links<T>` |
| `IMultiComponent` | `Multi<T>` |
| `IEntityType` | entity type |

Open generics and required closed constructions are not automatic. Resources,
systems, groups, feature assets, converters, abstract types, unmarked classes,
and disabled feature assemblies are also excluded.

Closed generic gameplay types use an assembly registrar:

```csharp
[assembly: StaticEcsTypeRegistrar(typeof(GameClosedTypes))]

internal sealed class GameClosedTypes : IStaticEcsTypeRegistrar<Main>
{
    public void Register(World<Main>.TypeRegistrar types)
    {
        types.Event<GameActionEvent<HealAction>>();
    }
}
```

## Configuration

Systems and operations obtain realtime dependencies through
`World<TWorld>.GetResource<T>()`. Programmatic features publish configuration
through direct `SetResource` calls. `IContext` is available during initialization through
`StaticEcsContext.Get()` or `StaticEcsContext.Get<TWorld>()`; it is not a
realtime service locator.

Initialization-owned subscriptions, background operations, and disposables use
the lifetime supplied to feature initialization:

```csharp
public override async UniTask InitializeAsync(ILifeTime lifeTime)
{
    lifeTime.AddDispose(CreateSubscription());
    await LoadAsync(lifeTime.Token);
}
```

Ordinary runtime ownership remains in public systems: acquire state in `Init`
and release it in `Destroy`. Clearing an ECS Resource does not automatically
dispose the referenced object.

Read asynchronous dependencies directly through `Resource<T>.GetAsync(lifeTime)`.
The default timeout is 5 seconds in the Editor and 10 seconds in a Player, and
timeout diagnostics name the exact requested resource.

See each family README for resources, system order, authoring defaults, and
family-owned closed generic registrations.
