namespace VAKIFLOGIN.DTOs{
    public class UserListDto{
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
        public int PositionId { get; set; }
        public string PositionName { get; set; } = "";
        public int? ManagerUserId { get; set; }
        public required string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "";
        public DateTime? HireDate { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }

    }






}