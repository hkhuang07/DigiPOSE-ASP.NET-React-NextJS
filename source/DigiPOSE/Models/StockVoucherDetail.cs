using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class StockVoucherDetail
    {
        [Key] public int VoucherDetailId { get; set; }
        public int VoucherId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ActualPrice { get; set; }

        public StockVoucher? StockVoucher { get; set; }
        public Product? Product { get; set; }
    }
}
