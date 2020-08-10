using System.Diagnostics;
using Plugin.SharedTransitions;
using TransitionShellApp.Views.Image;
using Xamarin.Forms;

namespace TransitionShellApp
{
	public partial class AppShell : SharedTransitionShell
	{
		public AppShell()
		{
			Routing.RegisterRoute("imageTo", typeof(ImageToPage));
			InitializeComponent();
		}

		public override void OnTransitionStarted(SharedTransitionEventArgs args)
		{
			Debug.WriteLine($"From override: Transition started - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
		}

		public override void OnTransitionEnded(SharedTransitionEventArgs args)
		{
			Debug.WriteLine($"From override: Transition ended - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
		}

		public override void OnTransitionCancelled(SharedTransitionEventArgs args)
		{
			Debug.WriteLine($"From override: Transition cancelled - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
		}

		private void AppShell_OnTransitionStarted(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition started - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}

		private void AppShell_OnTransitionEnded(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition ended - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}

		private void AppShell_OnTransitionCancelled(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition cancelled - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}
	}
}
