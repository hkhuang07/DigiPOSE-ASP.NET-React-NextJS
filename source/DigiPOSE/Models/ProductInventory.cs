using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class ProductInventory
    {
        [Key] public int InventoryId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        [Display(Name = "Product")] public int ProductId { get; set; }
        [Display(Name = "Stock Qty")] public int StockQuantity { get; set; } = 0;
        [Display(Name = "Min Level")] public int MinStockLevel { get; set; } = 0;

        [Timestamp] public byte[]? RowVersion { get; set; }

        public Branch? Branch { get; set; }
        public Product? Product { get; set; }
    }
}