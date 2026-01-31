# Player Controller System - Documentación Completa

## Índice
1. [Resumen del Sistema](#resumen-del-sistema)
2. [Arquitectura](#arquitectura)
3. [Setup Inicial en Unity](#setup-inicial-en-unity)
4. [Configuración de Layers](#configuración-de-layers)
5. [Configuración de Tags](#configuración-de-tags)
6. [ScriptableObjects](#scriptableobjects)
7. [Configuración del Player GameObject](#configuración-del-player-gameobject)
8. [Configuración de Plataformas](#configuración-de-plataformas)
9. [Sistema de Estados](#sistema-de-estados)
10. [Sistema de Eventos](#sistema-de-eventos)
11. [Sistema de Máscaras](#sistema-de-máscaras)
12. [Controles](#controles)
13. [Parámetros Físicos](#parámetros-físicos)
14. [Troubleshooting](#troubleshooting)

---

## Resumen del Sistema

Sistema de control de personaje para un **auto-runner** con las siguientes características:

| Característica | Descripción |
|----------------|-------------|
| Movimiento automático | El personaje se mueve solo horizontalmente |
| Ping-pong | Rebota automáticamente al tocar paredes |
| Salto | Tap en pantalla o click |
| Doble salto | Segundo tap en el aire |
| Wall Cling | Se agarra a paredes por 0.5s |
| Wall Jump | Salta desde paredes cambiando dirección |
| One-Way Platforms | Plataformas atravesables desde abajo |
| Drop Through | Puede bajar por plataformas one-way |

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                      PlayerController                        │
│  ┌───────────────┐  ┌─────────────┐  ┌───────────────────┐  │
│  │ StateMachine  │  │ PlayerMotor │  │ PlayerEvents      │  │
│  │ (estados)     │  │ (física)    │  │ (ScriptableObject)│  │
│  └───────────────┘  └─────────────┘  └───────────────────┘  │
│                                                              │
│  ┌───────────────┐  ┌─────────────┐                         │
│  │ MaskManager   │  │ PlayerData  │                         │
│  │ (power-ups)   │  │ (config SO) │                         │
│  └───────────────┘  └─────────────┘                         │
└─────────────────────────────────────────────────────────────┘
```

### Estructura de Archivos

```
Assets/Scripts/Gameplay/
├── Player/
│   ├── Interfaces/
│   │   ├── IPlayerState.cs
│   │   └── IPlayerController.cs
│   ├── States/
│   │   ├── PlayerStateBase.cs
│   │   ├── MovingState.cs
│   │   ├── JumpingState.cs
│   │   ├── FallingState.cs
│   │   ├── WallClingState.cs
│   │   └── WallJumpState.cs
│   ├── Events/
│   │   └── PlayerEvents.cs
│   ├── PlayerController.cs
│   ├── PlayerMotor.cs
│   ├── PlayerStateMachine.cs
│   └── PlayerData.cs
│
└── Masks/
    ├── IMask.cs
    ├── MaskBase.cs
    ├── MaskManager.cs
    └── Masks/
        └── ExampleMask.cs (contiene 4 máscaras de ejemplo)
```

---

## Setup Inicial en Unity

### Paso 1: Importar Scripts
Los scripts ya están en `Assets/Scripts/Gameplay/`. Unity los compilará automáticamente.

### Paso 2: Crear Carpeta para ScriptableObjects
```
Assets/
└── Data/
    ├── Player/
    │   ├── PlayerData.asset
    │   └── PlayerEvents.asset
    └── Masks/
        └── (máscaras aquí)
```

---

## Configuración de Layers

### Crear los siguientes Layers:

1. Ir a **Edit → Project Settings → Tags and Layers**
2. En la sección **Layers**, añadir:

| Layer # | Nombre | Uso |
|---------|--------|-----|
| 8 | Ground | Plataformas sólidas (no atravesables) |
| 9 | OneWayPlatform | Plataformas atravesables desde abajo |
| 10 | Wall | Paredes laterales |
| 11 | Player | El jugador (opcional, para colisiones) |

### Configuración Visual:

```
User Layer 8:  [Ground________]
User Layer 9:  [OneWayPlatform]
User Layer 10: [Wall__________]
User Layer 11: [Player________]
```

### Physics2D Layer Collision Matrix (Opcional)

Ir a **Edit → Project Settings → Physics 2D** y configurar qué layers colisionan:

| | Ground | OneWayPlatform | Wall | Player |
|---|:---:|:---:|:---:|:---:|
| **Ground** | - | - | - | ✓ |
| **OneWayPlatform** | - | - | - | ✓ |
| **Wall** | - | - | - | ✓ |
| **Player** | ✓ | ✓ | ✓ | - |

---

## Configuración de Tags

No se requieren tags especiales, pero opcionalmente puedes crear:

| Tag | Uso |
|-----|-----|
| Player | Identificar al jugador |
| Platform | Identificar plataformas |

---

## ScriptableObjects

### PlayerData (Configuración del Jugador)

1. **Crear el asset:**
   - Click derecho en carpeta `Assets/Data/Player/`
   - **Create → Game → Player Data**
   - Nombrar: `PlayerData`

2. **Configurar valores:**

```
┌─────────────────────────────────────────────────────────┐
│ Player Data (ScriptableObject)                          │
├─────────────────────────────────────────────────────────┤
│ ▼ Movement                                              │
│   Move Speed                    [8      ]               │
│                                                         │
│ ▼ Jump                                                  │
│   Jump Force                    [16.5   ]               │
│   Double Jump Force             [16.5   ]               │
│   Max Jumps                     [2      ]               │
│                                                         │
│ ▼ Wall                                                  │
│   Wall Slide Speed              [3      ]               │
│   Wall Cling Duration           [0.5    ]               │
│   Wall Jump Force          X    [8      ]               │
│                            Y    [16.5   ]               │
│                                                         │
│ ▼ Physics                                               │
│   Gravity Scale                 [3      ]               │
│   Fall Multiplier               [2.5    ]               │
│   Low Jump Multiplier           [2      ]               │
│                                                         │
│ ▼ Detection                                             │
│   Ground Layer                  [Ground         ] ☑     │
│   One Way Platform Layer        [OneWayPlatform ] ☑     │
│   Wall Layer                    [Wall           ] ☑     │
│   Ground Check Radius           [0.2    ]               │
│   Wall Check Distance           [0.5    ]               │
└─────────────────────────────────────────────────────────┘
```

### PlayerEvents (Eventos del Jugador)

1. **Crear el asset:**
   - Click derecho en carpeta `Assets/Data/Player/`
   - **Create → Game → Player Events**
   - Nombrar: `PlayerEvents`

2. **Conectar listeners (opcional):**
   - Los eventos se pueden escuchar desde otros scripts
   - O conectar directamente en el Inspector

```
┌─────────────────────────────────────────────────────────┐
│ Player Events (ScriptableObject)                        │
├─────────────────────────────────────────────────────────┤
│ ▼ Movement Events                                       │
│   On Direction Changed          [+ Add Listener]        │
│                                                         │
│ ▼ Jump Events                                           │
│   On Jump                       [+ Add Listener]        │
│   On Land                       [+ Add Listener]        │
│                                                         │
│ ▼ Wall Events                                           │
│   On Wall Hit                   [+ Add Listener]        │
│   On Wall Cling                 [+ Add Listener]        │
│   On Wall Jump                  [+ Add Listener]        │
│                                                         │
│ ▼ State Events                                          │
│   On State Changed              [+ Add Listener]        │
└─────────────────────────────────────────────────────────┘
```

---

## Configuración del Player GameObject

### Estructura del GameObject

```
Player (GameObject)
├── Components:
│   ├── Transform
│   ├── SpriteRenderer
│   ├── Rigidbody2D
│   ├── BoxCollider2D (o CapsuleCollider2D)
│   ├── PlayerController (Script)
│   ├── PlayerMotor (Script)
│   └── MaskManager (Script) [Opcional]
│
└── Children:
    ├── GroundCheck (Transform vacío)
    └── WallCheck (Transform vacío)
```

### Paso a Paso:

#### 1. Crear GameObject Base
- **GameObject → Create Empty** o **2D Object → Sprite**
- Nombrar: `Player`
- Position: (0, 0, 0)

#### 2. Añadir SpriteRenderer
- Si usaste Create Empty, añadir **Component → Rendering → Sprite Renderer**
- Asignar sprite del personaje
- **Order in Layer**: 10 (o el que prefieras)

#### 3. Configurar Rigidbody2D
Añadir **Component → Physics 2D → Rigidbody 2D**

```
┌─────────────────────────────────────────────────────────┐
│ Rigidbody 2D                                            │
├─────────────────────────────────────────────────────────┤
│ Body Type                       [Dynamic      ▼]        │
│ Material                        [None          ]        │
│ Simulated                       [✓]                     │
│                                                         │
│ ▼ Mass                                                  │
│   Use Auto Mass                 [ ]                     │
│   Mass                          [1          ]           │
│                                                         │
│ ▼ Linear                                                │
│   Linear Damping                [0          ]           │
│   Gravity Scale                 [3          ] ← SE SOBRESCRIBE│
│                                                         │
│ ▼ Angular                                               │
│   Angular Damping               [0.05       ]           │
│                                                         │
│ ▼ Collision Detection                                   │
│   Collision Detection           [Continuous  ▼]         │
│   Sleeping Mode                 [Start Awake ▼]         │
│   Interpolate                   [Interpolate ▼]         │
│                                                         │
│ ▼ Constraints                                           │
│   Freeze Position          X [ ] Y [ ]                  │
│   Freeze Rotation          Z [✓] ← IMPORTANTE           │
└─────────────────────────────────────────────────────────┘
```

#### 4. Añadir Collider
Añadir **Component → Physics 2D → Box Collider 2D** (o Capsule)

```
┌─────────────────────────────────────────────────────────┐
│ Box Collider 2D                                         │
├─────────────────────────────────────────────────────────┤
│ Edit Collider                   [Botón]                 │
│ Material                        [None          ]        │
│ Is Trigger                      [ ]                     │
│ Used By Effector                [ ]                     │
│ Used By Composite               [ ]                     │
│ Auto Tiling                     [ ]                     │
│ Offset                     X    [0      ] Y [0      ]   │
│ Size                       X    [1      ] Y [2      ]   │
│ Edge Radius                     [0          ]           │
└─────────────────────────────────────────────────────────┘
```

**Tamaño recomendado:** 1x2 unidades (personaje estándar)

#### 5. Crear GroundCheck (Hijo)
- Click derecho en Player → **Create Empty**
- Nombrar: `GroundCheck`
- **Position Local**: (0, -1, 0) — justo debajo de los pies

```
        Player
           │
     ┌─────┴─────┐
     │           │
     │  Sprite   │
     │           │
     └─────┬─────┘
           │
        (0,-1,0) ← GroundCheck aquí
```

#### 6. Crear WallCheck (Hijo)
- Click derecho en Player → **Create Empty**
- Nombrar: `WallCheck`
- **Position Local**: (0, 0, 0) — centro del personaje

```
        Player
           │
     ┌─────┼─────┐
     │     │     │
WallCheck→ ●     │ ← También detecta a la derecha
     │           │
     └───────────┘
```

#### 7. Añadir Scripts

**PlayerController:**
```
┌─────────────────────────────────────────────────────────┐
│ Player Controller (Script)                              │
├─────────────────────────────────────────────────────────┤
│ ▼ Data                                                  │
│   Data                          [PlayerData      ○]     │
│   Events                        [PlayerEvents    ○]     │
│                                                         │
│ ▼ Initial Settings                                      │
│   Initial Direction             [1      ] ← 1=derecha, -1=izq│
└─────────────────────────────────────────────────────────┘
```

**PlayerMotor:**
```
┌─────────────────────────────────────────────────────────┐
│ Player Motor (Script)                                   │
├─────────────────────────────────────────────────────────┤
│ ▼ References                                            │
│   Ground Check                  [GroundCheck     ○]     │
│   Wall Check                    [WallCheck       ○]     │
│                                                         │
│ ▼ Drop Through                                          │
│   Drop Through Duration         [0.25   ]               │
└─────────────────────────────────────────────────────────┘
```

**MaskManager (Opcional):**
```
┌─────────────────────────────────────────────────────────┐
│ Mask Manager (Script)                                   │
├─────────────────────────────────────────────────────────┤
│ ▼ Events                                                │
│   On Mask Equipped              [+ Add Listener]        │
│   On Mask Unequipped            [+ Add Listener]        │
│   On Mask Ability Used          [+ Add Listener]        │
│                                                         │
│ ▼ Debug                                                 │
│   Starting Mask                 [None            ○]     │
└─────────────────────────────────────────────────────────┘
```

#### 8. Asignar Layer al Player
- Seleccionar Player
- En Inspector, cambiar **Layer** a `Player`
- Cuando pregunte, aplicar a hijos: **No, this object only**

---

## Configuración de Plataformas

### Plataforma Sólida (Ground)

```
Platform_Solid (GameObject)
├── Transform
│   └── Scale: (5, 1, 1) o el tamaño deseado
├── SpriteRenderer
│   └── Sprite: [tu sprite de plataforma]
├── BoxCollider2D
│   └── Size: ajustar al sprite
└── Layer: Ground ← IMPORTANTE
```

**Configuración del Collider:**
```
┌─────────────────────────────────────────────────────────┐
│ Box Collider 2D                                         │
├─────────────────────────────────────────────────────────┤
│ Is Trigger                      [ ]                     │
│ Used By Effector                [ ] ← NO activar        │
└─────────────────────────────────────────────────────────┘
```

### Plataforma One-Way (Atravesable)

```
Platform_OneWay (GameObject)
├── Transform
│   └── Scale: (5, 0.5, 1)
├── SpriteRenderer
│   └── Sprite: [sprite de plataforma fina]
├── BoxCollider2D
│   └── Used By Effector: ✓
├── PlatformEffector2D ← NUEVO
│   └── Use One Way: ✓
└── Layer: OneWayPlatform ← IMPORTANTE
```

**Configuración del Collider:**
```
┌─────────────────────────────────────────────────────────┐
│ Box Collider 2D                                         │
├─────────────────────────────────────────────────────────┤
│ Is Trigger                      [ ]                     │
│ Used By Effector                [✓] ← ACTIVAR           │
└─────────────────────────────────────────────────────────┘
```

**Configuración del Platform Effector 2D:**
```
┌─────────────────────────────────────────────────────────┐
│ Platform Effector 2D                                    │
├─────────────────────────────────────────────────────────┤
│ Use Collider Mask               [ ]                     │
│ Rotational Offset               [0          ]           │
│                                                         │
│ ▼ One Way                                               │
│   Use One Way                   [✓] ← ACTIVAR           │
│   Use One Way Grouping          [ ]                     │
│   Surface Arc                   [180        ]           │
│                                                         │
│ ▼ Sides                                                 │
│   Use Side Friction             [ ]                     │
│   Use Side Bounce               [ ]                     │
│   Side Arc                      [5          ]           │
└─────────────────────────────────────────────────────────┘
```

### Pared (Wall)

```
Wall (GameObject)
├── Transform
│   └── Scale: (1, 10, 1) o altura deseada
├── SpriteRenderer
│   └── Sprite: [sprite de pared]
├── BoxCollider2D
│   └── Size: ajustar al sprite
└── Layer: Wall ← IMPORTANTE
```

---

## Sistema de Estados

### Diagrama de Transiciones

```
                         ┌─────────────┐
                         │   MOVING    │◄─────────────────┐
                         │  (ground)   │                  │
                         └──────┬──────┘                  │
                                │                         │
                    ┌───────────┼───────────┐             │
                    │           │           │             │
               jump │      no ground   wall hit           │
                    │           │           │             │
                    ▼           ▼           ▼             │
             ┌──────────┐ ┌──────────┐ ┌──────────┐       │
             │ JUMPING  │ │ FALLING  │ │  WALL    │       │
             │          │ │          │ │  CLING   │       │
             └────┬─────┘ └────┬─────┘ └────┬─────┘       │
                  │            │            │             │
             apex │       ground│      jump │             │
                  │            │            │             │
                  ▼            │            ▼             │
             ┌──────────┐      │      ┌──────────┐        │
             │ FALLING  │──────┴─────▶│  WALL    │        │
             │          │   wall hit  │  JUMP    │        │
             └────┬─────┘             └────┬─────┘        │
                  │                        │              │
             ground                    ground/fall        │
                  │                        │              │
                  └────────────────────────┴──────────────┘
```

### Descripción de Estados

| Estado | Entrada | Comportamiento | Salidas |
|--------|---------|----------------|---------|
| **Moving** | Tocar suelo | Movimiento horizontal auto, resetea jumpCount | Jump→Jumping, Sin suelo→Falling, Pared→WallCling |
| **Jumping** | Saltar | Ascenso, puede hacer doble salto | Apex→Falling, Pared→WallCling |
| **Falling** | Velocidad Y ≤ 0 | Caída, puede hacer doble salto | Suelo→Moving, Pared→WallCling |
| **WallCling** | Tocar pared en aire | Gravedad 0 por 0.5s | Jump→WallJump, Timeout→Falling, Suelo→Moving |
| **WallJump** | Saltar desde pared | Impulso diagonal, cambia dirección | Apex→Falling, Pared→WallCling, Suelo→Moving |

### Anti-Vuelo Infinito

```
jumpCount se resetea SOLO al tocar suelo (MovingState.Enter)

GROUNDED:     jumpCount = 0
Tap:          jumpCount = 1 (salto)
Tap en aire:  jumpCount = 2 (doble salto)
jumpCount ≥ 2: Tap ignorado

Wall Cling:   NO resetea jumpCount
Wall Jump:    NO resetea jumpCount (evita exploit)
```

---

## Sistema de Eventos

### Eventos Disponibles

| Evento | Parámetro | Cuándo se dispara | Uso típico |
|--------|-----------|-------------------|------------|
| `OnDirectionChanged` | `int direction` (-1 o 1) | Al rebotar en pared o wall jump | Flip sprite, audio |
| `OnJump` | `int jumpCount` (1 o 2) | Al saltar o doble saltar | VFX, audio |
| `OnLand` | `float fallSpeed` | Al aterrizar | Dust VFX, audio |
| `OnWallHit` | `Vector2 normal` | Al tocar pared | Partículas, audio |
| `OnWallCling` | `Vector2 position` | Al agarrarse a pared | Animación |
| `OnWallJump` | `int newDirection` | Al saltar de pared | VFX especial |
| `OnStateChanged` | `PlayerState state` | Al cambiar estado | Debug, analytics |

### Ejemplo: Conectar Audio

```csharp
// AudioManager.cs
public class AudioManager : MonoBehaviour
{
    [SerializeField] private PlayerEvents playerEvents;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip wallHitSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        playerEvents.OnJump.AddListener(PlayJumpSound);
        playerEvents.OnLand.AddListener(PlayLandSound);
        playerEvents.OnWallHit.AddListener(PlayWallHitSound);
    }

    private void OnDisable()
    {
        playerEvents.OnJump.RemoveListener(PlayJumpSound);
        playerEvents.OnLand.RemoveListener(PlayLandSound);
        playerEvents.OnWallHit.RemoveListener(PlayWallHitSound);
    }

    private void PlayJumpSound(int jumpCount)
    {
        audioSource.PlayOneShot(jumpSound);
    }

    private void PlayLandSound(float fallSpeed)
    {
        if (fallSpeed > 5f)
            audioSource.PlayOneShot(landSound);
    }

    private void PlayWallHitSound(Vector2 normal)
    {
        audioSource.PlayOneShot(wallHitSound);
    }
}
```

---

## Sistema de Máscaras

### Crear una Máscara Nueva

1. Crear script que herede de `MaskBase`:

```csharp
using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    [CreateAssetMenu(fileName = "MyMask", menuName = "Game/Masks/My Mask")]
    public class MyMask : MaskBase
    {
        [Header("My Mask Settings")]
        [SerializeField] private float myValue = 1.5f;

        public override void OnEquip(PlayerController player)
        {
            Debug.Log("Máscara equipada!");
        }

        public override void OnUnequip(PlayerController player)
        {
            Debug.Log("Máscara desequipada!");
        }

        public override void OnUpdate(PlayerController player)
        {
            // Llamado cada frame mientras está equipada
        }

        public override void ModifyJump(ref float jumpForce)
        {
            jumpForce *= myValue;
        }

        public override void ModifySpeed(ref float speed)
        {
            // Modificar velocidad
        }

        public override void ModifyWallCling(ref float duration)
        {
            // Modificar duración de wall cling
        }
    }
}
```

2. Crear ScriptableObject:
   - Click derecho → **Create → Game → Masks → My Mask**
   - Configurar valores en Inspector

### Máscaras de Ejemplo Incluidas

| Máscara | Menú | Efecto |
|---------|------|--------|
| `TripleJumpMask` | Game/Masks/Triple Jump Mask | Añade +1 salto extra |
| `SpeedMask` | Game/Masks/Speed Mask | Multiplica velocidad x1.5 |
| `WallClimberMask` | Game/Masks/Wall Climber Mask | +1s de wall cling |
| `HighJumpMask` | Game/Masks/High Jump Mask | Salto x1.3 más alto |

### Equipar Máscara por Código

```csharp
// Obtener referencia al MaskManager
MaskManager maskManager = player.GetComponent<MaskManager>();

// Equipar máscara
maskManager.EquipMask(myMaskAsset);

// Desequipar
maskManager.UnequipCurrentMask();

// Verificar si tiene máscara
if (maskManager.HasMask)
{
    IMask current = maskManager.CurrentMask;
}
```

---

## Controles

### Móvil
| Input | Acción |
|-------|--------|
| Tap en pantalla | Salto / Doble salto |
| Swipe hacia abajo | Drop through (plataformas one-way) |

### PC/Editor
| Input | Acción |
|-------|--------|
| Click izquierdo | Salto / Doble salto |
| Espacio | Salto / Doble salto |
| S / Flecha abajo | Drop through |

---

## Parámetros Físicos

### Valores Recomendados

| Parámetro | Valor | Notas |
|-----------|-------|-------|
| Gravity Scale | 3 | Gravedad aumentada para juego ágil |
| Move Speed | 8 | Unidades por segundo |
| Jump Force | 16.5 | Altura ~4.5 unidades |
| Double Jump Force | 16.5 | Igual que salto normal |
| Wall Jump Force | (8, 16.5) | X=horizontal, Y=vertical |
| Wall Cling Duration | 0.5s | Tiempo agarrado a pared |
| Wall Slide Speed | 3 | Velocidad al deslizar (no usado actualmente) |
| Fall Multiplier | 2.5 | Caída más rápida |
| Low Jump Multiplier | 2 | Salto corto si suelta rápido |
| Ground Check Radius | 0.2 | Radio de detección de suelo |
| Wall Check Distance | 0.5 | Distancia de detección de pared |

### Cálculo de Altura de Salto

Con `gravityScale = 3` y `jumpForce = 16.5`:

```
Altura = jumpForce² / (2 * Physics2D.gravity.y * gravityScale)
Altura = 16.5² / (2 * 9.81 * 3)
Altura ≈ 4.6 unidades
```

---

## Troubleshooting

### El personaje no detecta el suelo

1. ✓ Verificar que GroundCheck existe y está posicionado correctamente
2. ✓ Verificar que el Layer del suelo está asignado en PlayerData
3. ✓ Verificar que Ground Check Radius es suficiente (0.2 recomendado)
4. ✓ Verificar que la plataforma tiene collider

### El personaje atraviesa paredes

1. ✓ Verificar Layer de la pared = Wall
2. ✓ Verificar que Wall Layer está asignado en PlayerData
3. ✓ Rigidbody2D con Collision Detection = Continuous
4. ✓ La pared tiene BoxCollider2D

### Wall Cling no funciona

1. ✓ Verificar que WallCheck está asignado en PlayerMotor
2. ✓ Wall Check Distance suficiente (0.5 recomendado)
3. ✓ El personaje debe estar moviéndose HACIA la pared

### Plataformas one-way no funcionan

1. ✓ Layer = OneWayPlatform
2. ✓ BoxCollider2D con "Used By Effector" = ✓
3. ✓ PlatformEffector2D añadido
4. ✓ "Use One Way" = ✓
5. ✓ Layer asignado en PlayerData.oneWayPlatformLayer

### El personaje no rebota en paredes

1. ✓ La pared tiene Layer = Wall
2. ✓ El estado actual es MovingState (solo rebota en suelo)
3. ✓ El contacto es lateral (normal.x > 0.5)

### Doble salto no funciona

1. ✓ Max Jumps = 2 en PlayerData
2. ✓ El salto anterior incrementó jumpCount
3. ✓ jumpCount se resetea al tocar suelo

### Debug Visual

En el Editor, selecciona el Player para ver:
- **Círculo verde**: Ground Check (si detecta suelo)
- **Líneas azules**: Wall Check (raycast a izquierda y derecha)

### Debug en Pantalla

Durante el Play Mode aparece información en la esquina superior izquierda:
```
State: Moving
Direction: 1
Jump Count: 0/2
Grounded: True
One-Way Platform: False
Wall: False (Dir: 0)
Velocity: (8.0, 0.0)
```

---

## Checklist Final

- [ ] Layers creados: Ground, OneWayPlatform, Wall, Player
- [ ] PlayerData.asset creado y configurado
- [ ] PlayerEvents.asset creado
- [ ] Player GameObject con todos los componentes
- [ ] GroundCheck hijo posicionado en pies
- [ ] WallCheck hijo posicionado en centro
- [ ] PlayerController con referencias asignadas
- [ ] PlayerMotor con referencias asignadas
- [ ] Al menos una plataforma Ground para probar
- [ ] Al menos una pared Wall para probar
- [ ] (Opcional) Plataforma OneWayPlatform con Platform Effector 2D

---

## Próximos Pasos Sugeridos

1. **Añadir Animaciones**: Usar el evento `OnStateChanged` para cambiar animaciones
2. **Añadir Audio**: Suscribirse a eventos para reproducir sonidos
3. **Añadir VFX**: Partículas al saltar, aterrizar, wall jump
4. **Crear Máscaras**: Implementar power-ups únicos para el juego
5. **Input Touch Mejorado**: Añadir swipe para drop-through en móvil
