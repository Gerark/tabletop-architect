# Runtime Architecture

This document describes the current runtime architecture for Tabletop Architect. It reflects the agreed direction for the **first serious implementation slice** of the runtime core.

The goal is to keep the runtime:

- data-oriented
- serializable
- deterministic enough to save/load reliably
- separate from visuals/presentation
- incremental to implement

This is not a final design for every future feature. It is the current architectural baseline.

---

## 1. Core philosophy

The runtime must be built around **plain serializable data**.

Authoritative runtime state must not depend on:

- object references
- callback chains
- hidden execution stacks
- partially executed action objects
- UI state
- presentation objects

The runtime should be able to restore a match from saved data by reconstructing the same stable situation, then resuming from that stable boundary.

Main principles:

- use **ids/handles**, never runtime object refs as source of truth
- use **dense containers**
- use **monotonic runtime ids**
- keep authored definitions and runtime state clearly separated
- save only at **stable boundaries**
- keep the first binding system shallow and explicit

---

## 2. Definitions vs runtime

There is a hard distinction between:

### Authored definitions

These come from game data / JSON.

Examples:

- game info
- element definitions
- area definitions
- topology definitions
- property definitions
- setup definitions
- play definitions
- event rules
- operations
- conditions

### Runtime state

This is the live match state created from the definitions.

Examples:

- runtime element instances
- runtime player state
- runtime area instances
- runtime slot state
- current phase
- current player
- event queue
- property values
- temps
- box stock availability

Definitions describe what can exist and how rules behave. Runtime state tracks what currently exists and what is currently true.

---

## 3. Runtime ids and dense containers

Every runtime object that can be referenced must have a **stable monotonic runtime id**.

Examples:

- element instance id
- area instance id
- slot instance id
- player id

Rules:

- ids are monotonic
- ids are never reused during a match
- ids are serializable
- ids are the only authoritative cross-reference mechanism

Every important runtime object type should live in its own **dense serializable container**.

Examples:

- runtime elements container
- runtime areas container
- runtime slots container
- runtime players container

The runtime may keep internal lookup tables like:

- `id -> dense index`

But container position is not identity.

---

## 4. Match root

The match root is the master runtime instance.

It should contain only:

- match progression state
- execution state
- runtime containers
- global property state
- temporary scoped state
- id counters
- box stock counts
- event queue state

### Match root responsibilities

#### 4.1 Match progression

- selected ruleset
- current phase
- current player id
- ended flag
- winner id if any

#### 4.2 Match-level persistent state

- match-scoped properties

#### 4.3 Match-level temporary state

- match temps
- turn temps
- setup temps

#### 4.4 Execution state

- current execution mode
- queued next phase
- current event payload if resolving
- queued emitted events

#### 4.5 Runtime world containers

- players
- elements
- areas
- slots

#### 4.6 Id counters

- next element id
- next area id
- next slot id
- next player id if needed

#### 4.7 Box stock state

A match-level stock structure aligned with element definitions.

For example:

- available count per authored element definition index

This is required for `TakeFromBox` and `ReturnToBox`.

---

## 5. Save / load philosophy

The first implementation does **not** support save/load in the middle of action resolution.

Save should happen only at **stable boundaries**.

### Stable / saveable boundaries

Examples:

- waiting for player to choose the next available action
- after an action fully resolved
- after all triggered events fully resolved
- after queued phase changes were fully applied
- ended match state

### Not saveable for now

Examples:

- mid-operation
- mid-event rule
- while draining the event queue
- during movement stepping
- while the player is mid-gesture inside an action
- while resolving internal action flow

The runtime should be resumed from stable state, not from an instruction pointer inside a half-resolved action.

---

## 6. Execution states

The match root should expose a small execution mode/state.

Current recommended states:

- `Setup`
- `WaitingForPlayerAction`
- `Resolving`
- `Ended`

`Resolving` is not saveable. The important stable state is `WaitingForPlayerAction`.

---

## 7. Properties and Temps

Runtime state is split into two conceptual categories:

### Properties

Persistent meaningful state.

Examples:

- `money`
- `completedLaps`
- `Pawn`

