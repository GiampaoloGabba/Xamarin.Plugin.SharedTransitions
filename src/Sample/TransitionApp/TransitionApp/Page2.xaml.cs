using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp
{
	public partial class Page2 : ContentPage
	{
	    public Page2()
	    {
	        InitializeComponent();

	        SharedTransitionNavigationPage.SetBackgroundAnimation(this, BackgroundAnimation.None);
	        SharedTransitionNavigationPage.SetSharedTransitionDuration(this, 500);
	    }

	    private void ImageTapped(object sender, EventArgs e)
	    {
	        Navigation.PushAsync(new Page3());
	    }

	    private void Button_OnClicked(object sender, EventArgs e)
	    {
	        
	    }
    }
}