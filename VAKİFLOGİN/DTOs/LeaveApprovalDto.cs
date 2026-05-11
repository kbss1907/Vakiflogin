namespace VAKIFLOGIN.DTOs
{
    public class LeaveApprovalDto
    {
        public int LeaveRequestId { get; set; }
        public string Decision { get; set; } ="";
        public string? Comment { get; set; }
    }
}