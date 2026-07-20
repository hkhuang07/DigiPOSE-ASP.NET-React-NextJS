using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class ProductInventory
    {
        [Key] public int InventoryId { get; set; }
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int StockQuantity { get; set; } = 0;
        public int MinStockLevel { get; set; } = 0;

        [Timestamp] public byte[]? RowVersion { get; set; }

        public Branch? Branch { get; set; }
        public Product? Product { get; set; }
    }
}