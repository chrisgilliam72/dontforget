using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DontForget.Helpers;
using SQLite;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace DontForget
{
    enum SortMethod {PriceAscending, PriceDescending,Alphabetical,ImportantFirst }
    public partial class ShoppingListView : ItemListView
    {
        private SortMethod CurrentingSorting { get; set; }


        public ShoppingListView()
        {
            InitializeComponent();
            BindingContext = this;
            CurrentingSorting = SortMethod.Alphabetical;


        }

        private void ShoppingListView_itemAddededHander(object sender, EventArgs e)
        {
            var groceryItem = sender as GroceryItem;
            if (groceryItem != null)
                AddItem(groceryItem);
        }


       
        private async Task UpdateItem(ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem != null)
            {
                var dbCartItem = await _Connection.Table<ShoppingCartitem>().FirstOrDefaultAsync(x => x.GroceryItemID == shoppingListItem.ItemID);
                if (dbCartItem!=null)
                {
                    dbCartItem.Quantity =shoppingListItem.Quantity;
                    var intRetValue = await _Connection.UpdateAsync(dbCartItem);
                }


                Update();
            }           
        }

        private async Task DeleteItem(ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem != null)
            {

                var shoppingCartItem = await _Connection.FindAsync<ShoppingCartitem>(x => x.GroceryItemID == shoppingListItem.ItemID);
                if (shoppingCartItem != null)
                {
                    IsBusy = true;
                    await _Connection.DeleteAsync(shoppingCartItem);
                    IsBusy = false;
                }

                Removeitem(shoppingListItem);
                SortList();
            }
        }

        private void Removeitem(ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem != null)
            {
                var selecteditem = ItemList.FirstOrDefault(x => x.ItemID == shoppingListItem.ItemID);
                ItemList.Remove(selecteditem);
                Update();
                SortList();
            }
        }

        protected  override void OnAppearing()
        {
            base.OnAppearing();
            //parentWindow.Title = "Shopping List";

        }

        public async Task Refresh()
        {
            await base.Refresh(false);
            SortList();
        }


        public async void HandleRefreshing(object sender, EventArgs e)
        {
            var lstView = sender as ListView;
            await Refresh();
            SortList();
            lstView.IsRefreshing = false;
        }

        public void AddItem(GroceryItem groceryItem)
        {
            var shoppingListItem = new ShoppingListItem(groceryItem);

            ItemList.Add(shoppingListItem);
            SortList();
            Update();
        }

        public async Task SaveItem(GroceryItem groceryItem)
        {
            var shoppingListItem = new ShoppingListItem(groceryItem);
            var shoppingCartItem = (ShoppingCartitem)shoppingListItem;

            if (ItemList.FirstOrDefault(x => x.ItemID == shoppingListItem.ItemID) == null)
            {
                IsBusy = true;
                await _Connection.InsertAsync(shoppingCartItem);
                IsBusy = false;


            }
        }

        public async void SelectionListItemsAdd(object sender, System.EventArgs e)
        {
            var itemsSelView = sender as ItemsSelectionView;
            var selectedItems = itemsSelView.SelectedItems;
            if (selectedItems!=null && selectedItems.Count>0)
            {
                var nonDuplicateItems=selectedItems.Except(ItemList).ToList();
                var itemsToSave = nonDuplicateItems.Select(x => (ShoppingCartitem)x).Where(x=>!x.IsBought).ToList();
                ItemList.AddRange(nonDuplicateItems);
                IsBusy = true;
                await _Connection.InsertAllAsync(itemsToSave);
                IsBusy = false;
                SortList();
                Update();

            }

        }


        public async void  OnBuyItem(object sender, System.EventArgs e)
        {
            var menuitem = sender as MenuItem;
            var shoppingItem = menuitem.CommandParameter as ShoppingListItem;
            var parentView = Parent as GoShoppingView;
            var viewShoppingCart = parentView.ShoppingCartView;
            Removeitem(shoppingItem);
            await viewShoppingCart.AddItem(shoppingItem);
        }

        public async void OnRemoveItem(object sender, System.EventArgs e)
        {
            var menuitem = sender as MenuItem;
            var shoppingListItem = menuitem.CommandParameter as ShoppingListItem;

            await DeleteItem(shoppingListItem);
            SortList();
        }

        public void OnQuantityMinus(object sender, System.EventArgs e)
        {
            var buttonItem = sender as ImageButton;
            var shoppingListItem = buttonItem.CommandParameter as ShoppingListItem;
            if (shoppingListItem.Quantity>1)
                shoppingListItem.Quantity--;
            SortList();
            UpdateItem(shoppingListItem);

        }

        public void OnQuantityPlus(object sender, System.EventArgs e)
        {
            var buttonItem = sender as ImageButton;
            var shoppingListItem=buttonItem.CommandParameter as ShoppingListItem;
            shoppingListItem.Quantity++;
            SortList();
            UpdateItem(shoppingListItem);
        }

        public async void AddItemsClicked(object sender, System.EventArgs e)
        {
            var goShoppingView = Parent as GoShoppingView;
            var itemsSelView = new ItemsSelectionView();
            itemsSelView.ShoppingListView = goShoppingView.ShoppingListView;
            itemsSelView.ShoppingCartView = goShoppingView.ShoppingCartView;
            itemsSelView.OKPushed += SelectionListItemsAdd;

            await Navigation.PushModalAsync(itemsSelView);
        }

        public void HandleItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            ((ListView)sender).SelectedItem = null;
        }

        private void SortList()
        {
            List<ShoppingListItem> sortedList = null;
            switch (CurrentingSorting)
            {
                case SortMethod.ImportantFirst:
                    sortedList = ItemList.OrderByDescending(x => x.IsImportant).ThenBy(x => x.ShoppingListDescription).ToList();
                    break;
                case SortMethod.PriceAscending:
                    sortedList = ItemList.OrderBy(x => x.LineCost).ToList();
                    break;
                case SortMethod.PriceDescending:
                    sortedList = ItemList.OrderByDescending(x => x.LineCost).ToList();
                    break;
                case SortMethod.Alphabetical:
                    sortedList = ItemList.OrderBy(x => x.Description).ToList();
                    break;

            }
            ItemList.Clear();
            ItemList.AddRange(sortedList);
            OnPropertyChanged("ItemList");
        }
        
        public async void FilterClicked(object sender, System.EventArgs e)
        {
            var sortByString= await DisplayActionSheet("Sort By", "Cancel", null, "Important First", "Price Ascending", "Price Descending", "Alphabetical");

            switch (sortByString)
            {
                case "Important First": CurrentingSorting = SortMethod.ImportantFirst;SortList(); break;
                case "Price Ascending": CurrentingSorting = SortMethod.PriceAscending; SortList(); break;
                case "Price Descending": CurrentingSorting = SortMethod.PriceDescending;SortList(); break;
                case "Alphabetical": CurrentingSorting = SortMethod.Alphabetical;SortList(); break;

            }
        }

        public void CopyListClicked(object sender, System.EventArgs e)
        {
            CopyListToClipboard();
        }

        public  void CopyListToClipboard()
        {
            if (ItemList != null && ItemList.Count > 0)
            {

                String copyText = "Hi," + Environment.NewLine + "Please can you pick up the following items:" + Environment.NewLine;

                foreach (var item in ItemList)
                {
                    copyText += item.ShoppingListDescription + Environment.NewLine;
                }

                Clipboard.SetTextAsync(copyText);
                DisplayAlert("Copy Items", ItemList.Count() + " items copied.", "OK");
            }
            else
                DisplayAlert("Copy Items", "You don't have any items to copy.", "OK");
        }

        public async void TrashClicked(object sender, System.EventArgs e)
        {
           var result = await  DisplayAlert("Confirm", "Are you sure you want to clear the list?", "Yes", "No");
            if (result)
            {
                IsBusy = true;
                await _Connection.ExecuteAsync("delete from ShoppingCartitem where IsBought=false");
                IsBusy = false;
                ItemList.Clear();
                Update();
            }
        }
    }
}
