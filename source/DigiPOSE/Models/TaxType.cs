using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class TaxType
    {
        [Key] public int TaxTypeId { get; set; }

        [Required, StringLength(100)]
        public string TaxName { get; set; } = null!; // VD: "Thuế GTGT 8%", "Không chịu thuế"

        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxPercentage { get; set; }

        [Required, StringLength(20)]
        public string TaxXmlCode { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Product>? Products { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}