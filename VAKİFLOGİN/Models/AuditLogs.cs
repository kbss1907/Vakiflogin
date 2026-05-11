namespace VAKIFLOGIN.Models
{
    public class AuditLogs
    {
        public int AuditLogId { get; set; }
        public int ActorUserId { get; set; }
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public int EntityId { get; set; } 
        public DateTime CreatedAt { get; set; }    
    }   
} 