using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Counter
    {
        [Key] public int CounterId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        [Required, StringLength(50)][Display(Name = "Counter name")] public string CounterName { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public Branch? Branch { get; set; }

        public ICollection<Shift>? Shifts { get; set; }

    }
}
