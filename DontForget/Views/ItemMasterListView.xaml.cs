
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DontForget.Helpers;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.FilePicker;
using Newtonsoft.Json;
using PCLStorage;
using System.IO;
using DontForget.Persistence;
using System;


namespace DontForget
{

    public partial class ItemMasterListView : ContentPage
    {
        private string _filter;
        private SQLiteAsyncConnection _Connection;


        public RangeObservableCollection<ItemDetail> Items { get; set; }


        public RangeObservableCollection<ItemDetail> SortedItems
        {
            get
            {
                var tmpCollection = new RangeObservableCollection<ItemDetail>();
                List<ItemDetail> tmpLst = null;
                if (string.IsNullOrEmpty(_filter))
                    tmpLst = Items.OrderBy(x => x.Description).ToList();
                else
                    tmpLst = Items.Where(x => x.Description.StartsWith(_filter)).OrderBy(x => x.Description).ToList();

                tmpCollection.AddRange(tmpLst);
                return tmpCollection;
            }
        }


        public ItemMasterListView()
        {
            InitializeComponent();
            Items = new RangeObservableCollection<ItemDetail>();
            BindingContext = this;
            _Connection = DependencyService.Get<ISQLiteDB>().GetConnection();
           
        }



        public async Task Refresh()
        {
            Items.Clear();
            await _Connection.CreateTableAsync<GroceryItem>();
            var groceryItems = await _Connection.Table<GroceryItem>().OrderBy(x=>x.Description).ToListAsync();
            var itemList = groceryItems.Select(x => (ItemDetail)x).ToList();
            Items.AddRange(itemList);
            OnPropertyChanged("SortedItems");
        }

        async void ExportItems_Clicked(object sender, System.EventArgs e)
        {
            try
            {

                var jsonData = JsonConvert.SerializeObject(Items);

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var docsFolder = await FileSystem.Current.GetFolderFromPathAsync(documents);


                var file = await docsFolder.CreateFileAsync("shoppinglistitems.json", CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(jsonData);
                }

                await DisplayAlert("Items exported!", "Please connect this device to itunes in order to transfer your list.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Export failed", "Unable to write export file", "OK");
            }

        }


