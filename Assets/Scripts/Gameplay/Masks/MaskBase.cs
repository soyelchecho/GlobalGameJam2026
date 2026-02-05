using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    public abstract class MaskBase : ScriptableObject, IMask
    {
        [Header("Mask Info")]
        [SerializeField] protected string maskId;
        [SerializeField] protected Sprite maskSprite;
        [SerializeField] [TextArea] protected string description;

        [Header("Animation")]
        [SerializeField] protected string equipAnimationTrigger = "EquipMask";
        [SerializeField] protected string unequipAnimationTrigger = "UnequipMask";

        public string MaskId => maskId;
        public Sprite MaskSprite => maskSprite;
        public string Description => description;
        public string EquipAnimationTrigger => equipAnimationTrigger;
        public string UnequipAnimationTrigger => unequipAnimationTrigger;

        public virtual void OnEquip(PlayerController player)
        {
        }

        public virtual void OnUnequip(PlayerController player)
        {
        }

        public virtual void OnUpdate(PlayerController player)
        {
        }

        public virtual void ModifyJump(ref float jumpForce)
        {
        }

        public virtual void ModifySpeed(ref float speed)
        {
        }

        public virtual void ModifyWallCling(ref float duration)
        {
        }
    }
}
