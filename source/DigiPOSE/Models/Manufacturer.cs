using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Manufacturer
    {
        [Key] public int ManufacturerId { get; set; }
        [Required, StringLength(100)][Display(Name = "Manufacturer name")] public string ManufacturerName { get; set; } = null!;
        [StringLength(200)] public string? Slug { get; set; }
        [StringLength(255)][Display(Name = "Logo URL")] public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Product>? Products { get; set; }
    }
}
