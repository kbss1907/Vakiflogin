using Microsoft.AspNetCore.Mvc;
using VAKIFLOGIN.Data;
using VAKIFLOGIN.Models;
using VAKIFLOGIN.Services;
using VAKIFLOGIN.Controllers;
using Microsoft.AspNetCore.Authorization;
using VAKIFLOGIN.DTOs;
namespace VAKIFLOGIN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveRepository _leaveRepository;

        public LeaveController(ILeaveRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }
        [HttpPost("request")]
        public IActionResult CreateLeaveRequest([FromBody] LeaveRequestDto dto)
        {
            try
            {
                // IDOR Fix: UserId'yi JWT claim'den al, client'a güvenme
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                
                var leaveRequest = new LeaveRequests
                {
                    UserId = int.Parse(userIdClaim.Value),
                    LeaveTypeId = dto.LeaveTypeId,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,
                    Reason = dto.Reason ?? ""
                };
                _leaveRepository.createLeaveRequest(leaveRequest);
                return Ok(new { message = "İzin talebi başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("approve")]
        public IActionResult ApproveLeaveRequest([FromBody] LeaveApprovalDto dto)
        {
            try
            {
                // IDOR Fix: ApproverUserId'yi JWT claim'den al
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                
                var approverUserId = int.Parse(userIdClaim.Value);
                _leaveRepository.approveLeaveRequest(dto.LeaveRequestId, approverUserId, dto.Decision, dto.Comment);
                return Ok(new { message = "İşlem başarılı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("user/{userId}")]
        public IActionResult GetLeaveRequestsByUserId(int userId)
        {
            // IDOR Fix: Route'taki userId'yi yoksay, claim'den al
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            var authenticatedUserId = int.Parse(userIdClaim.Value);
            var leaveRequests = _leaveRepository.GetLeaveRequestsByUserId(authenticatedUserId);
            return Ok(leaveRequests);
        }
        [HttpGet("types")]
        [AllowAnonymous]
        public IActionResult GetLeaveTypes()
        {
            try
            {
                var types = _leaveRepository.GetLeaveTypes();
                
                if (types == null || !types.Any())
                {
                    return Ok(new { message = "Sistemde kayıtlı aktif izin türü bulunmamaktadır." });
                }
                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "İzin türleri alınırken bir hata oluştu.", error = ex.Message });
            }
        }
        [HttpGet("pending")]
        public IActionResult GetPendingLeaveRequests()
        {
            try
            {
                var leaveRequests = _leaveRepository.GetPendingLeaveRequests();
                if (leaveRequests == null || !leaveRequests.Any())
                {
                    return Ok(new { message = "Bekleyen izin talebi bulunmamaktadır." });
                }
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Bekleyen izin talepleri alınırken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}