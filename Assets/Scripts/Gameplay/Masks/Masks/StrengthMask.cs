using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    [CreateAssetMenu(fileName = "StrengthMask", menuName = "Game/Masks/Strength Mask")]
    public class StrengthMask : MaskBase
    {
        [Header("Strength Mask Settings")]
        [Tooltip("Jump force multiplier when wearing this mask")]
        [SerializeField] private float jumpMultiplier = 1f;

        [Tooltip("Movement speed multiplier when wearing this mask")]
        [SerializeField] private float speedMultiplier = 1f;

        public override void OnEquip(PlayerController player)
        {
            // Strength mask equipped - player can now break objects with requiredMaskId = "strength"
        }

        public override void OnUnequip(PlayerController player)
        {
            // Strength mask removed
        }

        public override void ModifyJump(ref float jumpForce)
        {
            jumpForce *= jumpMultiplier;
        }

        public override void ModifySpeed(ref float speed)
        {
            speed *= speedMultiplier;
        }
    }
}
