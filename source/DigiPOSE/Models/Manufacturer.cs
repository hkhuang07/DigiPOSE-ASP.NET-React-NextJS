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

        [StringLength(100)][Display(Name = "Contact Person")] public string? ContactPerson { get; set; }
        [StringLength(20)][Display(Name = "Phone")] public string? Phone { get; set; }
        [StringLength(100)][Display(Name = "Email")] public string? Email { get; set; }
        [StringLength(255)][Display(Name = "Address")] public string? Address { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Tax Code")] public string? TaxCode { get; set; }
        [StringLength(150)][Display(Name = "Website")] public string? Website { get; set; }
        [StringLength(500)][Display(Name = "Description")] public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Product>? Products { get; set; }
    }
}
