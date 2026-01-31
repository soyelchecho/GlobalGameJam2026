using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Player
{
    public enum PlayerState
    {
        Moving,
        Jumping,
        Falling,
        WallCling,
        WallJump
    }

    [System.Serializable]
    public class DirectionChangedEvent : UnityEvent<int> { }

    [System.Serializable]
    public class JumpEvent : UnityEvent<int> { }

    [System.Serializable]
    public class WallHitEvent : UnityEvent<Vector2> { }

    [System.Serializable]
    public class WallClingEvent : UnityEvent<Vector2> { }

    [System.Serializable]
    public class WallJumpEvent : UnityEvent<int> { }

    [System.Serializable]
    public class LandEvent : UnityEvent<float> { }

    [System.Serializable]
    public class StateChangedEvent : UnityEvent<PlayerState> { }

    [CreateAssetMenu(fileName = "PlayerEvents", menuName = "Game/Player Events")]
    public class PlayerEvents : ScriptableObject
    {
        [Header("Movement Events")]
        public DirectionChangedEvent OnDirectionChanged = new DirectionChangedEvent();

        [Header("Jump Events")]
        public JumpEvent OnJump = new JumpEvent();
        public LandEvent OnLand = new LandEvent();

        [Header("Wall Events")]
        public WallHitEvent OnWallHit = new WallHitEvent();
        public WallClingEvent OnWallCling = new WallClingEvent();
        public WallJumpEvent OnWallJump = new WallJumpEvent();

        [Header("State Events")]
        public StateChangedEvent OnStateChanged = new StateChangedEvent();

        public void RaiseDirectionChanged(int direction)
        {
            OnDirectionChanged?.Invoke(direction);
        }

        public void RaiseJump(int jumpCount)
        {
            OnJump?.Invoke(jumpCount);
        }

        public void RaiseLand(float fallSpeed)
        {
            OnLand?.Invoke(fallSpeed);
        }

        public void RaiseWallHit(Vector2 wallNormal)
        {
            OnWallHit?.Invoke(wallNormal);
        }

        public void RaiseWallCling(Vector2 position)
        {
            OnWallCling?.Invoke(position);
        }

        public void RaiseWallJump(int newDirection)
        {
            OnWallJump?.Invoke(newDirection);
        }

        public void RaiseStateChanged(PlayerState newState)
        {
            OnStateChanged?.Invoke(newState);
        }

        public void ClearAllListeners()
        {
            OnDirectionChanged.RemoveAllListeners();
            OnJump.RemoveAllListeners();
            OnLand.RemoveAllListeners();
            OnWallHit.RemoveAllListeners();
            OnWallCling.RemoveAllListeners();
            OnWallJump.RemoveAllListeners();
            OnStateChanged.RemoveAllListeners();
        }
    }
}
