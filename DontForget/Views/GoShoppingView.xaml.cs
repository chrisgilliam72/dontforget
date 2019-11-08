using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DontForget
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoShoppingView : TabbedPage
    {

        public ShoppingListView ShoppingListView
        {
            get
            {
                return viewShoppingList;
            }

        }

        public ItemMasterListView MasterListView
        {
            get
            {
                return viewMasterList;
            }

        }

        public ShoppingBillView ShoppingCartView
        {
            get
            {
                return viewShoppingCart;
            }

        }


        public GoShoppingView()
        {
            InitializeComponent();
        }



        public async void Refresh()
        {
            IsBusy = true;
            await viewMasterList.Refresh();
            await viewShoppingList.Refresh();
            await viewShoppingCart.Refresh();
            IsBusy = false;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
    }
}
