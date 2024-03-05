using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Interview_Project.Models
{
    public class User:IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }=DateTime.Now;
        public string PhotoPath { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}
