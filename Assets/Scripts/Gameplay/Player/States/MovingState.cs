using UnityEngine;

namespace Gameplay.Player.States
{
    public class MovingState : PlayerStateBase
    {
        public override void Enter(PlayerController player)
        {
            base.Enter(player);
            player.JumpCount = 0;
            player.ResetWallCling();

            float fallSpeed = Mathf.Abs(player.Motor.Velocity.y);
            if (fallSpeed > 1f)
            {
                player.Events.RaiseLand(fallSpeed);
            }

            // If already touching wall in move direction (e.g. landed while sliding), flip
            if (player.IsTouchingWall && player.WallDirection == player.MoveDirection)
            {
                player.MoveDirection = -player.MoveDirection;
                player.Events.RaiseDirectionChanged(player.MoveDirection);
            }
        }

        public override void FixedUpdate(PlayerController player)
        {
            player.Motor.Move(player.MoveDirection);

            if (!player.IsGrounded)
            {
                player.ChangeState(PlayerState.Falling);
            }
        }

        public override void OnJumpPressed(PlayerController player)
        {
            if (CanJump(player))
            {
                PerformJump(player, player.Data.jumpForce);
                player.ChangeState(PlayerState.Jumping);
            }
        }

        public override void OnCollisionEnter(PlayerController player, Collision2D collision)
        {
            CheckWallAndFlip(player, collision);
        }

        private void CheckWallAndFlip(PlayerController player, Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & player.Data.AllWallLayers) != 0)
            {
                ContactPoint2D contact = collision.GetContact(0);
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    int newDirection = contact.normal.x > 0 ? 1 : -1;
                    if (newDirection != player.MoveDirection)
                    {
                        player.MoveDirection = newDirection;
                        player.Events.RaiseDirectionChanged(player.MoveDirection);
                        player.Events.RaiseWallHit(contact.normal);
                    }
                }
            }
        }
    }
}
