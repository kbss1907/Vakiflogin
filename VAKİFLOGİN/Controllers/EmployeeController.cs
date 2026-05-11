using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VAKIFLOGIN.Data;
using VAKIFLOGIN.DTOs;
namespace VAKIFLOGIN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public EmployeeController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        
        [HttpGet("listDto")]
        public IActionResult GetEmployeeListDto()
        {
            try
            {
                var list = _userRepository.GetAllUserListDto();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Liste alınırken bir hata oluştu", error = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = _userRepository.GetUserById(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Kullanıcı alınırken bir hata oluştu", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            try
            {
                _userRepository.UpdateEmployee(id, dto);
                return Ok(new { message = "Kullanıcı güncellendi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Kullanıcı güncellenirken bir hata oluştu", error = ex.Message });
            }
        }
        [HttpPut("{id}/role")]
        public IActionResult AssignRole(int id, [FromBody] AssignRoleDto dto)
        {
            try
            {
                _userRepository.AssignRole(id, dto.RoleId);
                return Ok(new { message = "Rol başarıyla atandı" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Rol atanırken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchUser(string searchTerm = "", int? departmentId = null, string status = "")
        {
            try
            {
                var list = _userRepository.SearchUser(searchTerm, departmentId, status);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Liste alınırken bir hata oluştu" });
            }
        }
        
    }
    
}
