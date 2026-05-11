namespace VAKIFLOGIN.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public int ManagerUserId { get; set; }
        public int CreatorAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? HireDate { get; set; }
    }
}