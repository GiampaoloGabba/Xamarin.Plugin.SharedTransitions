using Plugin.SharedTransitions.Platforms.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Page), typeof(SharedTransitionPageRenderer))]
namespace Plugin.SharedTransitions.Platforms.iOS.Renderers
{
	public class SharedTransitionPageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (Application.Current.MainPage is ISharedTransitionContainer shellPage)
			{
				shellPage.TransitionMap.RemoveFromPage((Page)Element);
			}
			if (Element.Parent is ISharedTransitionContainer navPage)
			{
				navPage.TransitionMap.RemoveFromPage((Page)Element);
			}

			base.Dispose(disposing);
		}
	}
}
