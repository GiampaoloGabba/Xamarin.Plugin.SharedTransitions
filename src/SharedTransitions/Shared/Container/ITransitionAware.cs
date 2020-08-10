using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    public interface ITransitionAware
    {
        void OnTransitionStarted(Page pageFrom, Page pageTo, NavOperation navOperation);
        void OnTransitionEnded(Page pageFrom, Page pageTo, NavOperation navOperation);
        void OnTransitionCancelled(Page pageFrom, Page pageTo, NavOperation navOperation);
    }
}
