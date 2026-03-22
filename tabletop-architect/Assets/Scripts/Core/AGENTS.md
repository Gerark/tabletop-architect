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
- Interaction waits must use explicit window ids and reject stale submissions.
- Transcript batches should stay flat, data-oriented, and runtime-produced.
- Hidden information filtering belongs to runtime materialization, not UI guesswork.
- Undo/redo is checkpoint-based, linear, and must restore RNG state exactly.

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
- Prefer additive controller extensions, wrappers, and new serializable data records over refactoring existing flow.
- When unsure, prefer simpler data-oriented structures.

## First implementation priorities
- Runtime ids and dense containers
- Match root
- Value / Property / Temps
- Element / Area / Slot runtime records
- Placement operations
- Local topology and movement
- Phase / event loop
- Transcript batches / interaction windows / checkpoint history as additive runtime metadata

## Local Agent Rules
- Default to read-only analysis.
- Do not modify files unless the user explicitly asks for a patch.
- Do not run `dotnet`, `msbuild`, Unity builds, test runners, or any command that generates binaries or artifacts.
- Do not use PowerShell reflection, `Assembly.Load*`, or `Add-Type`.
- Do not touch `Temp/`, `bin/`, `obj/`, or generated project files.
- If code changes are requested, provide a proposed diff in chat unless the user explicitly approves file edits.
