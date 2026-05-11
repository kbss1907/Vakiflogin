using VAKIFLOGIN.Models;
using VAKIFLOGIN.DTOs;

namespace VAKIFLOGIN.Data{
    public interface ILeaveRepository
    {
      int createLeaveRequest(LeaveRequests leaveRequest);
      string approveLeaveRequest(int leaveRequestId, int approverUserId, string decision, string? comment);
      List<LeaveRequestListDto> GetLeaveRequestsByUserId(int userId);
      List<LeaveTypes> GetLeaveTypes();
      List<LeaveRequestListDto> GetPendingLeaveRequests();


    }
}