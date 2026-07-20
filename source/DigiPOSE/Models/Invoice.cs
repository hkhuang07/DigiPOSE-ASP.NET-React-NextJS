using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Invoice
    {
        [Key] public int InvoiceId { get; set; } 
        public int OrderId { get; set; } 

        [Display(Name = "Invoice Status")] public int InvoiceStatusId { get; set; }
        [Display(Name = "Invoice Type")] public int InvoiceTypeId { get; set; }

        [Required, StringLength(20)][Display(Name = "Form")] public string Form { get; set; } = null!;   // Mẫu số
        [Required, StringLength(20)][Display(Name = "Series")] public string Series { get; set; } = null!; // Ký hiệu
        [Required, StringLength(50)][Display(Name = "Invoice No")] public string InvoiceNo { get; set; } = null!; // Số hóa đơn

        [Display(Name = "Invoice Date")] public DateTime Date { get; set; } = DateTime.Now;
        public DateTime? SignedDate { get; set; }

        [Column(TypeName = "decimal(18,4)")] public decimal? ExchangeRate { get; set; }
        [StringLength(10)] public string? CurrencyCode { get; set; } 

        [StringLength(50)] public string? OriginalInvoiceNo { get; set; }
        [StringLength(255)] public string? OriginInvoiceReason { get; set; }

        [StringLength(50)] public string? TaxAuthorityCode { get; set; }
        [StringLength(100)] public string? InvoiceLookupCode { get; set; }

        public Order Order { get; set; } = null!;
        public InvoiceStatus? InvoiceStatus { get; set; }
        public InvoiceType? InvoiceType { get; set; }
    }
}