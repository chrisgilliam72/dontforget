using System;
using System.Linq;
using System.Threading.Tasks;
using DontForget.Helpers;
using DontForget.Persistence;
using SQLite;
using Xamarin.Forms;

namespace DontForget
{
    public class ItemListView : ContentPage
    {

        public int ItemCount
        {
            get
            {

                return ItemList != null ? ItemList.Count : 0;
            }
        }

        public decimal TotalCost
        {
            get
            {
                return ItemList != null ? ItemList.Sum(x => x.LineCost) : 0;
            }

        }

        protected SQLiteAsyncConnection _Connection;
        public RangeObservableCollection<ShoppingListItem> ItemList { get; set; }
        public ItemListView()
        {

            ItemList = new RangeObservableCollection<ShoppingListItem>();
            _Connection = DependencyService.Get<ISQLiteDB>().GetConnection();
        }



        protected async Task Refresh(bool boughtItens)
        {
            var parentWindow = this.Parent as GoShoppingView;

            ItemList.Clear();
            await _Connection.CreateTableAsync<ShoppingCartitem>();
            var cartItems = await _Connection.Table<ShoppingCartitem>().Where(x=>x.IsBought== boughtItens).ToListAsync();
            if (cartItems != null && cartItems.Count > 0)
            {
                var masterListView = parentWindow.MasterListView;
                var groceryItems = masterListView.GetItems(cartItems.Select(x => x.GroceryItemID).ToList());
                var shoppingListItems = groceryItems.Select(x => new ShoppingListItem(x)).ToList();
                foreach (var item in shoppingListItems)
                {
                    var cartItem = cartItems.FirstOrDefault(x => x.GroceryItemID == item.ItemID);
                    item.Quantity = cartItem != null ? cartItem.Quantity :  0;

                }
                ItemList.AddRange(shoppingListItems);
                Update();
            }

        }

        public bool HasItem(int itemID)
        {
            if (ItemList!=null && ItemList.Count >0)
                return (ItemList.FirstOrDefault(x => x.ItemID == itemID) != null);
            return false;
        }

        public bool HasItem(String itemDesription)
        {
            if (ItemList != null && ItemList.Count > 0)
                return (ItemList.FirstOrDefault(x => x.Description == itemDesription) != null);
            return false;
        }

        public void Update()
        {
            OnPropertyChanged("ItemList");
            OnPropertyChanged("ItemCount");
            OnPropertyChanged("TotalCost");
        }
    }
}

