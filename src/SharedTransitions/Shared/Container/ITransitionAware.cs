namespace Plugin.SharedTransitions
{
    public interface ITransitionAware
    {
        void OnTransitionStarted(SharedTransitionEventArgs args);
        void OnTransitionEnded(SharedTransitionEventArgs args);
        void OnTransitionCancelled(SharedTransitionEventArgs args);
    }
}
