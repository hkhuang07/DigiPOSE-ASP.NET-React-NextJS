using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class ShiftStatus
    {
        [Key] public int StatusId { get; set; }
        [Required, StringLength(50)][Display(Name = "Shift status name")] public string StatusName { get; set; } = null!;
        [StringLength(255)][Display(Name = "Description")] public string? Description { get; set; }
        public ICollection<Shift>? Shifts { get; set; }
    }
}