using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DontForget
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new GoShoppingView();
            //MainPage =  new NavigationPage(new GoShoppingView());
        }

        protected override void OnStart()
        {
            //var mainPage = MainPage as NavigationPage;
            //var goShoppingView =mainPage.CurrentPage as GoShoppingView;

            var goShoppingView = MainPage as GoShoppingView;
            goShoppingView.Refresh();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
