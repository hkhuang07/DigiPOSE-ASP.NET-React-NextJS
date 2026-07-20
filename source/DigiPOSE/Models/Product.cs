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
        [Required, StringLength(150)][Display(Name = "Product Name")] public string ProductName { get; set; } = null!;
        [Column(TypeName = "decimal(18,0)")][Display(Name = "Selling Price")] public decimal BasePrice { get; set; }
        [StringLength(255)][Display(Name = "Image URL")] public string? ImageUrl { get; set; }
        [StringLength(200)][Display(Name = "Slug")] public string? Slug { get; set; }

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