# Project Guidelines

## UI Management

### UIManager Setup

1. **Create UIManager GameObject:**
   - Create an empty GameObject in your scene (e.g., name it "UIManager")
   - Add the `UIManager` component (`Assets/Scripts/Gameplay/UI/UIManager.cs`)

2. **Assign Panels (create as needed):**
   - **Mask Info Panel** - shown when player picks up mask, dismissed by tap/swipe
   - **Death Panel** - shown when player dies, dismissed by tap/swipe
   - **Next Level Panel** - shown when reaching next level
   - **Lava Warning Panel** - auto-hides after duration (default 2s)

3. **MaskPickup Setup:**
   - Create a GameObject with a 2D Collider (set as Trigger) + Rigidbody2D (Kinematic)
   - Add `MaskPickup` component (`Assets/Scripts/Gameplay/Interactables/MaskPickup.cs`)
   - Check **"Equip On Pickup"** to freeze player and play animation
   - Check **"Show Mask Info Panel"** to display the panel
   - Player stays frozen until panel is dismissed

4. **How to call from other scripts:**
   ```csharp
   using Gameplay.UI;

   UIManager.Instance.ShowMaskInfoPanel();   // Show mask info
   UIManager.Instance.ShowDeathPanel();      // Show death message
   UIManager.Instance.ShowLavaWarning();     // Show lava warning
   UIManager.Instance.ShowNextLevelPanel();  // Show next level
   ```

5. **Events you can hook into:**
   - `OnMaskInfoShown` / `OnMaskInfoDismissed`
   - `OnDeathShown` / `OnDeathDismissed`
   - `OnNextLevelShown` / `OnNextLevelDismissed`
   - `OnLavaWarningShown` / `OnLavaWarningHidden`

### Lava Rising

The `RisingLava` component (`Assets/Scripts/Gameplay/Hazards/RisingLava.cs`) supports multiple start modes:
- **Manual** - call `StartRising()` from code
- **OnAwake** - starts immediately
- **OnFirstJump** - starts on player's first jump
- **OnFirstJumpAfterMask** - starts on first jump after player touches MaskPickup

To show lava warning when lava starts rising, hook into `RisingLava` or call `UIManager.Instance.ShowLavaWarning()`.

## Audio Management

Audio uses a **centralized volume system** with 3 specialized managers:

### VolumeManager (Central Volume Control)

Location: `Assets/Scripts/Gameplay/Audio/VolumeManager.cs`

- **Static class** - no GameObject needed, access from anywhere
- Stores Master, Music, and SFX volumes in PlayerPrefs
- All 3 audio managers read from VolumeManager automatically
- Fires `OnVolumeChanged` event when any value changes

**Usage from any script:**
```csharp
using Gameplay.Audio;

// Set volumes (0-1 range, auto-saved to PlayerPrefs)
VolumeManager.MasterVolume = 0.8f;
VolumeManager.MusicVolume = 0.5f;
VolumeManager.SFXVolume = 1f;

// Get final computed volumes (category * master)
float music = VolumeManager.GetMusicVolume(); // 0.4f (0.5 * 0.8)
float sfx = VolumeManager.GetSFXVolume();     // 0.8f (1.0 * 0.8)

// Persist to disk (call after slider adjustments)
VolumeManager.Save();

// Subscribe to changes
VolumeManager.OnVolumeChanged += MyCallback;
```

### 1. AudioManager (Music)

Location: `Assets/Scripts/Gameplay/Audio/AudioManager.cs`

- Handles background music with crossfade between calm/intense themes
- Volume: `VolumeManager.GetMusicVolume() * duckMultiplier`
- Auto-subscribes to `VolumeManager.OnVolumeChanged`
- DontDestroyOnLoad

**Setup:**
1. Create GameObject "AudioManager"
2. Add `AudioManager` component
3. Assign calm and intense theme clips

**Usage:**
```csharp
AudioManager.Instance.PlayCalmTheme();
AudioManager.Instance.PlayIntenseTheme();
AudioManager.Instance.StopMusic();
AudioManager.Instance.DuckMusic(0.2f);
AudioManager.Instance.RestoreMusicVolume();
```

### 2. PlayerAudioManager (Player Sounds)

Location: `Assets/Scripts/Gameplay/Audio/PlayerAudioManager.cs`

- Handles jump, death, footsteps, wall scratch
- Volume: `VolumeManager.GetSFXVolume()` (reads on each PlayOneShot)
- Auto-hooks into PlayerEvents (jump, wall cling, state changes)

**Setup:**
1. Add to Player GameObject
2. Assign `PlayerEvents` ScriptableObject
3. Assign clips: Jump, Death, Wall Scratch, Steps arrays

**Usage:**
```csharp
PlayerAudioManager.Instance.PlayJump();
PlayerAudioManager.Instance.PlayDeath();
PlayerAudioManager.Instance.PlayWallScratch();
PlayerAudioManager.Instance.PlayFootstepAmethyst();
```

### 3. EnvironmentAudioManager (Ambient & Stage)

Location: `Assets/Scripts/Gameplay/Audio/EnvironmentAudioManager.cs`

- Handles ambient loops and stage/prop SFX
- Volume: `VolumeManager.GetSFXVolume()` for both ambient and SFX
- Auto-subscribes to `VolumeManager.OnVolumeChanged` for live ambient updates
- DontDestroyOnLoad

**Setup:**
1. Create GameObject "EnvironmentAudioManager"
2. Add `EnvironmentAudioManager` component
3. Assign ambient and SFX clips

**Usage:**
```csharp
// Ambient (loops)
EnvironmentAudioManager.Instance.PlayLavaAmbient();
EnvironmentAudioManager.Instance.PlayWindAmbient();
EnvironmentAudioManager.Instance.StopAmbient();

// Stage SFX
EnvironmentAudioManager.Instance.PlayBreakingRock();
EnvironmentAudioManager.Instance.PlayCrystalBreaking();

// Props SFX
EnvironmentAudioManager.Instance.PlayCrystal();
```

### Audio Files Reference
```
Assets/_Project/Audio/
├── ggjMainTheme.wav
├── Character/
│   ├── Jump.wav, Death.wav, Scratch Wall.wav
│   ├── Step Amatist 1.wav, Step Amatist 2.wav
│   └── Step burning rock 1.wav, Step burning rock 2.wav
├── Props/
│   ├── Crystal.wav
│   └── Mask 1.wav, Mask 2.wav, Mask 3.wav
├── Stage/
│   ├── breaking rock.wav, Crystal breaking.wav
│   └── Levitating Mask.wav
└── Environment/
    ├── Wind.wav, Wind fire.wav
    ├── Lava and wind fire. The best ;).wav
    ├── Lava alone.wav
    └── Glass environment.wav
```
