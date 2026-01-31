using UnityEngine;

namespace Gameplay.Player.States
{
    public class WallClingState : PlayerStateBase
    {
        private const float WallCheckGracePeriod = 0.05f;
        private int wallDirection;

        public override void Enter(PlayerController player)
        {
            base.Enter(player);
            wallDirection = player.WallDirection;
            player.Motor.StartWallCling();
            player.Events.RaiseWallCling(player.transform.position);
            player.MarkWallClingUsed();
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
                ExitToFalling(player);
            }
        }

        public override void FixedUpdate(PlayerController player)
        {
            // Grace period to let physics settle before checking wall contact
            if (stateTimer > WallCheckGracePeriod)
            {
                // Check if still touching wall in the original direction
                if (!player.Motor.CheckWall(wallDirection))
                {
                    ExitToFalling(player);
                    return;
                }
            }

            if (player.IsGrounded)
            {
                player.ChangeState(PlayerState.Moving);
            }
        }

        private void ExitToFalling(PlayerController player)
        {
            player.ChangeState(PlayerState.Falling);
        }

        public override void OnJumpPressed(PlayerController player)
        {
            int newDirection = -wallDirection;
            player.MoveDirection = newDirection;
            player.Events.RaiseDirectionChanged(newDirection);
            player.Events.RaiseWallJump(newDirection);

            player.ResetWallCling();
            player.ChangeState(PlayerState.WallJump);
        }
    }
}
