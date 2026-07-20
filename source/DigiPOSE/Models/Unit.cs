using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Unit
    {
        [Key] public int UnitId { get; set; }
        [Required, StringLength(50)][Display(Name = "Unit name")] public string UnitName { get; set; } 
        [StringLength(255)][Display(Name = "Description")] public string? Description { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
