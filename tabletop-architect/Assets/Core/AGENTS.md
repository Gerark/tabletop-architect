# AGENTS.md

## Project purpose
This repository contains the runtime core for Tabletop Architect.
Prioritize clean architecture, serializable runtime data, and incremental implementation.

## Non-negotiable architecture rules
- Authoritative runtime state must be plain serializable data.
- Use ids/handles, never runtime object references as source of truth.
- Use dense containers plus monotonic ids.
- Do not use nullable `?` modeling for runtime state.
- Properties are persistent state.
- Temps are temporary/helper state.
- Bindings are shallow for now: direct properties and Temps only.
- Do not add deep dereference like `CurrentPlayer.Pawn.CurrentFace.Value` unless explicitly requested.
- Save/load is supported only at stable boundaries, never mid-action or mid-resolution.

## Coding rules
- Keep changes incremental and easy to review.
- Prefer extending existing structures over inventing parallel systems.
- Ask before broad refactors.
- Preserve backward compatibility where reasonable.
- Add comments only where they clarify non-obvious runtime rules.

## Validation before finishing
- Build the project.
- Run the relevant tests if they exist.
- If no tests exist, explain what was validated manually.

## How to work
- Implement one vertical slice at a time.
- Do not try to build the whole engine in one pass.
- Keep authored definitions and runtime state clearly separated.
- When unsure, prefer simpler data-oriented structures.

## First implementation priorities
- Runtime ids and dense containers
- Match root
- Value / Property / Temps
- Element / Area / Slot runtime records
- Placement operations
- Local topology and movement
- Phase / event loop