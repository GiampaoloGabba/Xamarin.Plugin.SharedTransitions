using System;
using AndroidXApp.Views.Collectionview;
using AndroidXApp.Views.Image;
using AndroidXApp.Views.Layout;
using AndroidXApp.Views.Listview;
using Xamarin.Forms;

namespace AndroidXApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonImage_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ImageFromPage());
        }

        private void ButtonLayouts_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LayoutFromPage());
        }

        private void ButtonListView_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ListViewFromPage());
        }

        private void ButtonCollectionView_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CollectionviewFromPage());
        }
    }
}
