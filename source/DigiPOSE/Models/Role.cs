using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Role
    {
        [Key] public int RoleId { get; set; }
        [Required, StringLength(150)][Display(Name = "Role name")] public string RoleName { get; set; } = null!;
        [StringLength(255)][Display(Name = "Description")] public string? Description { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
