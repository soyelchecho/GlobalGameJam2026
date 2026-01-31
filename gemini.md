# Global Game Jam 2026 - Project Guidelines

## Información del Proyecto

| Campo | Valor |
|-------|-------|
| **Nombre** | Global Game Jam 2026 |
| **Engine** | Unity (2D) |
| **Género** | Auto-runner vertical/horizontal con platforming |
| **Plataforma** | Mobile (iOS/Android) + PC |
| **Input** | Touch (tap) / Mouse click / Keyboard |

---

## Resumen del Juego

Auto-runner 2D donde el jugador:
- Se mueve automáticamente de forma horizontal
- Rebota en paredes (ping-pong)
- Salta con tap/click (salto + doble salto)
- Se agarra a paredes temporalmente (wall cling 0.5s)
- Salta desde paredes cambiando dirección (wall jump)
- Puede equipar máscaras (power-ups) que modifican habilidades

---

## Arquitectura del Código

### Patrón Principal: State Machine + Event System + ScriptableObjects

```
┌─────────────────────────────────────────────────────────┐
│                    PlayerController                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │ StateMachine │  │ PlayerMotor │  │ PlayerEvents    │  │
│  │ (estados)    │  │ (física)    │  │ (UnityEvents)   │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
                           │
            ┌──────────────┼──────────────┐
            ▼              ▼              ▼
    ┌───────────┐  ┌───────────┐  ┌───────────┐
    │ AudioMgr  │  │ VFXMgr    │  │ Analytics │
    └───────────┘  └───────────┘  └───────────┘
```

### Principios de Diseño

1. **Separación de responsabilidades**
   - `PlayerController`: Coordinación, input, referencias
   - `PlayerMotor`: Física y movimiento (Rigidbody2D)
   - `PlayerStateMachine`: Gestión de estados
   - `PlayerEvents`: Comunicación desacoplada

2. **ScriptableObjects para configuración**
   - `PlayerData`: Parámetros del jugador (velocidad, salto, etc.)
   - `PlayerEvents`: Canal de eventos compartido
   - `MaskBase`: Definición de power-ups

3. **Composición sobre herencia**
   - Estados implementan `IPlayerState`
   - Máscaras implementan `IMask`
   - Componentes modulares en el Player

---

## Estructura de Carpetas

```
Assets/
├── Scripts/
│   └── Gameplay/
│       ├── Player/
│       │   ├── Interfaces/
│       │   │   ├── IPlayerState.cs
│       │   │   └── IPlayerController.cs
│       │   ├── States/
│       │   │   ├── PlayerStateBase.cs
│       │   │   ├── MovingState.cs
│       │   │   ├── JumpingState.cs
│       │   │   ├── FallingState.cs
│       │   │   ├── WallClingState.cs
│       │   │   └── WallJumpState.cs
│       │   ├── Events/
│       │   │   └── PlayerEvents.cs
│       │   ├── PlayerController.cs
│       │   ├── PlayerMotor.cs
│       │   ├── PlayerStateMachine.cs
│       │   └── PlayerData.cs
│       │
│       ├── Masks/
│       │   ├── IMask.cs
│       │   ├── MaskBase.cs
│       │   ├── MaskManager.cs
│       │   └── Masks/
│       │       └── (máscaras específicas)
│       │
│       └── Documentation/
│           ├── PlayerControllerSystem.md
│           └── QuickSetupGuide.md
│
├── Data/
│   ├── Player/
│   │   ├── PlayerData.asset
│   │   └── PlayerEvents.asset
│   └── Masks/
│       └── (máscaras .asset)
│
├── Prefabs/
│   ├── Player/
│   ├── Platforms/
│   └── Environment/
│
├── Sprites/
│   ├── Player/
│   ├── Platforms/
│   └── UI/
│
└── Scenes/
```

---

## Convenciones de Código

### Namespaces

```csharp
namespace Gameplay.Player { }       // Sistema del jugador
namespace Gameplay.Player.States { } // Estados del jugador
namespace Gameplay.Masks { }        // Sistema de máscaras
namespace Gameplay.Level { }        // Generación de nivel
namespace Gameplay.UI { }           // Interfaz de usuario
namespace Core { }                  // Sistemas core (audio, save, etc.)
```

### Nomenclatura