Properties are:

- persistent
- saveable
- directly exposed in bindings
- declared in authored data

### Temps

Temporary helper state.

Examples:

- `moveAmount`
- `hasMovedThisTurn`

Temps are:

- temporary
- scoped
- usually auto-cleared depending on context
- accessed under `Temps.*`

This same conceptual split should exist on:

- Match
- Player
- Element
- Area

---

## 8. Authored Property

The authored concept is `Property`. This replaces the narrower `Resource` naming.

A property is declared persistent state.

### First property scopes

For the shared/global property-definition list:

- `Match`
- `Player`
- `Team`

Element and Area will later have their own local property definitions inside their own definitions.

### Property values

A property stores a `Value`.

A property has:

- key
- scope
- value kind/type
- optional default value

If default value is omitted, runtime uses an implicit default based on the `Value` kind.

Properties may represent:

- numbers
- booleans
- strings
- ids
- collections

If a property stores an element, it stores an **element instance id**, never an object reference.

---

## 9. Value

`Value` is the universal data carrier for the runtime and the authored rule definitions.

It is used for:

- authored literals
- authored binding expressions
- property default values
- runtime property values
- temps values
- operation parameters

### Current Value shape

`Value` is a tagged union style container with:

- a kind
- one meaningful payload depending on the kind

Current / planned kinds for the first implementation:

- Null
- Int
- Float
- Bool
- String
- Binding
- ElementId
- Collection

### Collection rules

Collections are stored as collections of `Value`.

Rules:

- homogeneous items only
- no nested collections for now
- no object refs
- references are ids/handles only

### Binding values

`ValueKind.Binding` is valid in authored definitions and execution definitions.

Saved runtime state should mostly store already-resolved values rather than unresolved bindings.

---

## 10. Binding system

Bindings are intentionally shallow for the first implementation.

### Syntax

- dot notation only
- no indexing
- no filters
- no functions
- no LINQ-like expressions

### Binding roots

Current roots:

- `Match`
- `CurrentPlayer`
- `Players`
- `repeat`
- `Event`
- `Temps`

### Exposure rules

Bindings only traverse **explicitly exposed runtime members**. No generic reflection over arbitrary C# fields/properties.

### What is exposed first

For runtime owners:

- direct Properties
- `Temps`

And for built-in execution roots:

- `repeat.Current`
- `repeat.Index`
- event payload fields
- match/current player/player collections

### Important limitation

Property values that store ids are **not automatically dereferenced**.

This means:

Valid:

- `CurrentPlayer.money`
- `CurrentPlayer.Pawn`
- `CurrentPlayer.Temps.hasMoved`
- `Temps.moveAmount`
- `Event.Total`

Not valid for now:

- `CurrentPlayer.Pawn.CurrentFace.Value`

Because `Pawn` is just a property holding an element id, not an auto-dereferenced runtime element object.

---

## 11. Elements as box entries

Authored elements represent what exists in the game box.

Elements may define an `Amount`, which means:

- one authored element entry may represent multiple interchangeable possible runtime copies

This preserves a tabletop mental model:

- authored data says what pieces exist
- setup chooses what gets taken out of the box
- runtime creates real instances only when brought into the match

If a piece must be individually meaningful, it should be authored as a distinct element instead of relying on `Amount`.

---

## 12. Box lifecycle

### 12.1 `TakeFromBox`

Creates runtime instances from authored stock.

Rules:

- consumes stock
- can use `ByKey` or `ByTag`
- `ByKey` may use `Amount`, defaulting to `1`
- `ByTag` means all available matching copies
- may optionally place the extracted objects immediately
- if no destination is given, extracted objects become `Unplaced`

### 12.2 `ReturnToBox`

Destroys runtime instances and restores stock.

Rules:

- only valid for `Unplaced` runtime elements
- restores numeric availability to authored stock
- does not preserve “which exact original copy slot” returned

### 12.3 `UnplaceElement`

Moves placed runtime elements to the `Unplaced` state. Does not destroy the runtime instance.

### 12.4 `PlaceElement`

Places or reparents runtime element(s) into an area/slot.

Rules:

