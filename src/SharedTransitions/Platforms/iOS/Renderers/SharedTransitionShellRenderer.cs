using Plugin.SharedTransitions;
using Plugin.SharedTransitions.Platforms.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(SharedTransitionShell), typeof(SharedTransitionShellRenderer))]
namespace Plugin.SharedTransitions.Platforms.iOS
{
	public class SharedTransitionShellRenderer : ShellRenderer
	{
		protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
		{
			return new SharedTransitionShellSectionRenderer(this);
		}
	}
}
