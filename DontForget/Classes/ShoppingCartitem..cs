using System;
using SQLite;

namespace DontForget
{
    public class ShoppingCartitem
    {
        [PrimaryKey, AutoIncrement]
        public int ShoppingCartItemID { get; set; }
        public int GroceryItemID { get; set; }
        public int Quantity { get; set; }
        public bool IsBought { get; set; }

        public ShoppingCartitem()
        {
            IsBought = false;
        }

        public static explicit operator ShoppingCartitem(GroceryItem groceryItem)
        {
            var shoppingCartItem = new ShoppingCartitem();
            shoppingCartItem.GroceryItemID = groceryItem.ItemID;
            shoppingCartItem.Quantity = 1;
            return shoppingCartItem;
        }
    }
}
