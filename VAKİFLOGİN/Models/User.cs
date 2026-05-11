using System.Text.Json.Serialization;
namespace VAKIFLOGIN.Models

{
    public class User
    {
        public int Id { get; set;}
        public string Phone { get; set; } = ""; 
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }  
        public int ManagerUserId { get; set; }     
        public int  CreatorAdminId { get; set; } 
        public string Email { get; set; } = "";
        [JsonIgnore] 
        public byte[] PasswordHash { get; set; } = [];
        [JsonIgnore] 
        public byte[] PasswordSalt { get; set; } = [];
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";  
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public  DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = "";
        public DateTime? HireDate { get; set; }

    }
}
