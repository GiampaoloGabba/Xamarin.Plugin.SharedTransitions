using System;
using System.Diagnostics;
using Plugin.SharedTransitions;
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

		private void SharedTransitionNavigationPage_OnTransitionStarted(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition started - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}

		private void SharedTransitionNavigationPage_OnTransitionEnded(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition ended - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}

		private void SharedTransitionNavigationPage_OnTransitionCancelled(object sender, SharedTransitionEventArgs e)
		{
			Debug.WriteLine($"From event: Transition cancelled - {e.PageFrom}|{e.PageTo}|{e.NavOperation}");
		}
	}
}
