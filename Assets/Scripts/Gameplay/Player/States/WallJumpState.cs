using UnityEngine;

namespace Gameplay.Player.States
{
    public class WallJumpState : PlayerStateBase
    {
        private const float WallJumpControlLockTime = 0.15f;

        public override void Enter(PlayerController player)
        {
            base.Enter(player);
            player.Motor.WallJump(player.MoveDirection);
        }

        public override void FixedUpdate(PlayerController player)
        {
            if (stateTimer < WallJumpControlLockTime)
            {
                return;
            }

            bool pushingIntoWall = player.IsTouchingWall && player.WallDirection == player.MoveDirection;
            bool blockedHorizontally = player.Motor.IsBlockedHorizontally(player.MoveDirection);

            // Don't push horizontally if blocked by any surface
            if ((!pushingIntoWall || player.CanWallCling) && !blockedHorizontally)
            {
                player.Motor.Move(player.MoveDirection);
            }

            if (player.Motor.Velocity.y <= 0)
            {
                player.ChangeState(PlayerState.Falling);
                return;
            }

            if (player.CanWallCling && pushingIntoWall)
            {
                player.ChangeState(PlayerState.WallCling);
            }
        }

        public override void OnJumpPressed(PlayerController player)
        {
            if (CanJump(player))
            {
                PerformJump(player, player.Data.doubleJumpForce);
                player.ChangeState(PlayerState.Jumping);
            }
        }

        public override void OnCollisionEnter(PlayerController player, Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & player.Data.groundLayer) != 0)
            {
                // Only land if hitting from ABOVE (normal pointing up)
                ContactPoint2D contact = collision.GetContact(0);
                if (contact.normal.y > 0.5f)
                {
                    player.ChangeState(PlayerState.Moving);
                    return;
                }
            }

            if (((1 << collision.gameObject.layer) & player.Data.AllWallLayers) != 0)
            {
                ContactPoint2D contact = collision.GetContact(0);
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    int wallDir = contact.normal.x > 0 ? -1 : 1;
                    if (wallDir == player.MoveDirection && player.CanWallCling)
                    {
                        player.Events.RaiseWallHit(contact.normal);
                        player.ChangeState(PlayerState.WallCling);
                    }
                }
            }
        }
    }
}