- if already placed elsewhere, it moves automatically
- no separate “remove then place” is required
- if same slot/area is targeted, it can behave as reorder if needed

All of these operations are atomic.

---

## 13. Placement state

A runtime element should not be modeled as “somewhere null”.

It should have explicit placement state such as:

- `Placed`
- `Unplaced`

No nullable placement semantics are required.

This improves:

- serialization
- save/load
- validation
- runtime clarity

---

## 14. Areas and slots

### Areas

Areas are logical locations.

They may be:

- global
- or owned by an element

### Slots

All placement inside an area goes through slots. There is no separate “area content list”.

Rules:

- every area has at least one slot
- if only one slot exists, it is implicitly default
- if multiple slots exist, exactly one must be default

### Placement meaning

- `Area="X"` with no slot specified means “use default slot”
- querying an area means the whole area across all slots
- querying area + slot means only that slot

### Slot capacity

Slots may be:

- single-capacity
- multi-capacity

No type restrictions on slot content yet.

### Single-capacity rules

- one element into a free slot is valid
- many elements into one slot fails
- placing into an occupied single slot fails for now

### Multi-capacity rules

- ordered contents
- batch placement preserves selected order
- one placement rule applies to the whole batch

---

## 15. Area ownership and lifecycle

### Global areas

Always exist independently of any element.

### Owned areas

Defined inside an element definition. They become active only while an instance of that owner is **placed**.

Rules:

- owned runtime areas are created when the owner is placed
- owned runtime areas disappear when the owner stops being placed
- owned areas move with the owner
- if an owner has occupied owned areas, the owner cannot be unplaced/returned

This creates a natural hierarchy:

- owner element
- owned areas
- placed child elements
- their owned areas
- etc.

Removal must happen from leaves upward.

---

## 16. Runtime placement truth

Placement has two representations:

### Structural truth

Slot contents are the structural truth.

A slot knows:

- which element ids it contains
- in what order

### Mirrored cache

Element placement fields mirror:

- area id
- slot id
- order index
- placement state

Both must stay in sync. If they disagree, that is a runtime bug.

---

## 17. Runtime record philosophy

The runtime record types are not yet finalized as C# code, but the intended data content is clear.

### Runtime element record

Should minimally hold:

- element instance id
- definition index
- owner player id or invalid id
- placement state
- area instance id
- slot instance id
- order index
- current face index
- persistent Properties
- Temps

### Runtime area record

Should minimally hold:

- area instance id
- area definition index
- owner element instance id or invalid id
- persistent Properties
- Temps
- slot ids

### Runtime slot record

Should minimally hold:

- slot instance id
- area instance id
- slot definition index
- contained element ids in order

### Runtime player record

Should minimally hold:

- player id
- order index
- persistent Properties
- Temps

---

## 18. Topology

For now, topology is local to one owner element. Areas belonging to different owners are not connected yet.

### Authoring

An element may define:

- owned areas
- local topologies connecting those areas

Topology definitions live inside the element definition as siblings of owned areas.

### Runtime

Runtime topology should use one generic directed graph model:

- nodes = runtime area instances
- links = directed named edges

Even if authored data uses:

- linear paths
- explicit link groups

Runtime should compile both into the same graph model.

### Linear path authoring

A linear path is authoring sugar. It generates directed links named:

- `Forward`

If looped:

- last node links to first with `Forward`

If not looped:

- last node has no outgoing `Forward`

### Link groups

Link groups explicitly define directed named links between areas in the same owner.

### Topology name uniqueness

Topology names must be unique within the owner definition.

---

## 19. Movement

Movement uses topology traversal.

### Start requirements

Move fails unless:

- the element is a runtime element
- it is currently placed
- its current area belongs to the topology owner being used
- the move starts from the owner of the current area

### Link name

Move specifies which named link to follow.

Examples:

- `Forward`
- later maybe `Backward`

### Step resolution rules

At each step:

- if exactly one outgoing link with that name exists, continue
- if zero exist on the **first** step, fail
- if zero exist after at least one step, stop normally
- if more than one matching outgoing link exists, fail for now

### Direction

