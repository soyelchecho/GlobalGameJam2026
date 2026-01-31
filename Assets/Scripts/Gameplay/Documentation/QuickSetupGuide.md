# Player Controller - Guía Rápida de Setup

## 1. Crear Layers (Edit → Project Settings → Tags and Layers)

```
User Layer 8:  Ground
User Layer 9:  OneWayPlatform
User Layer 10: Wall
```

## 2. Crear ScriptableObjects

### PlayerData
- Click derecho en Project → **Create → Game → Player Data**
- Configurar:
  - Ground Layer: `Ground`
  - One Way Platform Layer: `OneWayPlatform`
  - Wall Layer: `Wall`

### PlayerEvents
- Click derecho en Project → **Create → Game → Player Events**

## 3. Crear Player GameObject

```
Player
├── Rigidbody2D
│   ├── Collision Detection: Continuous
│   ├── Interpolate: Interpolate
│   └── Freeze Rotation Z: ✓
├── BoxCollider2D (1x2)
├── SpriteRenderer
├── PlayerController.cs
│   ├── Data: [PlayerData]
│   └── Events: [PlayerEvents]
├── PlayerMotor.cs
│   ├── Ground Check: [GroundCheck]
│   └── Wall Check: [WallCheck]
└── Hijos:
    ├── GroundCheck (pos: 0, -1, 0)
    └── WallCheck (pos: 0, 0, 0)
```

## 4. Crear Plataforma Sólida

```
Platform_Solid
├── SpriteRenderer
├── BoxCollider2D
└── Layer: Ground ← IMPORTANTE
```

## 5. Crear Plataforma One-Way

```
Platform_OneWay
├── SpriteRenderer
├── BoxCollider2D
│   └── Used By Effector: ✓
├── PlatformEffector2D
│   └── Use One Way: ✓
└── Layer: OneWayPlatform ← IMPORTANTE
```

## 6. Crear Pared

```
Wall
├── SpriteRenderer
├── BoxCollider2D
└── Layer: Wall ← IMPORTANTE
```

## 7. Controles

| Input | Acción |
|-------|--------|
| Click / Tap / Espacio | Salto |
| S / Flecha Abajo | Drop through plataforma |

## 8. Verificar en Play Mode

Debe aparecer en esquina superior izquierda:
```
State: Moving
Direction: 1
Jump Count: 0/2
Grounded: True
```

---

## Valores por Defecto

| Parámetro | Valor |
|-----------|-------|
| Move Speed | 8 |
| Jump Force | 16.5 |
| Max Jumps | 2 |
| Wall Cling Duration | 0.5s |
| Gravity Scale | 3 |
