using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    public static class PageExtensions
    {
        /// <summary>
        /// Find the current page based on the MainPage type
        /// </summary>
        /// <param name="mainPage">Application current mainpage</param>
        internal static Page GetCurrentPage(this Page mainPage)
        {
            switch (mainPage)
            {
				case Shell appShell:
					return appShell.GetCurrentShellPage();
                case SharedTransitionNavigationPage navPage:
                    return navPage.Navigation?.NavigationStack?.Last();
                case MasterDetailPage masterPage:
                    return masterPage.Detail.GetCurrentPage();
                case TabbedPage tabbedPage:
                    return tabbedPage.CurrentPage.GetCurrentPage();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Find the current page from the main shell
        /// </summary>
        internal static Page GetCurrentShellPage(this Shell shell)
        {
	        var currentSection = shell?.CurrentItem?.CurrentItem?.CurrentItem;

	        if (currentSection != null && currentSection is IShellContentController shellContentController)
	        {
		        return shellContentController.Page;
	        }
	        return null;

        }

    }
}
