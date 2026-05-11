namespace VAKIFLOGIN.Models
{
    public class LeaveTypes
    {
        public int LeaveTypeId { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public bool IsActive { get; set; } 
    }   
} 