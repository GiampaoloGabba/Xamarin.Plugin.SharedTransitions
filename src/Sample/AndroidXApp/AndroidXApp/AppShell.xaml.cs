using System;
using System.Collections.Generic;
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

		private void AppShell_OnTransitionStarted(object sender, EventArgs e)
		{
			Debug.WriteLine("Transition started");
		}

		private void AppShell_OnTransitionEnded(object sender, EventArgs e)
		{
			Debug.WriteLine("Transition ended");
		}

		private void AppShell_OnTransitionCancelled(object sender, EventArgs e)
		{
			Debug.WriteLine("Transition ended");
		}
	}
}
