# Stun Feature

## Capabilities

Provides multi-source stun state, `StunActiveTag`, and operations that keep the aggregate state synchronized as sources are added or removed.

## Usage

Add `StunFeatureAsset`, then call `StunOperations` from gameplay or effect handlers.

## Configuration

Place this feature before any effect feature that delegates lifetime changes to stun operations. Stun state is persistent ECS data rather than a transient event.
