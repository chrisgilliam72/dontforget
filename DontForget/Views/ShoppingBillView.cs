using System;

using Xamarin.Forms;

namespace ShoppingList.Views
{
    public class ShoppingBillView : ContentPage
    {
        public ShoppingBillView()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

