# Match3

A swap-based match-3 game for Android, built in Unity 6 (C#).

![Gameplay](ProjectExternal/match3_19_10_26.gif)

## What it does

- Swipe to swap two pieces.
- Match 3 or more same-color pieces to clear them.
- Bigger shapes create special pieces:
  - 4 in a row: Rocket
  - 5 in a row: Light Ball
  - L or T shape: TNT
  - 2×2 square: Propeller
- Cascades fall and refill automatically.
- Box: breaks after a few hits from adjacent matches.
- Lock: an overlay that can cover several cells. Adjacent matches damage it; when it breaks, the pieces underneath are freed.

## How it's built

- Core game logic is plain C#, no Unity references. Covered by EditMode unit tests.
- Matches are found with a flood fill per color region, then split into shape templates (lines, L, T, square) by priority.
- Turn resolution is synchronous: one swap resolves the whole cascade at once, then the Presentation layer plays it back step by step.
- Obstacles (Box, Lock) are driven by flags on a shared `Piece` type, not separate classes per mechanic.
- Piece views and effects are object-pooled. No GC allocation while the board is idle.

## Status

In progress. Core, shape matching, special piece creation and the obstacle system are done and tested.

Next: level data, a level editor, and win/lose.

Known gaps:
- Special pieces are created but don't activate yet.
- Input is mouse-only; touch support is pending.
- The starting board is random; a temporary swap clears initial matches until levels come from the editor.