Topology links are always directed. `A -> B` does not imply `B -> A`.

### End-of-path behavior

If movement requests more steps than can be taken:

- if zero actual steps happen, that is an error
- otherwise movement stops at the last reachable area
- leftover steps are ignored

### Requested vs actual steps

Movement event payload should expose:

- requested steps
- actual steps

---

## 20. Movement events

Current movement event set:

- `OnAreaPassed` -> intermediate traversed areas
- `OnAreaLanded` -> final destination area
- `OnMovementCompleted` -> after movement fully resolves

If moving from A to D through B and C:

- `OnAreaPassed(B)`
- `OnAreaPassed(C)`
- `OnAreaLanded(D)`
- `OnMovementCompleted`

Rules:

- starting area does not fire these
- 0-step move means no movement should happen
- negative steps are unsupported for now

---

## 21. Face model

Each runtime element instance tracks its own current face.

Rules:

- face is runtime state
- all runtime instances track face independently
- even one-face elements still track face normally

Initial face:

- explicit default face if present
- otherwise first authored face

### Face operations

#### `Roll`

- randomizes face for one or many targets
- atomic
- emits one `OnRolled` event for the whole roll
- event exposes total and ordered per-target results

#### `SetFace`

- sets face by face id
- one or many targets
- atomic
- emits `OnFaceChanged`

#### `FlipElement`

- valid only for 2-face elements for now
- toggles to the other face
- one or many targets
- atomic
- emits `OnFaceChanged`

---

## 22. Selectors

Supported selector styles:

- `ByKey`
- `ByTag`
- `ByBinding`

Rules:

- only one selector source per operation
- no combined filters for now
- selectors may resolve to zero, one, or many
- zero results fail for now
- selectors do not decide whether one/many is acceptable
- the consuming operation decides

### `ByBinding`

May resolve to:

- one runtime object/value
- a collection

But there is no implicit conversion:

- wrong runtime type for the operation means failure

`TakeFromBox` uses only:

- `ByKey`
- `ByTag`

No `ByBinding` there for now.

---

## 23. Atomic operations and failure behavior

Important runtime rule: all major operations are **atomic**.

If invalid:

- nothing is partially applied
- operation fails
- execution stops immediately for now

Examples:

- invalid placement
- invalid move
- insufficient stock
- wrong binding type
- ambiguous destination

Later the creator may be able to define alternate flows, but not yet.

---

## 24. Player actions

Phases define candidate player actions.

Each `PlayerAction` may have:

- `when`

This uses the same `Condition` structure as the rest of the system.

### Availability

Available actions are **derived** from state. They are not authoritative saved state.

On load:

- restore match state
- recompute action availability from current phase, current player, properties, temps, and conditions

This is important because action availability may depend on things like:

- remaining action points
- move already used
- flags or property values

---

## 25. Action philosophy

A player performs exactly one action at a time.

An action:

- performs its own core job
- may emit event(s)
- does not hardcode all later consequences inside itself

Consequences are handled by event listeners.

Examples:

- `Roll` just rolls
- movement consequences are handled by the event system

This keeps authored rules cleaner and prevents bundled hidden behaviors.

---

## 26. Events

### Event listeners

A phase may define multiple event rules with the same trigger.

Each event rule may have:

- trigger
- optional `when`
- flat ordered list of operations
- optional `nextPhase`

Matching event rules execute in authored order.

### Nested events

Nested event execution is not allowed.

If operations during an event emit more events:

- those events are queued
- they are processed only after the current event finishes

Queue order is FIFO.

### `nextPhase`

Only event rules may queue `nextPhase`.

Rules:

- `nextPhase` is queued, not applied immediately
- last assignment wins
- phase transition happens only after all queued emitted events finished
- phase transition is the final step of the resolution chain

If no `nextPhase` is assigned:

- the current phase remains active
- if this results in a dead state, that is a ruleset/runtime problem

---

## 27. Temps scopes

Temporary state exists in different scopes.

### Event-local

Valid only during one event execution.

### Turn-local

Valid during one turn. Cleared at turn end.

### Setup-local

Valid during setup.

If something must survive beyond the event:

