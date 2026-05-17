# Additional Movement Game States

Here are common 2D platformer movement-related game states that are missing from your current list:

## Wall & Ledge States
* **wallSliding** / **wallClinging**: Currently tracked in `HeroControllerStates.wallSliding` and `ActorStates.wall_sliding`.
* **wallJumping**: A specific momentary state where the player is springing off a wall. (Kept as requested).

## Movement States Not Needed / To Be Removed
The following states are currently in your codebase (`ActorStates` or `HeroControllerStates`) but are completely **unused** (or highly situational to mechanics not currently implemented) and can be removed:

* **downSpikeRecovery** (`HeroControllerStates`): Unused hazard mechanic.
* **inWalkZone** (`HeroControllerStates`): Unused forced walking area mechanic.
* **isTouchingSlopeLeft** / **isTouchingSlopeRight** (`HeroControllerStates`): Handled natively by Unity physics just fine, not being effectively utilized right now.
* **hard_landing** (`ActorStates`): Never set or checked.
* **dash_landing** (`ActorStates`): Never set or checked.
* **no_input** (`ActorStates`): Never set or checked.
* **idle** (`ActorStates`): Fully unused in the script logic right now.
* **running** (`ActorStates`): Fully unused in the script logic right now.
* **airborne** (`ActorStates`): Fully unused in the script logic right now.
