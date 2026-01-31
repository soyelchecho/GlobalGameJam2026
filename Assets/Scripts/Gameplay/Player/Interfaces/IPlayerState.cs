namespace Gameplay.Player
{
    public interface IPlayerState
    {
        void Enter(PlayerController player);
        void Exit(PlayerController player);
        void Update(PlayerController player);
        void FixedUpdate(PlayerController player);
        void OnJumpPressed(PlayerController player);
        void OnCollisionEnter(PlayerController player, UnityEngine.Collision2D collision);
    }
}
