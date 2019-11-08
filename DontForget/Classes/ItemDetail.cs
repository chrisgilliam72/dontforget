using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace DontForget
{
    public class ItemDetail : INotifyPropertyChanged
    {
        public GroceryItem GroceryItem { get; set; }

        public String Description
        {
            get
            {
                return GroceryItem.Description;
            }
            set
            {
                GroceryItem.Description = value;
                OnPropertyChanged("Description");
            }
        }

        public decimal Price
        {
            get
            {
                return GroceryItem.Price;
            }
            set
            {
                GroceryItem.Price = value;
                OnPropertyChanged("Price");
            }
        }

        public ItemDetail()
        {
            GroceryItem = new GroceryItem();
        }

        public ItemDetail(GroceryItem groceryItem)
        {
            GroceryItem = groceryItem;
        }

        public static explicit operator ItemDetail (GroceryItem groceryItem)
        {
            var ItemDetail = new ItemDetail(groceryItem);
            return ItemDetail;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

