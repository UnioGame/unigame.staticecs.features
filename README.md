# UniGame Static ECS Features

Independently composable gameplay feature families for the UniGame Static ECS stack.

## Capabilities

| Family | Assembly | Responsibility |
| --- | --- | --- |
| Characteristics | `unigame.staticecs.features.characteristics` | Characteristics, modifiers, health, shield, mana, and speed. |
| Stun | `unigame.staticecs.features.stun` | Multi-source stun state and operations. |
| Damage | `unigame.staticecs.features.damage` | Damage/healing events, filters, application, and death-pending state. |
| Effects | `unigame.staticecs.features.effects` | Timed effects, stacking, ticking, and effect cleanup. |
| Target Selection | `unigame.staticecs.features.targetselection` | Targetable data and spatial target index. |
| Movement | `unigame.staticecs.features.movement` | Destination data and navigation-independent operations. |
| Game Actions | `unigame.staticecs.features.gameactions` | Action-mask maintenance. |
| Ability | `unigame.staticecs.features.ability` | Data-driven ability execution and authoring. |
| A* Movement | `unigame.staticecs.features.movement.astar` | Conditional A* graph and agent integration. |

Every independently enabled family owns its runtime asmdef and feature asset. Ability Graph editor code is isolated in an Editor-only asmdef inside the Ability family.

## Usage

Create the feature assets required by the game, configure their inline serializable feature,
add them to a `StaticEcsServiceSource`, and keep dependency order explicit. The asset wrapper
contains no duplicated feature settings. The demo source is a complete composition example.

Gameplay code uses feature operations after startup:

```csharp
DamageOperations.RaiseDamage(attacker, target, 25f, DamageType.Physical);
MovementOperations.SetDestination(actor, destination);
EffectOperations.Apply<StunEffect>(target, source, duration: 2f);
```

Code-defined and asset-authored abilities share the same runtime registry. The `Main` aliases are the convenient default; generic-on-world forms remain available for tests and multi-world projects.

## Configuration

- Put shared data dependencies before consumers: characteristics before damage, time before effects and abilities, and target selection before AoE ability steps.
- Treat `RegisterAll` as a supplement. Closed generic components/events, resources, handlers, and registries are registered manually by their feature.
- Use native `IEvent` for transient commands or notifications. Use components when state must survive without a receiver, participate in queries, or represent a workflow.
- Do not send events during `RegisterSystemsAsync`; receivers do not exist until system groups initialize. `StartAsync` is the first safe startup stage.
- A system may be a struct or class. Implement only the lifecycle methods it uses.
- A* integration compiles only with `STATIC_ECS_ASTAR` and requires A* Pathfinding Project 5.x.

Each family README documents its own capabilities and required composition.
