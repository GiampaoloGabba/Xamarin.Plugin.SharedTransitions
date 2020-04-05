using Android.Content;
using Plugin.SharedTransitions.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Page), typeof(SharedTransitionPageRenderer))]
namespace Plugin.SharedTransitions.Platforms.Android
{
	public class SharedTransitionPageRenderer : PageRenderer
	{
		public SharedTransitionPageRenderer(Context context) : base(context)
		{
			
		}

		protected override void Dispose(bool disposing)
		{
			if (Application.Current.MainPage is ISharedTransitionContainer shellPage)
			{
				shellPage.TransitionMap.RemoveFromPage(Element);
			}
			if (Element.Parent is ISharedTransitionContainer navPage)
			{
				navPage.TransitionMap.RemoveFromPage(Element);
			}

			base.Dispose(disposing);
		}
	}
}
