namespace VAKIFLOGIN.DTOs
{
    public class LeaveRequestDto
    {
        public int LeaveTypeId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Reason { get; set; } = "";
    }
}