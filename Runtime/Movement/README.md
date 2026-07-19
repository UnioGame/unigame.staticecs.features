# Movement Feature

## Capabilities

Provides navigation-independent destination data and operations. Optional adapters translate that data to a navigation backend.

## Usage

Add `MovementFeatureAsset`, then set or clear destinations through `MovementOperations`. Add the A* feature asset separately when that backend is enabled.

## Configuration

The base movement family does not own a navigation system. A* support is isolated in its conditional asmdef and requires `STATIC_ECS_ASTAR`; backend feature ordering follows the speed characteristic and base movement features.
