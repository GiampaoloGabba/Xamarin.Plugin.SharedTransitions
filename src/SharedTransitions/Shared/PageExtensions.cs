using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    public static class PageExtensions
    {
        public static Page GetCurrentPageInNavigationStack(this Page mainPage)
        {
            switch (mainPage)
            {
                case NavigationPage navPage:
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
