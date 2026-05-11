namespace VAKIFLOGIN.Models
{
    public class Position
    {
        public int PositionId { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public bool IsActive { get; set; } 
        public int Level { get; set; }
    }   
} 