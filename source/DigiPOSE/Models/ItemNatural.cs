using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Models
{
    //Định nghĩa Tính chất dòng bán (Ví dụ: 1. Bán thường, 2. Khuyến mãi, 3. Chiết khấu)
    public class ItemNature
    {
        [Key] public int NatureId { get; set; }

        [Required, StringLength(100)]
        public string NatureName { get; set; } = null!;

        // Mã dùng để đẩy lên API của Cơ quan Thuế (1, 2, 3, 4 theo Thông tư 78)
        [StringLength(50)]
        public string? TaxXmlCode { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}