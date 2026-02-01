using UnityEngine;

namespace Gameplay.Player.States
{
    public class FallingState : PlayerStateBase
    {
        public override void FixedUpdate(PlayerController player)
        {
            bool pushingIntoWall = player.IsTouchingWall && player.WallDirection == player.MoveDirection;
            bool blockedHorizontally = player.Motor.IsBlockedHorizontally(player.MoveDirection);

            // If front blocked and can't wall cling, flip direction
            if (player.IsFrontBlocked && !player.CanWallCling)
            {
                FlipDirection(player);
                pushingIntoWall = false; // Recalculate since direction changed
                blockedHorizontally = false;
            }

            // Don't push horizontally if:
            // 1. Pushing into wall and can't cling (slide down)
            // 2. Blocked by any surface (platform side, etc.)
            if ((!pushingIntoWall || player.CanWallCling) && !blockedHorizontally)
            {
                player.Motor.Move(player.MoveDirection);
            }

            // Only land if actually falling (not jumping through a one-way platform)
            if (player.IsGrounded && player.Motor.Velocity.y <= 0.1f)
            {
                player.ChangeState(PlayerState.Moving);
                return;
            }

            if (player.CanWallCling && pushingIntoWall)
            {
                player.ChangeState(PlayerState.WallCling);
            }
        }

        private void FlipDirection(PlayerController player)
        {
            player.MoveDirection = -player.MoveDirection;
            player.Events.RaiseDirectionChanged(player.MoveDirection);
            player.Events.RaiseWallHit(new Vector2(-player.MoveDirection, 0));
        }

        public override void OnJumpPressed(PlayerController player)
        {
            // Wall jump from sliding - always allowed (doesn't consume jump count)
            if (player.IsTouchingWall && player.WallDirection == player.MoveDirection)
            {
                int newDirection = -player.MoveDirection;
                player.MoveDirection = newDirection;
                player.Events.RaiseDirectionChanged(newDirection);
                player.Events.RaiseWallJump(newDirection);
                player.ResetWallCling();
                player.ChangeState(PlayerState.WallJump);
                return;
            }

            // Regular double jump - requires available jumps
            if (CanJump(player))
            {
                PerformJump(player, player.Data.doubleJumpForce);
                player.ChangeState(PlayerState.Jumping);
            }
        }

        public override void OnCollisionEnter(PlayerController player, Collision2D collision)
        {
            // Check both ground layer AND one-way platform layer
            if (((1 << collision.gameObject.layer) & player.Data.AllGroundLayers) != 0)
            {
                // Only land if hitting from ABOVE (normal pointing up)
                ContactPoint2D contact = collision.GetContact(0);
                if (contact.normal.y > 0.5f)
                {
                    player.ChangeState(PlayerState.Moving);
                    return;
                }
                // If hitting from the side, don't change state (let player slide down)
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
