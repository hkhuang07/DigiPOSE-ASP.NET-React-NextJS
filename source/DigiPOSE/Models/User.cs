using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace DigiPOSE.Models
{
    public class User
    {
        [Key] public int UserId { get; set; }
        [Display(Name = "Role")] public int RoleId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        [Required, StringLength(150)][Display(Name = "User name")] public string UserName { get; set; } = null!;
        [Required, StringLength(150)][Display(Name = "Password Hash")] public string PasswordHash { get; set; } = null!;
        [Display(Name = "Is Active")] public bool IsActive { get; set; } = true;

        public Role? Role { get; set; }
        public Branch? Branch { get; set; }
    }
}