- it must be explicitly written to broader temp scope or persistent property state

---

## 28. What is intentionally not solved yet

Not part of the first serious implementation:

- external/modular topology connecting different owners
- deep dereference from id-valued properties in bindings
- mid-action save/load
- polymorphic serialized action state
- advanced action sources from perks/equipment/etc.
- nested collections
- mixed-type collections
- slot content restrictions
- swap/replace behavior in single slots
- more advanced topology path choice logic
- failure-branch authoring
- full visual/presentation architecture

These can be added later without invalidating the current runtime core.

---

## 29. First implementation slice

The first runtime slice should focus on:

1. monotonic ids and dense containers
2. match root
3. `Value`
4. `Property` + `Temps`
5. runtime player / element / area / slot records
6. box stock counts
7. `TakeFromBox`
8. `PlaceElement`
9. `UnplaceElement`
10. `ReturnToBox`
11. local topology graph
12. `Move`
13. phase/action/event execution loop
14. stable save/load boundaries

No view logic should be required for this slice.

---

## 30. Final summary

This architecture deliberately favors:

- correctness over cleverness
- serializable state over runtime convenience
- shallow bindings over implicit traversal
- stable save/load boundaries over mid-resolution persistence
- explicit ids over references
- one generic runtime graph for topology
- one consistent runtime state model across match, players, elements, and areas

It is meant to support a first serious gameplay runtime, not every future feature at once.

---

## Addendum: Incremental Runtime Extension Slice

The next runtime slice extends the current controller-centric flow without replacing it.

### A. Visualization stays non-authoritative

- runtime state remains the source of truth
- transcript and interaction data are produced by runtime, not by presentation
- presentation is free to animate, delay, or gate input locally
- runtime resolution does not wait on animation playback

The first adapter can stay text-based. Unity presentation should remain optional and layered on top.

### B. Transcript batches

The runtime now treats a transcript as an ordered record for one resolution pass.

Rules:

- use the term `Transcript`
- keep transcript data flat and serializable
- use many `TranscriptBatch` instances over time
- each batch contains a flat ordered list of `TranscriptEntry`
- a batch ends when runtime reaches `WaitingForPlayerAction`
- a batch also ends when runtime reaches `WaitingForReaction`
- a batch may carry metadata such as stop reason and interaction window id

The runtime may collect raw transcript entries during resolution, then materialize observer-specific batches when the wait boundary is reached.

### C. Interaction windows

Waiting states are now explicit runtime windows, not just implicit controller moments.

Rules:

- every waiting boundary creates a unique interaction window id
- submitted input must reference that window id
- stale input is rejected
- `WaitingForPlayerAction` identifies the acting player for that window
- `WaitingForReaction` identifies the eligible reacting players for that window
- legal actions and reactions are always re-evaluated from current authoritative state

This keeps the current action model intact while making wait boundaries safe and resumable.

### D. Reaction waits

Reactions are treated as another wait boundary rather than a separate execution architecture.

Rules:

- only open `WaitingForReaction` if at least one legal reaction exists
- if no legal reaction exists, continue resolution immediately
- if multiple players can react, first valid submission wins
- a chosen reaction may queue the next phase and hand the next action window to the reacting player

This remains additive to the current phase/action/event loop.

### E. Checkpoint history

Undo/redo is checkpoint-based and linear.

Rules:

- create a checkpoint at every `WaitingForPlayerAction`
- create a checkpoint at every `WaitingForReaction`
- keep exact RNG state in the checkpointed data
- use incremental top-level diffs between checkpoints
- do not restore old transcript playback as part of history restore
- if the user undoes and then continues differently, discard the future branch

The current slice favors top-level segment diffs over a broad rewrite of the runtime state model.

### F. Observer-safe transcript materialization

Hidden information is owned by runtime filtering, not by UI heuristics.

Rules:

- keep stable public runtime ids even when content is hidden
- hide secret content, not object existence
- materialize observer-specific transcript batches immediately at the wait boundary
- let transcript entries carry public fields plus observer-specific overrides when needed

The first slice can start with conservative owner/private redaction hooks and expand from there without changing the batch model.
