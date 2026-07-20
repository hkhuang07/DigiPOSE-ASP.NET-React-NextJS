using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Customer
    {
        [Key] public int CustomerId { get; set; }
        [Display(Name = "Type")] public int CustomeTypeId { get; set; }
        [Required, StringLength(100)][Display(Name = "Full Name")] public string FullName { get; set; } = null!;
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Tax Code")] public string TaxCode { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Id No")] public string IdNo { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Phone Number")] public string PhoneNumber { get; set; }
        [StringLength(100)][Display(Name = "Email")] public string Email { get; set; }
        [StringLength(255)][Display(Name = "Address")] public string Address { get; set; }
        [Display(Name = "Reward Points")] public int RewardPoints { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public CustomeType? CustomeType { get; set; }

    }
}
