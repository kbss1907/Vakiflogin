namespace VAKIFLOGIN.DTOs
{
    public class UpdateEmployeeDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Phone { get; set; }
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public required string Status { get; set; }
    }
}