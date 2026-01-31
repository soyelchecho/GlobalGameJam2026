using UnityEngine;

namespace Gameplay.Player.States
{
    public class WallClingState : PlayerStateBase
    {
        private int wallDirection;

        public override void Enter(PlayerController player)
        {
            base.Enter(player);
            wallDirection = player.WallDirection;
            player.Motor.StartWallCling();
            player.Events.RaiseWallCling(player.transform.position);
        }

        public override void Exit(PlayerController player)
        {
            base.Exit(player);
            player.Motor.EndWallCling();
        }

        public override void Update(PlayerController player)
        {
            base.Update(player);

            float clingDuration = player.GetModifiedWallClingDuration(player.Data.wallClingDuration);
            if (stateTimer >= clingDuration)
            {
                player.ChangeState(PlayerState.Falling);
            }
        }

        public override void FixedUpdate(PlayerController player)
        {
            if (!player.IsTouchingWall)
            {
                player.ChangeState(PlayerState.Falling);
                return;
            }

            if (player.IsGrounded)
            {
                player.ChangeState(PlayerState.Moving);
            }
        }

        public override void OnJumpPressed(PlayerController player)
        {
            int newDirection = -wallDirection;
            player.MoveDirection = newDirection;
            player.Events.RaiseDirectionChanged(newDirection);
            player.Events.RaiseWallJump(newDirection);

            player.ChangeState(PlayerState.WallJump);
        }
    }
}
