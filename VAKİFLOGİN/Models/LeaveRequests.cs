namespace VAKIFLOGIN.Models
{
    public class LeaveRequests
    {
        public int LeaveRequestId { get; set; }
        public int UserId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Reason { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
    }  
} 