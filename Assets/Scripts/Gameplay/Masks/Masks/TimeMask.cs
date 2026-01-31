using UnityEngine;
using Gameplay.Player;
using Gameplay.Temporal;

namespace Gameplay.Masks
{
    [CreateAssetMenu(fileName = "TimeMask", menuName = "Game/Masks/Time Mask")]
    public class TimeMask : MaskBase
    {
        [Header("Time Mask Settings")]
        [SerializeField] private Color pastWorldTint = new Color(0.7f, 0.85f, 1f, 1f);

        public Color PastWorldTint => pastWorldTint;

        public override void OnEquip(PlayerController player)
        {
            TimeManager.Instance.SetTimeState(TimeState.Past);
        }

        public override void OnUnequip(PlayerController player)
        {
            TimeManager.Instance.SetTimeState(TimeState.Present);
        }
    }
}
