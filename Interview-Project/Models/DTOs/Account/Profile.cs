namespace Interview_Project.Models.DTOs.Account
{
    public class ProfileDto
    {
        public string  userId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Jwt { get; set; }
        public string PhotoPath { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
