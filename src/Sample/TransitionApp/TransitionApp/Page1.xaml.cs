using System;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp
{
	public partial class Page1 : ContentPage
	{
	    public Page1()
	    {
	        InitializeComponent();
	        SharedTransitionNavigationPage.SetBackgroundAnimation(this, BackgroundAnimation.SlideFromTop);
	        SharedTransitionNavigationPage.SetSharedTransitionDuration(this, 500);
	    }

	    private async void ImageTapped(object sender, EventArgs eventArgs)
	    {
            await Navigation.PushAsync(new Page2());
	    }

	    private void Button_OnClicked(object sender, EventArgs e)
	    {
	        
	    }
    }
}