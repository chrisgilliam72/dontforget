using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DontForget.Helpers;
using DontForget.Persistence;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DontForget
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsSelectionView : ContentPage
    {
        private string _filter;
        private SQLiteAsyncConnection _Connection;
        public ShoppingBillView ShoppingCartView { get; set; }
        public ShoppingListView ShoppingListView { get; set; }
        public EventHandler OKPushed;

        public List<ShoppingListItem> SelectedItems
        {
            get
            {
                var tmpList = Items.Where(x => x.IsSelected).ToList();
                return tmpList;
            }
        }

        private RangeObservableCollection<ShoppingListItem> _items;
        public RangeObservableCollection<ShoppingListItem> Items
        {
            get
            {
                if (_items!=null)
                {
                   
                    var tmpCollection = new RangeObservableCollection<ShoppingListItem>();
                    List<ShoppingListItem> tmpLst = null;
                    if (string.IsNullOrEmpty(_filter))
                        tmpLst = _items.OrderBy(x => x.Description).ToList();
                    else
                        tmpLst = _items.Where(x => x.Description.StartsWith(_filter)).OrderBy(x => x.Description).ToList();

                    tmpCollection.AddRange(tmpLst);
                    return tmpCollection;
                }
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        public ItemsSelectionView()
        {
            InitializeComponent();
            BindingContext = this;
            _Connection = DependencyService.Get<ISQLiteDB>().GetConnection();
            Items = new RangeObservableCollection<ShoppingListItem>();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            _items.Clear();
            await _Connection.CreateTableAsync<GroceryItem>();
            var itemList = await _Connection.Table<GroceryItem>().ToListAsync();

            var tmpSelectableList=itemList.Select(x => new ShoppingListItem(x)).ToList();

            if (ShoppingListView!=null && ShoppingCartView!=null)
            { 
                foreach (var item in tmpSelectableList)
                {
                    if (!ShoppingListView.HasItem(item.ItemID) && !ShoppingCartView.HasItem(item.ItemID))
                        _items.Add(item);
                }
            }


            OnPropertyChanged("Items");
        }

         void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            var selectedItem = e.Item as ShoppingListItem;
            selectedItem.IsSelected = !selectedItem.IsSelected;
            ((ListView)sender).SelectedItem = null;
        }


        void HandleCancelClicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        void HandleOKClicked(object sender, System.EventArgs e)
        {
            OKPushed(this, EventArgs.Empty);
            Navigation.PopModalAsync();
        }

        void SearchTextChangedHandled(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            _filter = e.NewTextValue;
            OnPropertyChanged("Items");
        }
    }
}
