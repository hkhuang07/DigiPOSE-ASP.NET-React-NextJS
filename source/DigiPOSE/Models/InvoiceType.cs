using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class InvoiceType
    {
        [Key] public int InvoiceTypeId { get; set; }
        [Required, StringLength(255)] public string TypeName { get; set; } = null!;
        [StringLength(255)] public string? Description { get; set; }

        public ICollection<Invoice>? Invoices { get; set; }
    }
}