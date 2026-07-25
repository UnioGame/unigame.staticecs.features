# Stun

## Capabilities

Stun stores concurrent sources in a multi-component, maintains
`StunActiveTag`, and emits `StunChangedEvent` when effective state changes.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | `ModifierRegistry` | none |
| Features | modifier back-reference composition | stun source, tag, and event data |

## Usage

Add `StunFeatureAsset` before Effects when using `StunEffect`:

```csharp
var asset = ScriptableObject.CreateInstance<StunFeatureAsset>();
```

`StunSourceComponent`, `StunActiveTag`, and `StunChangedEvent` are concrete
markers and are auto-registered. The `Multi<T>` storage is discovered from
`IMultiComponent`. Resources, systems, groups, assets, converters, open and
closed generic constructions, abstract types, and disabled feature assemblies
are not. Stun needs no closed-generic registrar.

## Configuration

Stun has no constructor parameters or system group. Timed expiration belongs to
Effects. Features await asynchronous resources through
`Resource<T>.GetAsync(lifeTime)`; waiting defaults to 5 seconds in the Editor and
10 seconds in a Player.
