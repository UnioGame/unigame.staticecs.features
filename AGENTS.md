# AGENTS — unigame.staticecs.features

## Слой и зависимости

- Зависит от `unigame.staticecs` (база) и `unigame.staticecs.unity` (даёт дефолтный мир `Main`).
- Игровые фичи проекта (`Game.*`) используют этот пакет; обратные зависимости запрещены.

## World-default aliases (ОБЯЗАТЕЛЬНО)

Каждый новый публичный generic-on-TWorld API в этом пакете обязан иметь Main-default форму без `TWorld` рядом с generic-версией. Полное правило, шаблоны и чеклист — [docs/knowledge/static-ecs/conventions/world-default-aliases.md](../../../../docs/knowledge/static-ecs/conventions/world-default-aliases.md).

Кратко:

- **Класс**: отдельный файл `<TypeName>.Main.cs` с `class X : X<Main>`. Generic-версия не должна быть `sealed`.
- **Static-операция**: перегрузка без `TWorld` в том же файле под комментарием `// --- Main-default overloads ---`. Без `partial`.
- **Hook-методы** с required-сигнатурой (`OnDelete<TWorld>` и т. п.) дублировать не нужно.

PR'ы, добавляющие новую generic-on-TWorld фичу/систему/операцию без алиаса, не принимаются.

## Тесты

Тесты в `Tests/Editor/` намеренно используют свои `IWorldType` (`TestModifierWorld`, `TestStunWorld`) и работают с generic-формой. Не переключайте их на Main-default алиасы.
