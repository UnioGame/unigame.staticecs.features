# UniGame Static ECS Features

## Capabilities

This package provides feature-first gameplay families for Characteristics, Stun,
Damage, Effects, Target Selection, Movement, Timer, Game Actions, Ability execution,
and View System presentation.
Each family owns its ECS data, resource contracts, systems, Unity authoring, and
focused tests.

See the [shared Static ECS documentation](../../../docs/knowledge/static-ecs/).

Features are parameterless. Configuration, registries, random sources, filters,
handlers, and resolved services live in ECS Resources.

## Usage

Declare features in dependency order:

1. Time, Timer, and RNG;
2. Characteristics and Stun;
3. Target Selection and Movement;
4. Damage and Effects;
5. Game Actions and Ability;
6. Ability Database;
7. View Feature before game-owned presentation features.

See the [View Feature README](Runtime/View/README.md) for typed view-model
synchronization, placement, hierarchy, and View System configuration.

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

Automatic marker-type registration, closed generic registrars, lifetime ownership,
and resource await timeouts are documented in the
[unigame.staticecs.unity README](../unigame.staticecs.unity/README.md).

## Configuration

Systems and operations obtain realtime dependencies through
`World<TWorld>.GetResource<T>()`. Programmatic features publish configuration
through direct `SetResource` calls. `IContext` is available during initialization through
`StaticEcsContext.Get()` or `StaticEcsContext.Get<TWorld>()`; it is not a
realtime service locator.

See each family README for resources, system order, authoring defaults, and
family-owned closed generic registrations.
