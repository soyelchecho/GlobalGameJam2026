using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Simple helper for death animation events.
    /// Attach to player GameObject.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        /// <summary>
        /// Call from Animation Event at end of death animation
        /// </summary>
        public void HidePlayer()
        {
            gameObject.SetActive(false);
        }
    }
}
