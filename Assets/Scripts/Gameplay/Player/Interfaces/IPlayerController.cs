using UnityEngine;

namespace Gameplay.Player
{
    public interface IPlayerController
    {
        PlayerData Data { get; }
        PlayerMotor Motor { get; }
        PlayerStateMachine StateMachine { get; }
        PlayerEvents Events { get; }

        int MoveDirection { get; set; }
        int JumpCount { get; set; }
        bool IsGrounded { get; }
        bool IsTouchingWall { get; }
        int WallDirection { get; }

        void ChangeState(PlayerState newState);
    }
}