        async void ImportItems_Clicked(object sender, System.EventArgs e)
        {

            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var docsFolder = await FileSystem.Current.GetFolderFromPathAsync(documents);

                if (File.Exists(documents + @"/shoppinglistitems.json"))
                {
                    var file = await docsFolder.GetFileAsync("shoppinglistitems.json");
                    IsBusy = true;
                    using (var stream = await file.OpenAsync(PCLStorage.FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        var fileinfo = new FileInfo(file.Path);
                        char[] buffer = new char[fileinfo.Length];
                        reader.Read(buffer, 0, (int)fileinfo.Length);
                        var jsonStr = new String(buffer);
                        var importedItems = JsonConvert.DeserializeObject<List<ItemDetail>>(jsonStr);

                        if (Items.Count > 0)
                        {
                            var mergeSelection = await DisplayActionSheet("Merge or Overwrite existing list ?", "Cancel", "Merge", "Overwrite");
                            if (mergeSelection == "Cancel")
                                return;

                            if (mergeSelection == "Overwrite")
                            {
                                await _Connection.ExecuteAsync("delete from GroceryItem");
                                var groceryItems = importedItems.Select(x => x.GroceryItem).ToList();
                                await _Connection.InsertAllAsync(groceryItems);
                                await Refresh();
                            }
                            else
                            {
                                var nonDuplicatedItems = new List<ItemDetail>();
                                foreach (var importedItem in importedItems)
                                {
                                    var duplicateItem = Items.FirstOrDefault(x => x.Description == importedItem.Description);
                                    if (duplicateItem == null)
                                        nonDuplicatedItems.Add(importedItem);
                                }


                                var groceryItems = nonDuplicatedItems.Select(x => x.GroceryItem).ToList();
                                await _Connection.InsertAllAsync(groceryItems);
                                await Refresh();

                            }

                        }
                        else
                        {
                            var groceryItems = importedItems.Select(x => x.GroceryItem).ToList();
                            await _Connection.InsertAllAsync(groceryItems);
                            await Refresh();

                        }

                    }
                    IsBusy = false;

                }
                else
                    await DisplayAlert("Import failed", "Unable to locate export file", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Import failed", "An error occured trying to read the import file", "OK");
            }
          
        }


        async void OnEditItem(object sender, System.EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var itemToUpdate = menuItem.CommandParameter as ItemDetail;
            if (itemToUpdate != null)
            {
                var itemDetaiLView = new ItemDetailView(itemToUpdate.GroceryItem);
                itemDetaiLView.IsEditing = true;
                itemDetaiLView.ItemUpdated += UpdateItem;
                await Navigation.PushModalAsync(itemDetaiLView);

                //Deselect Item
                itemListView.SelectedItem = null;
            }

        }

        async void AddItem_Clicked(object sender, System.EventArgs e)
        {
            var itemDetaiLView = new ItemDetailView(new GroceryItem());
            itemDetaiLView.ListExistingItems = Items.ToList();
            itemDetaiLView.ItemAdded += AddItem;
            await Navigation.PushModalAsync(itemDetaiLView);
        }

        async void OnDeleteItem(object sender, System.EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var itemDetail = menuItem.CommandParameter as ItemDetail;
            if (itemDetail != null)
            {
                var dbCartItem = await _Connection.Table<ShoppingCartitem>().FirstOrDefaultAsync(x => x.GroceryItemID == itemDetail.GroceryItem.ItemID);
                if (dbCartItem == null)
                {
                    var item = Items.FirstOrDefault(x => x.GroceryItem.ItemID == itemDetail.GroceryItem.ItemID);
                    if (item != null)
                    {
                        Items.Remove(item);
                        OnPropertyChanged("Items");
                        OnPropertyChanged("SortedItems");
                        await _Connection.DeleteAsync(item.GroceryItem);

                    }
                }
                else
                {
                    await DisplayAlert("Delete failed", "Please remove this item from your shopping list.", "OK");
                }
            }
        }

        public List<ItemDetail> GetItems(List<int> itemIDS)
        {
            return Items.Where(x => itemIDS.Contains(x.GroceryItem.ItemID)).ToList();
        }

        public void AddItem (object sender, System.EventArgs e)
        {
            var groceryItem = sender as GroceryItem;
            var newItem = (ItemDetail)groceryItem;
            Items.Add(newItem);
            _Connection.InsertAsync(groceryItem);
            OnPropertyChanged("SortedItems");
        }

        public void UpdateItem(object sender, System.EventArgs e)
        {
            var itemToUpdate = sender as GroceryItem;
            if (itemToUpdate!=null)
            {
                var item = Items.FirstOrDefault(x => x.GroceryItem.ItemID == itemToUpdate.ItemID);
                if (item!=null)
                {
                    item.Description = itemToUpdate.Description;
                    item.Price = itemToUpdate.Price;
                    _Connection.UpdateAsync(item.GroceryItem);
                    OnPropertyChanged("SortedItems");
                }
            }

        }


        public async void HandleRefreshing(object sender, EventArgs e)
        {
            var lstView = sender as ListView;
            await Refresh();
            lstView.IsRefreshing = false;
        }
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {

            var goShoppingView = Parent as GoShoppingView;

            if (e.Item == null)
                return;

            var selectedItem = e.Item as ItemDetail;
            if (selectedItem != null)
            {
               await goShoppingView.ShoppingListView.SaveItem(selectedItem.GroceryItem);
               await goShoppingView.ShoppingListView.Refresh();

            }
            itemListView.SelectedItem = null;
        }

       void SearchTextChangedHandled (object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            _filter = e.NewTextValue;
            OnPropertyChanged("SortedItems");
        }
    }
}
