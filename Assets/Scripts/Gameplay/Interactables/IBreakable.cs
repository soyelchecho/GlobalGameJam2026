namespace Gameplay.Interactables
{
    public interface IBreakable
    {
        bool IsBroken { get; }
        bool CanBreak(string maskId);
        void Break();
    }
}
