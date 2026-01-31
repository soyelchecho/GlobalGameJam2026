using System.Collections.Generic;
using UnityEngine;
using Gameplay.Player.States;

namespace Gameplay.Player
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<PlayerState, IPlayerState> states;
        private IPlayerState currentState;
        private PlayerState currentStateType;
        private PlayerController player;

        public PlayerState CurrentStateType => currentStateType;
        public IPlayerState CurrentState => currentState;
        public float CurrentStateTimer => (currentState as PlayerStateBase)?.StateTimer ?? 0f;

        public PlayerStateMachine(PlayerController playerController)
        {
            player = playerController;

            states = new Dictionary<PlayerState, IPlayerState>
            {
                { PlayerState.Moving, new MovingState() },
                { PlayerState.Jumping, new JumpingState() },
                { PlayerState.Falling, new FallingState() },
                { PlayerState.WallCling, new WallClingState() },
                { PlayerState.WallJump, new WallJumpState() }
            };
        }

        public void Initialize(PlayerState initialState)
        {
            currentStateType = initialState;
            currentState = states[initialState];
            currentState.Enter(player);
            player.Events.RaiseStateChanged(initialState);
        }

        public void ChangeState(PlayerState newState)
        {
            if (currentStateType == newState) return;

            currentState?.Exit(player);

            currentStateType = newState;
            currentState = states[newState];
            currentState.Enter(player);

            player.Events.RaiseStateChanged(newState);
        }

        public void Update()
        {
            currentState?.Update(player);
        }

        public void FixedUpdate()
        {
            currentState?.FixedUpdate(player);
        }

        public void OnJumpPressed()
        {
            currentState?.OnJumpPressed(player);
        }

        public void OnCollisionEnter(Collision2D collision)
        {
            currentState?.OnCollisionEnter(player, collision);
        }
    }
}
