namespace VAKIFLOGIN.Models
{
    public class LeaveApprovals
    {
        public int LeaveApprovalId { get; set; }
        public int LeaveRequestId { get; set; }
        public int ApproverUserId { get; set; }
        public DateTime DecisionAt { get; set; }
        public string? Comment { get; set; }
        public int StepNo { get; set; } 
        public string Decision { get; set; }= "";
    }   
} 