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

		public override void OnTransitionStarted()
		{
			Debug.WriteLine("From override: Transition started");
		}

		public override void OnTransitionEnded()
		{
			Debug.WriteLine("From override: Transition ended");
		}

		public override void OnTransitionCancelled()
		{
			Debug.WriteLine("From override: Transition cancelled");
		}

		private void AppShell_OnTransitionStarted(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition started");
		}

		private void AppShell_OnTransitionEnded(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition ended");
		}

		private void AppShell_OnTransitionCancelled(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition ended");
		}
	}
}
