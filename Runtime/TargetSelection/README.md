# Target Selection Feature

## Capabilities

Registers targetable state, a spatial target index resource, and the index rebuild system used by gameplay and ability AoE queries.

## Usage

Add `TargetSelectionFeatureAsset` before features that execute spatial queries. Target entities require targetable and transform-binding data.

## Configuration

The default index is managed and rebuilt by the feature system. Tests or projects may install another `ITargetIndex<TWorld>` before feature registration.
