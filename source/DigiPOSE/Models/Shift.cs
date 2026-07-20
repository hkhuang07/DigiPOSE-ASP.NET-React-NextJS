using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Shift
    {
        [Key] public int ShiftId { get; set; }
        [Display(Name = "Employee")] public int UserId { get; set; }
        [Display(Name = "Counter")] public int CounterId { get; set; }
        [Display(Name = "Status")] public int StatusId { get; set; } 
        [Display(Name = "Start time")] public DateTime StartTime { get; set; }
        [Display(Name = "End time")] public DateTime? EndTime { get; set; }
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Start Cash")] public decimal StartCash { get; set; }
        [Column(TypeName = "decimal(18,4)")][Display(Name = "End Cash")] public decimal? EndCash { get; set; }

        public User? User { get; set; }
        public Counter? Counter { get; set; }
        public ShiftStatus? Status { get; set; }
    }
}
