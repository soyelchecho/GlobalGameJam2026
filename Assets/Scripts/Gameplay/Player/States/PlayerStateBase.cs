using UnityEngine;

namespace Gameplay.Player.States
{
    public abstract class PlayerStateBase : IPlayerState
    {
        protected float stateTimer;

        public float StateTimer => stateTimer;

        public virtual void Enter(PlayerController player)
        {
            stateTimer = 0f;
        }

        public virtual void Exit(PlayerController player)
        {
        }

        public virtual void Update(PlayerController player)
        {
            stateTimer += Time.deltaTime;
        }

        public virtual void FixedUpdate(PlayerController player)
        {
        }

        public virtual void OnJumpPressed(PlayerController player)
        {
        }

        public virtual void OnCollisionEnter(PlayerController player, Collision2D collision)
        {
        }

        protected bool CanJump(PlayerController player)
        {
            return player.JumpCount < player.Data.maxJumps;
        }

        protected void PerformJump(PlayerController player, float force)
        {
            player.JumpCount++;
            player.Motor.Jump(force);
            player.Events.RaiseJump(player.JumpCount);
        }

        protected void CheckWallCollision(PlayerController player)
        {
            if (player.IsTouchingWall)
            {
                int wallDir = player.WallDirection;
                if (wallDir != 0 && wallDir == player.MoveDirection)
                {
                    player.ChangeState(PlayerState.WallCling);
                }
            }
        }

        protected void CheckGrounded(PlayerController player)
        {
            if (player.IsGrounded && player.Motor.Velocity.y <= 0)
            {
                player.ChangeState(PlayerState.Moving);
            }
        }
    }
}
