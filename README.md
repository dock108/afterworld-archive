# Vestige

> *Previously "Afterworld Archive" — renamed to better reflect the project's direction toward memory, discovery, and interactive exploration.*

**Vestige** is a conceptual Unity prototype exploring narrative systems and world-building mechanics. It's an experimental sandbox for creature discovery, environmental storytelling, and archive-based progression.

## What This Prototype Explores

- **Creature Discovery** — Encounter and catalog mysterious beings in an open world
- **Scanning Mechanics** — Hold-to-scan system for logging discoveries and building knowledge
- **Archive Progression** — Unlock creature lore, behaviors, and instincts through repeated encounters
- **Procedural Ambience** — Self-contained audio atmosphere without external sound files
- **Memory Systems** — Save/load progression tied to what you've discovered

## What You Can Do Right Now

1. **Move** — Third-person controller with WASD movement and camera-relative direction
2. **Explore** — Navigate a minimal scene with procedural ambient audio
3. **Scan** — Hold to scan creatures and objects in the world
4. **Catalog** — Build an archive of creature knowledge through discovery

This is a **work-in-progress prototype**. Systems are functional but minimal — the focus is on exploring game feel and narrative mechanics, not production polish.

## Development Status

🧪 **Conceptual / Experimental**

This project is a creative sandbox for testing ideas. Expect rough edges, placeholder content, and systems that may change significantly.

## Run Locally

1. Open with **Unity 6.3 LTS** (6000.3.2f1 or compatible)
2. Load scene: `Assets/Scenes/Main.unity`
3. Enter Play Mode

### Troubleshooting

If Unity fails to open the project with errors like `GetManagerFromContext` / `MonoManager`:

- Quit Unity
- Delete `Library/`, `Temp/`, and `obj/` in the project folder
- Re-open the project so Unity regenerates package caches/lock state

## Build

Use Unity's standard build pipeline (File → Build Settings). Ensure `Main.unity` is included in build settings.

## Documentation

See [`/docs`](docs/overview.md) for technical details and system breakdowns.

---

<details>
<summary>Historical Note</summary>

This project was originally called "Afterworld Archive" with an underworld/afterlife theme. The direction shifted toward memory and exploration, making the old name no longer representative.

**January 2026:** Upgraded from Unity 2022.3 LTS to Unity 6.3 LTS for macOS 26 Tahoe compatibility and native Apple Silicon support.

</details>
