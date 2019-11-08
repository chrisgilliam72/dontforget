using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xamarin.Forms;

namespace DontForget
{
    public partial class ItemDetailView : ContentPage
    {

        public EventHandler ItemAdded { get; set; }
        public EventHandler ItemUpdated { get; set; }

        public String Description { get; set; }

        public String Price { get; set; }

        public bool IsImportant { get; set; }

        public bool IsEditing { get; set; }

        private GroceryItem _Item;

        public List<ItemDetail> ListExistingItems { get; set; }

        public ItemDetailView(GroceryItem groceryItem)
        {
            InitializeComponent();
            IsEditing = false;

            Description = groceryItem.Description;
            Price = groceryItem.Price.ToString("F2");
            IsImportant = groceryItem.IsImportant;
            _Item = groceryItem;
            BindingContext = this;
        }

        public void OnPriceChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (!String.IsNullOrEmpty(e.NewTextValue))
            {
                var charArry = e.NewTextValue.ToCharArray();
                var isValid = charArry.All(X => char.IsDigit(X));
                entry.Text = isValid ? e.NewTextValue : e.NewTextValue.Remove(e.NewTextValue.Length - 1);
            }

        }

        public void HandleCancelClicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        public void HandleOKClicked(object sender, System.EventArgs e)
        {

            if (_Item!=null)
            {
                if (String.IsNullOrEmpty(Description) || String.IsNullOrWhiteSpace(Description))
                {
                    DisplayAlert("Error", "Item description must be entered.", "OK");
                    return;
                }
                
                if (Price.Contains(","))
                    Price = Price.Replace(',', '.');
                decimal decResult;
                if (Decimal.TryParse(Price, out decResult)) 
                    _Item.Price = Convert.ToDecimal(Price, new CultureInfo("en-US"));
                else
                    _Item.Price = 0;
                _Item.Description = Description;
                _Item.IsImportant = IsImportant;
                if (IsEditing)
                    ItemUpdated(_Item, EventArgs.Empty);
                else
                {
                    if (ListExistingItems.FirstOrDefault(x => x.Description == Description)==null)
                        ItemAdded(_Item, EventArgs.Empty);
                    else
                    {
                        DisplayAlert("Error","You have already added an item with this name.","OK");
                        return;
                    }
                }

                Navigation.PopModalAsync();
            }

        }
    }
}