| Tipo | Convención | Ejemplo |
|------|------------|---------|
| Clases | PascalCase | `PlayerController` |
| Interfaces | I + PascalCase | `IPlayerState` |
| Métodos públicos | PascalCase | `OnJumpPressed()` |
| Métodos privados | PascalCase | `HandleInput()` |
| Variables privadas | camelCase | `moveDirection` |
| Variables serializadas | camelCase | `[SerializeField] private float jumpForce` |
| Constantes | UPPER_SNAKE_CASE | `const float MAX_SPEED = 10f` |
| Properties | PascalCase | `public int JumpCount { get; set; }` |
| Eventos | On + PascalCase | `OnJump`, `OnStateChanged` |
| ScriptableObjects | PascalCase + sufijo | `PlayerData`, `PlayerEvents` |
| Estados | Nombre + State | `MovingState`, `JumpingState` |

### Organización de Clases

```csharp
public class ExampleClass : MonoBehaviour
{
    // 1. Constantes
    private const float SOME_VALUE = 1f;

    // 2. Campos serializados (agrupados con Headers)
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float speed = 5f;

    // 3. Campos privados
    private Rigidbody2D rb;
    private bool isActive;

    // 4. Properties públicas
    public bool IsActive => isActive;

    // 5. Unity Messages (Awake, Start, Update, etc.)
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void FixedUpdate() { }

    // 6. Métodos públicos
    public void DoSomething() { }

    // 7. Métodos privados
    private void HandleInternal() { }

    // 8. Callbacks de eventos
    private void OnSomeEvent() { }

    // 9. Debug/Editor
    #if UNITY_EDITOR
    private void OnDrawGizmos() { }
    #endif
}
```

---

## Sistema de Estados del Jugador

### Estados Disponibles

| Estado | Enum | Descripción |
|--------|------|-------------|
| Moving | `PlayerState.Moving` | En suelo, movimiento automático |
| Jumping | `PlayerState.Jumping` | Ascendiendo tras salto |
| Falling | `PlayerState.Falling` | Cayendo (velocidad Y ≤ 0) |
| WallCling | `PlayerState.WallCling` | Agarrado a pared (0.5s) |
| WallJump | `PlayerState.WallJump` | Saltando desde pared |

### Implementar Nuevo Estado

```csharp
using UnityEngine;

namespace Gameplay.Player.States
{
    public class NewState : PlayerStateBase
    {
        public override void Enter(PlayerController player)
        {
            base.Enter(player);
            // Inicialización del estado
        }

        public override void Exit(PlayerController player)
        {
            base.Exit(player);
            // Limpieza del estado
        }

        public override void Update(PlayerController player)
        {
            base.Update(player); // Actualiza stateTimer
            // Lógica por frame
        }

        public override void FixedUpdate(PlayerController player)
        {
            // Lógica de física
            player.Motor.Move(player.MoveDirection);

            // Transiciones
            if (someCondition)
            {
                player.ChangeState(PlayerState.OtherState);
            }
        }

        public override void OnJumpPressed(PlayerController player)
        {
            // Respuesta a input de salto
        }

        public override void OnCollisionEnter(PlayerController player, Collision2D collision)
        {
            // Respuesta a colisiones
        }
    }
}
```

### Registrar Estado

En `PlayerStateMachine.cs`:

```csharp
states = new Dictionary<PlayerState, IPlayerState>
{
    { PlayerState.Moving, new MovingState() },
    { PlayerState.Jumping, new JumpingState() },
    // Añadir nuevo estado aquí
    { PlayerState.NewState, new NewState() }
};
```

---

## Sistema de Eventos

### Eventos Disponibles

```csharp
// En PlayerEvents.cs (ScriptableObject)
OnDirectionChanged  // int direction (-1 o 1)
OnJump              // int jumpCount (1 o 2)
OnLand              // float fallSpeed
OnWallHit           // Vector2 wallNormal
OnWallCling         // Vector2 position
OnWallJump          // int newDirection
OnStateChanged      // PlayerState newState
```

### Disparar Evento

```csharp
player.Events.RaiseJump(player.JumpCount);
player.Events.RaiseStateChanged(PlayerState.Jumping);
```

### Suscribirse a Evento

```csharp
// En Awake o OnEnable
playerEvents.OnJump.AddListener(HandleJump);

// En OnDisable
playerEvents.OnJump.RemoveListener(HandleJump);

private void HandleJump(int jumpCount)
{
    // Responder al evento
}
```

---

## Sistema de Máscaras (Power-ups)

### Crear Nueva Máscara

