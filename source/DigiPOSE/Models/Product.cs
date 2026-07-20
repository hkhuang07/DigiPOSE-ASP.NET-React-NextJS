using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Product
    {
        [Key] public int ProductId { get; set; }
        [Display(Name = "Category")] public int CategoryId { get; set; }
        [Display(Name = "Product Type")] public int ProductTypeId { get; set; }
        [Display(Name = "Unit")] public int UnitId { get; set; }
        [Display(Name = "Manufacturer")] public int? ManufacturerId { get; set; }
        [Display(Name = "Tax Type")] public int TaxTypeId { get; set; }

        [Required, StringLength(50)][Display(Name = "SKU")] public string SKU { get; set; } = null!;
        [Column(TypeName = "varchar(50)")][StringLength(50)][Display(Name = "Barcode")] public string? Barcode { get; set; }
        [Required, StringLength(150)][Display(Name = "Product Name")] public string ProductName { get; set; } = null!;
        [Range(0, double.MaxValue, ErrorMessage = "Selling Price must be non-negative")][Column(TypeName = "decimal(18,4)")][Display(Name = "Selling Price")] public decimal BasePrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Cost Price must be non-negative")][Column(TypeName = "decimal(18,4)")][Display(Name = "Cost Price")] public decimal CostPrice { get; set; } = 0;
        [StringLength(255)][Display(Name = "Image URL")] public string? ImageUrl { get; set; }
        [StringLength(200)][Display(Name = "Slug")] public string? Slug { get; set; }
        [StringLength(1000)][Display(Name = "Description")] public string? Description { get; set; }
        [Display(Name = "Min Stock Level")] public int MinStockLevel { get; set; } = 0;
        [Display(Name = "Max Stock Level")] public int MaxStockLevel { get; set; } = 1000;

        public bool IsActive { get; set; } = true;
        [Timestamp] public byte[]? RowVersion { get; set; }

        public TaxType? TaxType { get; set; }
        public Category? Category { get; set; }
        public Unit? Unit { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public ProductType? ProductType { get; set; }

        public ICollection<ProductInventory>? ProductInventories { get; set; }
        public ICollection<StockVoucherDetail>? StockVoucherDetails { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }

    }
}