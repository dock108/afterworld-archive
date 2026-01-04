# AGENTS.md — Afterworld Archive

> This file provides context for AI agents (Codex, Cursor, Copilot) working on this codebase.

## Quick Context

**What is this?** Unity 2022.3 LTS prototype—a creature-discovery exploration game with third-person controller, scanning mechanics, and procedural ambient audio.

**Tech Stack:** Unity 2022.3, C#, Universal Render Pipeline (URP)

**Key Directories:**
- `Assets/Scripts/` — All game logic
- `Assets/Scripts/Creatures/` — Creature data, encounters, instincts
- `Assets/Scripts/Scanning/` — Hold-to-scan mechanics
- `Assets/Scripts/Systems/` — Save system, performance tuning, mini-games
- `Assets/Scripts/UI/` — All UI controllers
- `Assets/Scripts/Audio/` — Procedural ambient audio
- `Assets/Resources/` — ScriptableObject databases and creature definitions
- `Assets/Scenes/` — Unity scene files

## Core Game Systems

1. **Third-Person Controller** — WASD movement with camera-relative direction
2. **Creature Encounters** — Discover and catalog creatures in the world
3. **Scanning System** — Hold-to-scan mechanic for logging discoveries
4. **Archive/Bestiary** — Progressive unlock of creature knowledge
5. **Procedural Audio** — Ambient soundscape without external audio files

## Coding Standards

### C# Conventions
- Follow Unity C# style (PascalCase for public members, camelCase for private)
- Use `[SerializeField]` for inspector-exposed private fields
- Prefer composition over inheritance
- Keep MonoBehaviours focused—single responsibility
- Use ScriptableObjects for data definitions

### Unity Best Practices
- Never access `Camera.main` in hot loops (cache the reference)
- Use `[RequireComponent]` to declare dependencies
- Null-check `Camera.main` for headless/test scenarios
- Keep Update() methods lean—heavy logic in coroutines or systems

### Documentation
- XML documentation comments on public APIs
- `[Header("Section")]` attributes to organize inspector fields
- Inline comments for non-obvious logic

## Do NOT

- Manually edit `.unity` scene files as text—use the Unity editor
- Modify `.meta` files directly
- Add external asset store packages without discussion
- Use `GameObject.Find()` in Update loops
- Hardcode magic numbers—use `[SerializeField]` constants

## ScriptableObject Patterns

Creatures and game data use ScriptableObjects:

```csharp
[CreateAssetMenu(menuName = "Afterworld/Creatures/Creature Data")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private CreatureRarity rarity;
    // ...
}
```

When adding new data types:
1. Create a ScriptableObject class
2. Add `[CreateAssetMenu]` attribute
3. Place assets in `Assets/Resources/` or appropriate subfolder

## Testing

Unity doesn't have built-in unit testing in this project yet. For now:
- Test in Play Mode manually
- Use Debug.Log for verification
- Consider adding Unity Test Framework if scope grows

## Development Setup

1. Open with **Unity 2022.3.10f1** (or compatible 2022.3 LTS)
2. Load scene: `Assets/Scenes/Main.unity`
3. Enter Play Mode to test

## Related Documentation

- `docs/overview.md` — Project layout and runtime systems

