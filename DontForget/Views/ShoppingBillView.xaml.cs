using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DontForget
{
    public partial class ShoppingBillView : ItemListView
    {
        public ShoppingBillView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async Task UpdateItemBought(ShoppingListItem item, bool isBought)
        {
            var groceryItem = await _Connection.FindAsync<ShoppingCartitem>(x => x.GroceryItemID == item.ItemID);
            if (groceryItem != null)
            {
                groceryItem.IsBought = isBought;
                await _Connection.UpdateAsync(groceryItem);
            }
        }


        public async Task Refresh()
        {
            await base.Refresh(true);
        }


        public async Task AddItem(ShoppingListItem item)
        {
            ItemList.Add(item);
            Update();

           await UpdateItemBought(item, true);

        }

        public async void ReturnItemClicked(object sender, System.EventArgs e)
        {
            var goShoppingView = Parent as GoShoppingView;
            var menuitem = sender as MenuItem;
            var shoppingItem = menuitem.CommandParameter as ShoppingListItem;
            if (shoppingItem!=null)
            {
                ItemList.Remove(shoppingItem);
                Update();

                await UpdateItemBought(shoppingItem, false);
                await goShoppingView.ShoppingListView.Refresh();

            }
        }

        public async void TrashClicked(object sender, System.EventArgs e)
        {
            var result = await DisplayAlert("Confirm", "Are you sure you want to clear the list?", "Yes", "No");
            if (result)
            {

                IsBusy = true;
                await _Connection.ExecuteAsync("delete from ShoppingCartitem where IsBought=true");
                IsBusy = false;

                ItemList.Clear();
                Update();
            }
        }
    }
}
