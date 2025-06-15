using System.ComponentModel.DataAnnotations; 
using Microsoft.AspNetCore.Identity;

namespace Bai3_WebBanHang.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required] 
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}