# Match3

A swap-based match-3 game for Android, built in Unity 6 (C#).

![Gameplay](ProjectExternal/match3_19_10_26.gif)

## What it does

- Swipe to swap two pieces.
- Match 3 or more same-color pieces to clear them.
- Cascades fall and refill automatically.
- Box: breaks after a few hits.
- Lock: blocks a piece until it takes enough damage, then unlocks it.

## How it's built

- Core game logic is plain C#, no Unity references. Fully unit tested (EditMode tests).
- Turn resolution is synchronous: one swap resolves the whole cascade at once, then the Presentation layer plays it back step by step.
- Obstacles (Box, Lock) are driven by flags on a shared `Piece` type, not separate classes per mechanic.
- Pieces and effects are object-pooled. No per-turn GC allocation.

## Status

In progress. Core and the obstacle system are done and tested. Level data, a level editor, and win/lose are next.
