using System;
using System.Diagnostics;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace AndroidXApp
{
	public partial class AppShell : SharedTransitionShell
	{
		public AppShell()
		{
			InitializeComponent();
		}

		public override void OnTransitionStarted(Page pageFrom, Page pageTo, NavOperation navOperation)
		{
			Debug.WriteLine($"From override: Transition started - {pageFrom}|{pageTo}|{navOperation}");
		}

		public override void OnTransitionEnded(Page pageFrom, Page pageTo, NavOperation navOperation)
		{
			Debug.WriteLine($"From override: Transition ended - {pageFrom}|{pageTo}|{navOperation}");
		}

		public override void OnTransitionCancelled(Page pageFrom, Page pageTo, NavOperation navOperation)
		{
			Debug.WriteLine($"From override: Transition cancelled - {pageFrom}|{pageTo}|{navOperation}");
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
