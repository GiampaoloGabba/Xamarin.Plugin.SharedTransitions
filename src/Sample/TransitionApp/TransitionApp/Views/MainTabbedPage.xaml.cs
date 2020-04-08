using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TransitionApp.Views
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
