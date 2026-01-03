# Afterworld Archive Overview

Afterworld Archive is a Unity 2022.3 LTS prototype that spins up a lightweight third-person scene with a controllable character, camera follow, and procedural ambient audio.

## Project layout

- `Assets/Scenes/Main.unity` - Primary scene used for local play mode.
- `Assets/Scripts/ThirdPersonBootstrap.cs` - Spawns the ground plane, player, and follow camera if they are missing.
- `Assets/Scripts/ThirdPersonController.cs` - Handles third-person movement input and character motion.
- `Assets/Scripts/Audio/AmbientAudioSystem.cs` - Generates procedural wind and hum loops and applies audio mixer volume controls.

## Scene file size

Unity `.unity` scene files are serialized data and tend to be much larger than code files. The `Main.unity` scene is expected to be verbose and should be managed through the Unity editor rather than split manually.

## Runtime systems

- **Bootstrap:** Ensures the minimal scene scaffolding is created at runtime so the project can run without manually placed objects.
- **Movement:** Character movement aligns to the camera's forward axis and supports walking/jogging.
- **Audio:** Procedural audio keeps ambience self-contained without external clip files.
