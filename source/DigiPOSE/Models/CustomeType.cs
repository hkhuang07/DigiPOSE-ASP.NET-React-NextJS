using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class CustomeType
    {
        [Key] public int CustomeTypeId { get; set; }
        [Required,StringLength(50)] public string TypeName { get; set; }
        [StringLength(255)] [Display(Name = "Description")]  public string? Description { get; set; }
    }
}
