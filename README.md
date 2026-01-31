# Global Game Jam 2026

Unity Mobile 2D Game with 3D Props

## Project Structure

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/              # Core systems (GameManager, Events, etc)
    │   ├── Gameplay/          # Game mechanics and logic
    │   ├── UI/                # UI controllers and components
    │   ├── Utilities/         # Helper classes and extensions
    │   ├── Audio/             # Audio management
    │   ├── Input/             # Input handling (touch, etc)
    │   ├── Managers/          # Singleton managers
    │   └── Data/              # Data structures and models
    │
    ├── Scenes/
    │   ├── Main/              # Main menu, splash
    │   ├── Levels/            # Game levels
    │   ├── UI/                # UI-only scenes
    │   └── Test/              # Testing scenes
    │
    ├── Prefabs/
    │   ├── Characters/        # Player, enemies, NPCs
    │   ├── Props/             # Interactive objects
    │   ├── UI/                # UI prefabs
    │   ├── Effects/           # VFX prefabs
    │   └── Environment/       # Background, platforms
    │
    ├── Art/
    │   ├── Sprites/
    │   │   ├── Characters/    # Character sprites/spritesheets
    │   │   ├── Props/         # 2D props
    │   │   ├── UI/            # UI graphics
    │   │   ├── Environment/   # Backgrounds, tiles
    │   │   └── Effects/       # Particle textures
    │   ├── Animations/
    │   │   ├── Characters/    # Character animations
    │   │   └── UI/            # UI animations
    │   ├── Materials/         # 2D materials
    │   └── Fonts/             # Custom fonts
    │
    ├── Art3D/
    │   ├── Models/
    │   │   ├── Props/         # 3D prop models
    │   │   └── Environment/   # 3D environment pieces
    │   ├── Materials/         # 3D materials
    │   └── Textures/          # 3D textures
    │
    ├── Audio/
    │   ├── Music/             # Background music
    │   ├── SFX/               # Sound effects
    │   └── Ambience/          # Ambient sounds
    │
    ├── Resources/             # Runtime-loaded assets
    ├── ScriptableObjects/
    │   ├── GameData/          # Game configuration data
    │   └── Configs/           # System configs
    │
    ├── Plugins/
    │   ├── Android/           # Android native plugins
    │   └── iOS/               # iOS native plugins
    │
    ├── Editor/                # Editor scripts
    └── StreamingAssets/       # Platform-specific data
```

## Tech Stack

- **Unity Version**: 2022.3 LTS (or newer)
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Target Platforms**: Android, iOS

## Team

Global Game Jam 2026 Team
