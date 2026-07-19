# Characteristics Feature

## Capabilities

Registers characteristic values, modifiers and source back-references, plus the standard health, shield, mana, and speed families. The feature owns mana regeneration system registration.

## Usage

Add a `CharacteristicsFeatureAsset` before features that read combat or movement characteristics. Use `CharacteristicOperations` and modifier extensions for runtime changes.

## Configuration

Closed generic characteristic components are registered explicitly. Custom characteristic marker types require their matching `CharacteristicFeature<TWorld, TStat>` registration and a Main-default alias when exposed publicly.
