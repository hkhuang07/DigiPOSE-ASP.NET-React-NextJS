using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class InvoiceStatus
    {
        [Key] public int InvoiceStatusId { get; set; }
        [Required, StringLength(100)] public string StatusName { get; set; } = null!;
        [StringLength(255)] public string? Description { get; set; }

        public ICollection<Invoice>? Invoices { get; set; }
    }
}