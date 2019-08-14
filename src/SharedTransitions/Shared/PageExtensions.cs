using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    public static class PageExtensions
    {
        /// <summary>
        /// Find the current navigation stack based on the MainPage type
        /// </summary>
        /// <param name="mainPage">Application current mainpage</param>
        internal static Page GetCurrentPageInNavigationStack(this Page mainPage)
        {
            switch (mainPage)
            {
                case SharedTransitionNavigationPage navPage:
                    return navPage.Navigation?.NavigationStack?.Last();
                case MasterDetailPage masterPage:
                    return masterPage.Detail.GetCurrentPageInNavigationStack();
                case TabbedPage tabbedPage:
                    return tabbedPage.CurrentPage.GetCurrentPageInNavigationStack();
                default:
                    return null;
            }
        }
    }
}