```csharp
using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    [CreateAssetMenu(fileName = "NewMask", menuName = "Game/Masks/New Mask")]
    public class NewMask : MaskBase
    {
        [Header("Mask Settings")]
        [SerializeField] private float someValue = 1f;

        public override void OnEquip(PlayerController player)
        {
            // Al equipar la máscara
        }

        public override void OnUnequip(PlayerController player)
        {
            // Al desequipar (restaurar valores originales)
        }

        public override void OnUpdate(PlayerController player)
        {
            // Cada frame mientras está equipada
        }

        public override void ModifyJump(ref float jumpForce)
        {
            jumpForce *= someValue;
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

### Equipar Máscara por Código

```csharp
MaskManager maskManager = player.GetComponent<MaskManager>();
maskManager.EquipMask(maskAsset);
maskManager.UnequipCurrentMask();
```

---

## Configuración de Unity

### Layers Requeridos

| Layer | Número | Uso |
|-------|--------|-----|
| Ground | 8 | Plataformas sólidas |
| OneWayPlatform | 9 | Plataformas atravesables |
| Wall | 10 | Paredes laterales |
| Player | 11 | El jugador |

### Physics2D Settings

- **Gravity**: (0, -9.81) por defecto
- El jugador usa `gravityScale = 3` para caída más rápida

### Rigidbody2D del Player

```
Body Type: Dynamic
Collision Detection: Continuous
Interpolate: Interpolate
Freeze Rotation Z: ✓
```

### Plataformas One-Way

Requieren:
1. `BoxCollider2D` con `Used By Effector = true`
2. `PlatformEffector2D` con `Use One Way = true`
3. Layer = `OneWayPlatform`

---

## Parámetros de Gameplay

### Valores por Defecto

| Parámetro | Valor | Descripción |
|-----------|-------|-------------|
| moveSpeed | 8 | Velocidad horizontal |
| jumpForce | 16.5 | Fuerza de salto (~4.5u altura) |
| doubleJumpForce | 16.5 | Fuerza de doble salto |
| maxJumps | 2 | Saltos permitidos (1 + 1 doble) |
| wallClingDuration | 0.5 | Segundos agarrado a pared |
| wallJumpForce | (8, 16.5) | Impulso de wall jump |
| gravityScale | 3 | Multiplicador de gravedad |
| fallMultiplier | 2.5 | Caída más rápida |

### Anti-Exploit: Vuelo Infinito

```
jumpCount se resetea SOLO en MovingState.Enter()
Wall Cling NO resetea jumpCount
Wall Jump NO resetea jumpCount
```

---

## Input

### Mobile
- **Tap**: Salto / Doble salto
- **Swipe Down**: Drop through (plataformas one-way)

### PC/Editor
- **Click izquierdo**: Salto
- **Espacio**: Salto
- **S / Flecha abajo**: Drop through

### Implementación Actual

```csharp
// En PlayerController.HandleInput()
if (Input.GetMouseButtonDown(0) || touch)
    OnJumpInput();

#if UNITY_EDITOR
if (Input.GetKeyDown(KeyCode.Space))
    OnJumpInput();
if (Input.GetKeyDown(KeyCode.S))
    TryDropThroughPlatform();
#endif
```

---

## Directrices de Desarrollo

### DO (Hacer)

- ✅ Usar ScriptableObjects para configuración
- ✅ Separar física (Motor) de lógica (Controller)
- ✅ Usar eventos para comunicación entre sistemas
- ✅ Mantener estados pequeños y enfocados
- ✅ Documentar parámetros con `[Tooltip]`
- ✅ Usar `#if UNITY_EDITOR` para debug
- ✅ Validar referencias en Awake
- ✅ Usar SerializeField en lugar de public

### DON'T (No hacer)

- ❌ Acceder directamente a otros sistemas (usar eventos)
- ❌ Poner lógica de física en Update (usar FixedUpdate)
- ❌ Hardcodear valores (usar PlayerData)
- ❌ Crear dependencias circulares
- ❌ Modificar PlayerData en runtime sin restaurar
- ❌ Usar FindObjectOfType en Update

---

## Testing en Editor

### Debug Visual

Al seleccionar el Player:
- **Círculo verde**: Ground check
- **Líneas azules**: Wall check (izq/der)

### Debug HUD

Durante Play Mode (esquina superior izquierda):
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

## Próximos Sistemas a Implementar

- [ ] Level Generation (procedural o por chunks)
- [ ] Camera Follow System
- [ ] Obstacle System
- [ ] Score/Progression System
- [ ] Audio Manager (conectar a eventos)
- [ ] VFX Manager (partículas)
- [ ] UI System (menús, HUD)
- [ ] Save System (progreso, máscaras)
- [ ] Analytics Integration

---

## Referencias Técnicas

- **Unity 2D Physics**: https://docs.unity3d.com/Manual/Physics2DReference.html
- **Platform Effector 2D**: https://docs.unity3d.com/Manual/class-PlatformEffector2D.html
- **ScriptableObjects**: https://docs.unity3d.com/Manual/class-ScriptableObject.html
- **UnityEvents**: https://docs.unity3d.com/Manual/UnityEvents.html
