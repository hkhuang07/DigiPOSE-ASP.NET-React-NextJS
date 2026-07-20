using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace DigiPOSE.Models
{
    public class Branch
    {
        [Key] public int BranchId { get; set; }
        [Required, StringLength(150)][Display(Name = "Branch name")] public string BranchName { get; set; } = null!;
        [StringLength(200)] public string? Slug { get; set; }
        [StringLength(255)][Display(Name = "Address")] public string? Address { get; set; }
        [StringLength(50)][Display(Name = "Contact Phone")] public string? ContactPhone { get; set; }
        [StringLength(100)][Display(Name = "Email")] public string? Email { get; set; }
        [StringLength(100)][Display(Name = "Manager Name")] public string? ManagerName { get; set; }
        [StringLength(255)][Display(Name = "Notes")] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<User>? Users { get; set; }
        public ICollection<Counter>? Counters { get; set; }
        public ICollection<StockVoucher>? StockVourchers { get; set; }
        public ICollection<ProductInventory>? ProductInventories { get; set; }
    }
}
    