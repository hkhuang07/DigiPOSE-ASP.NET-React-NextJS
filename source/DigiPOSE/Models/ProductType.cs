using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class ProductType
    {
        [Key] public int ProductTypeId { get; set; }
        [Required, StringLength(100)][Display(Name = "Type Name")]
        public string TypeName { get; set; } = null!;

        // Cờ Logic Lõi: True = Phải kiểm tra và trừ tồn kho, False = Bán thoải mái (Dịch vụ)
        [Display(Name = "Inventory Tracked")]
        public bool IsInventoryTracked { get; set; } = true;
        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
        public ICollection<Product>? Products { get; set; }
    }
}