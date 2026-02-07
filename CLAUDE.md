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

### AudioManager Setup

Location: `Assets/Scripts/Gameplay/Audio/AudioManager.cs`

1. **Create AudioManager GameObject:**
   - Create an empty GameObject named "AudioManager"
   - Add the `AudioManager` component
   - It will auto-create child AudioSources (Music, Ambient, SFX)
   - Mark as `DontDestroyOnLoad` automatically

2. **Assign AudioClips in Inspector:**
   - **Music:** Main theme
   - **Character:** Jump, Death, Wall Scratch, Footsteps (Amethyst[], BurningRock[])
   - **Props:** Crystal, Mask[] (randomly selected)
   - **Stage:** Breaking Rock, Crystal Breaking, Levitating Mask
   - **Environment:** Wind, Wind Fire, Lava+Wind+Fire, Lava Alone, Glass Environment

3. **Available Audio Files:**
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

4. **How to call from other scripts:**
   ```csharp
   using Gameplay.Audio;

   // Music
   AudioManager.Instance.PlayMusic();
   AudioManager.Instance.StopMusic();

   // Ambient (loops)
   AudioManager.Instance.PlayLavaAmbient();
   AudioManager.Instance.PlayWindAmbient();
   AudioManager.Instance.StopAmbient();

   // SFX - Character
   AudioManager.Instance.PlayJump();
   AudioManager.Instance.PlayDeath();
   AudioManager.Instance.PlayWallScratch();
   AudioManager.Instance.PlayFootstepAmethyst();
   AudioManager.Instance.PlayFootstepBurningRock();

   // SFX - Props
   AudioManager.Instance.PlayMaskPickup();
   AudioManager.Instance.PlayCrystal();

   // SFX - Stage
   AudioManager.Instance.PlayBreakingRock();
   AudioManager.Instance.PlayCrystalBreaking();

   // Volume
   AudioManager.Instance.SetMasterVolume(0.8f);
   AudioManager.Instance.SetMusicVolume(0.5f);
   ```
