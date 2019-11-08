using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace DontForget
{
    public class ShoppingListItem : GroceryItem, INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private int _quantity;
        public int Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
                OnPropertyChanged("LineCost");
                OnPropertyChanged("LineCostString");
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Description");
                OnPropertyChanged("ShoppingListDescription");
            }
        }

        public decimal LineCost
        {
            get
            {
                return Price * Quantity;
            }
        }
        public String LineCostString
        {
            get
            {
                var tmpLineCostString = (LineCost).ToString("F2");
                return tmpLineCostString;
            }
        }


        public String ShoppingListDescription
        {
            get
            {
                if (_quantity < 2)
                    return Description;
                else
                    return Description + " x " + _quantity;
            }
        }

        public Color TextColor
        {
            get
            {
                return IsImportant ? Color.Red : Color.Black;
            }
        }

        public ShoppingListItem(ItemDetail item)
        {
            ItemID = item.GroceryItem.ItemID;
            Price = item.GroceryItem.Price;
            Description = item.GroceryItem.Description;
            IsImportant = item.GroceryItem.IsImportant;
            IsSelected = false;
            Quantity = 1;
        }

        public ShoppingListItem(GroceryItem item)
        {

            ItemID = item.ItemID;
            Price = item.Price;
            Description = item.Description;
            IsImportant = item.IsImportant;
            IsSelected = false;
            Quantity = 1;

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

