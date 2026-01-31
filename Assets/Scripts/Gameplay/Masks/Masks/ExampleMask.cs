using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    [CreateAssetMenu(fileName = "TripleJumpMask", menuName = "Game/Masks/Triple Jump Mask")]
    public class TripleJumpMask : MaskBase
    {
        [Header("Triple Jump Settings")]
        [SerializeField] private int extraJumps = 1;

        private int originalMaxJumps;

        public override void OnEquip(PlayerController player)
        {
            originalMaxJumps = player.Data.maxJumps;
            player.Data.maxJumps = originalMaxJumps + extraJumps;
        }

        public override void OnUnequip(PlayerController player)
        {
            player.Data.maxJumps = originalMaxJumps;
        }
    }

    [CreateAssetMenu(fileName = "SpeedMask", menuName = "Game/Masks/Speed Mask")]
    public class SpeedMask : MaskBase
    {
        [Header("Speed Settings")]
        [SerializeField] private float speedMultiplier = 1.5f;

        public override void ModifySpeed(ref float speed)
        {
            speed *= speedMultiplier;
        }
    }

    [CreateAssetMenu(fileName = "WallClimberMask", menuName = "Game/Masks/Wall Climber Mask")]
    public class WallClimberMask : MaskBase
    {
        [Header("Wall Climber Settings")]
        [SerializeField] private float extraClingDuration = 1f;

        public override void ModifyWallCling(ref float duration)
        {
            duration += extraClingDuration;
        }
    }

    [CreateAssetMenu(fileName = "HighJumpMask", menuName = "Game/Masks/High Jump Mask")]
    public class HighJumpMask : MaskBase
    {
        [Header("High Jump Settings")]
        [SerializeField] private float jumpMultiplier = 1.3f;

        public override void ModifyJump(ref float jumpForce)
        {
            jumpForce *= jumpMultiplier;
        }
    }
}
