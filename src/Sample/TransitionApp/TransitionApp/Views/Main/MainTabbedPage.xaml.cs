using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace TransitionApp.Views.Main
{
	public partial class MainTabbedPage : TabbedPage
	{
		public MainTabbedPage()
		{
			InitializeComponent();
		}

		protected override void OnCurrentPageChanged()
		{
			if (CurrentPage is BlankPage)
				Application.Current.MainPage = new HomePage();
		}

		private void SharedTransitionNavigationPage_OnTransitionStarted(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition started");
		}

		private void SharedTransitionNavigationPage_OnTransitionEnded(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition ended");
		}

		private void SharedTransitionNavigationPage_OnTransitionCancelled(object sender, EventArgs e)
		{
			Debug.WriteLine("From event: Transition cancelled");
		}
	}
}
