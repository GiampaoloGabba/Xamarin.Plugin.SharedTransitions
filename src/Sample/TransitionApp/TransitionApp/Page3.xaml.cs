using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TransitionApp
{
	public partial class Page3 : ContentPage
	{
		public Page3 ()
		{
			InitializeComponent ();
		}

	    private async void ImageTapped(object sender, EventArgs e)
	    {
	        await Navigation.PopToRootAsync();
	    }
    }
}