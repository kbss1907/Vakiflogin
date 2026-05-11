namespace VAKIFLOGIN.DTOs
{
    public class LeaveRequestListDto
    {
        public int LeaveRequestId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public required string LeaveTypeName { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public required string Reason { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime SubmittedAt { get; set;}
    }
}