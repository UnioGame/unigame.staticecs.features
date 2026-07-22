# AGENTS — unigame.staticecs.features

Use the repo-local `$build-static-ecs-features` skill when implementing, migrating, or reviewing a gameplay feature family.

## Layer and assemblies

- Gameplay families depend on `unigame.staticecs` and `unigame.staticecs.unity`; neither lower package may depend back on a feature family.
- Every independently enabled family owns one runtime asmdef. Editor code uses a family-local Editor-only asmdef; tests use family-local test asmdefs where practical.
- Cross-family dependencies must be explicit asmdef references and must not create cycles.

## Family structure

Use only directories that contain real files:

```text
<FeatureName>/
  Components/
  Events/
  Systems/
  Operations/
  Conversion/
  Authoring/
  Editor/
  Tests/Editor/
  <FeatureName>Feature.cs
  <FeatureName>Feature.Main.cs
  <FeatureName>FeatureAsset.cs
  unigame.staticecs.features.<feature>.asmdef
  README.md
```

- Omit empty categories. Use domain names such as `Registries`, `Filters`, or `Services` instead of a catch-all `Runtime` directory.
- A family may contain variants/subfeatures only when they share the same registered type set. Independently disabled blocks get their own asmdef and feature asset.
- Preserve namespaces, public names, and `.meta` GUIDs when reorganizing files.

## Features and systems

- A feature owns all type/resource/registry preparation required by its logic and creates its systems programmatically.
- Every public generic-on-world class has a neighboring Main-default alias file.
- System registration is UniTask-based and sequential. Do not serialize system lists.
- Struct and class systems are both supported. Never add reference-type constraints and never implement unused `ISystem` methods.
- Use native events for transient streams; retain components for persistent/queryable workflow state. Receiver cleanup belongs in system `Destroy`.
- Prefer family-local serializable converters. Use Mono converters only for behavior that genuinely needs a MonoBehaviour, and use converter preset assets for repeated or configuration-created entity recipes.

## ECS naming and formatting

- `IComponent` and `IMultiComponent` types end with `Component`; `ITag` types end with `Tag`; `IEvent` types end with `Event`; `ISystem` types end with `System`.
- Prefer the shortest unambiguous domain name. Do not retain technical filler such as `Entry`, `Data`, `Runtime`, `Binding`, or `Ref` when the ECS suffix and the remaining domain role already communicate the meaning.
- Keep qualifiers such as `Config`, `Source`, `Target`, `Owner`, `Parent`, `Channel`, `Tracker`, `Mask`, `Destination`, `Path`, `Agent`, and `Obstacle` only when they distinguish a real domain role or lifecycle.
- Public ECS component and event fields use PascalCase. Inspector and authoring fields use `public camelCase`; private runtime fields use `_camelCase`.
- Use Allman braces, four spaces, block-scoped namespaces, and `using` directives inside the namespace with `System` imports first.
- Empty tags use a normal multi-line declaration. Attributes stay above their declaration, and fluent registrations and query chains use one logical call per line.
- Package asmdef names are lowercase, dot-separated, start with `unigame.staticecs.features`, and match their filenames.

## Documentation

- Each feature family has an English README with Capabilities / Usage / Configuration.
- Public docs describe the current package without migration history and link to shared/upstream Static ECS documentation rather than restating it.
- Every public type and member has at least a one-line XML summary.
