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
	}
}
