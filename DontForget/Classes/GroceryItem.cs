using System;
using SQLite;
namespace DontForget
{
    public class GroceryItem
    {
        [PrimaryKey,AutoIncrement]
        public int ItemID { get; set; }

        public String Description { get; set; }
        
        public Decimal Price { get; set; }

        public bool IsImportant { get; set; }
    }
}
