using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Masks
{
    public interface IMask
    {
        string MaskId { get; }
        Sprite MaskSprite { get; }

        void OnEquip(PlayerController player);
        void OnUnequip(PlayerController player);
        void OnUpdate(PlayerController player);

        void ModifyJump(ref float jumpForce);
        void ModifySpeed(ref float speed);
        void ModifyWallCling(ref float duration);
    }
}
