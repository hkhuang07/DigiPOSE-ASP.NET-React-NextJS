using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class CustomeType
    {
        [Key] public int CustomeTypeId { get; set; }
        [Required, StringLength(100)][Display(Name = "Customer Type")] public string TypeName { get; set; } = null!;
        [StringLength(255)] [Display(Name = "Description")]  public string? Description { get; set; }
    }
}
