# Movement Feature

## Capabilities

Provides navigation-independent destination data and operations. Optional NavMesh and A* adapters translate that data to a navigation backend. Both adapters include inline serializable converters alongside their Mono authoring components.

## Usage

Add `MovementFeatureAsset`, then set or clear destinations through `MovementOperations`. Add the A* feature asset separately when that backend is enabled. Prefer `NavMeshMovementSerializableConverter` or the `Astar*SerializableConverter` types in an entity provider for new inline authoring; keep Mono converters when a separate component improves the workflow.

## Configuration

The base movement family does not own a navigation system. A* support is isolated in its conditional asmdef and requires `STATIC_ECS_ASTAR`; backend feature ordering follows the speed characteristic and base movement features. Scene object references, including A* graph providers, cannot be stored in preset assets and should be configured inline on scene or prefab providers.
