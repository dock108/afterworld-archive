# Vestige — Project Overview

Vestige is a Unity 6.3 LTS prototype exploring creature discovery, archive-based progression, and environmental storytelling through interactive exploration.

## Creative Direction

**Core Experience:** Wander through a world, encounter mysterious creatures, and build understanding through observation and cataloging.

**Tone:** Atmospheric, curious, unhurried. Discovery over combat. Memory over conquest.

**Key Themes:**
- Traces left behind — remnants that tell stories
- Building knowledge through presence and attention
- The archive as both goal and narrative device

## What's Implemented

### Core Systems

| System | Status | Description |
|--------|--------|-------------|
| Third-Person Controller | ✅ Working | WASD movement, camera-relative direction |
| Camera Follow | ✅ Working | Smooth follow camera with Cinemachine 3 |
| Procedural Audio | ✅ Working | Ambient soundscape generated at runtime |
| Creature Encounters | ✅ Working | Spawn and interact with creatures |
| Hold-to-Scan | ✅ Working | Scan targets to log discoveries |
| Archive/Bestiary | ✅ Working | Progressive unlock of creature knowledge |
| Save System | ✅ Working | Persist player progress |

### Creature Data

Six sample creatures with full data definitions:
- Ember Spiral
- Glowcap Sprinter
- Hollow Mossback
- Lumenhart Stag
- Mistwing Skiff
- Thistle Pouncer

Each creature has: rarity, encounter notes, behaviors, and instinct data.

## Project Layout

```
Assets/
├── Scripts/
│   ├── ThirdPersonBootstrap.cs    — Scene scaffolding at runtime
│   ├── ThirdPersonController.cs   — Player movement
│   ├── Audio/                     — Procedural ambient audio
│   ├── Creatures/                 — Creature data, encounters, instincts
│   ├── Scanning/                  — Hold-to-scan mechanics
│   ├── Systems/                   — Save system, mini-games, performance
│   └── UI/                        — All UI controllers
├── Resources/
│   ├── CreatureDatabase.asset     — Central creature registry
│   └── Creatures/                 — Individual creature definitions
└── Scenes/
    └── Main.unity                 — Primary scene
```

## Runtime Behavior

- **Bootstrap:** `ThirdPersonBootstrap.cs` ensures minimal scene scaffolding exists at runtime — ground plane, player, camera — so the project runs without manually placed objects.
- **Movement:** Character movement aligns to camera forward axis. Supports walking/jogging.
- **Audio:** Procedural wind and hum loops. No external audio files required.
- **Scanning:** Hold input on valid targets to fill a scan meter and log discoveries.
- **Archive:** Scanned creatures unlock progressive knowledge tiers.

## Technical Notes

### Scene Files

Unity `.unity` files are serialized data and can be large. `Main.unity` should be edited through the Unity editor, not manually.

### ScriptableObject Patterns

Game data uses ScriptableObjects for easy authoring:

```csharp
[CreateAssetMenu(menuName = "Vestige/Creatures/Creature Data")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private CreatureRarity rarity;
}
```

### Mobile Considerations

- Target 60fps on mid-range devices
- `MobilePerformanceTuner` adjusts quality at runtime
- Object pooling for frequently spawned entities
- Minimize per-frame allocations

## Development Setup

1. Open with **Unity 6.3 LTS** (6000.3.2f1 or compatible)
2. Load scene: `Assets/Scenes/Main.unity`
3. Enter Play Mode to test
