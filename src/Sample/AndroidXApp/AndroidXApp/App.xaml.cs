using AndroidXApp.Views;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace AndroidXApp
{
	public partial class App : Application
	{

		public App()
		{
			InitializeComponent();
			MainPage = new AppShell();

			//If you want to try standard navpage in android x
			//MainPage = new SharedTransitionNavigationPage(new MainPage());
		}

		protected override void OnStart()
		{
		}

		protected override void OnSleep()
		{
		}

		protected override void OnResume()
		{
		}
	}
}
