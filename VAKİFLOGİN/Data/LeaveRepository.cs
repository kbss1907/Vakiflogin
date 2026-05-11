using Microsoft.Data.SqlClient;
using System.Data;
using VAKIFLOGIN.Models;
using VAKIFLOGIN.Services;
using VAKIFLOGIN.Controllers;
using VAKIFLOGIN.DTOs;
namespace VAKIFLOGIN.Data
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public LeaveRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public int createLeaveRequest(LeaveRequests leaveRequest)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_LeaveRequest_Create";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = leaveRequest.UserId });
                command.Parameters.Add(new SqlParameter("@LeaveTypeId", SqlDbType.Int) { Value = leaveRequest.LeaveTypeId });
                command.Parameters.Add(new SqlParameter("@StartDateTime", SqlDbType.DateTime) { Value = leaveRequest.StartDateTime });
                command.Parameters.Add(new SqlParameter("@EndDateTime", SqlDbType.DateTime) { Value = leaveRequest.EndDateTime });
                command.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 500) { Value = leaveRequest.Reason });
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public string approveLeaveRequest(int leaveRequestId, int approverUserId, string decision, string? comment)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_LeaveRequest_Approve";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@LeaveRequestId", SqlDbType.Int) { Value = leaveRequestId });
                command.Parameters.Add(new SqlParameter("@ApproverUserId", SqlDbType.Int) { Value = approverUserId });
                command.Parameters.Add(new SqlParameter("@Decision", SqlDbType.NVarChar, 50) { Value = decision });
                command.Parameters.Add(new SqlParameter("@Comment", SqlDbType.NVarChar, 255) { Value = comment ?? (object)DBNull.Value });
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
                

                return "İşlem Başarılı";
            }
        }
        public List<LeaveRequestListDto> GetLeaveRequestsByUserId(int userId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_LeaveRequest_GetByUserId";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (var reader = command.ExecuteReader())
                {
                    var leaveRequests = new List<LeaveRequestListDto>();
                    while (reader.Read())
                    {
                        leaveRequests.Add(new LeaveRequestListDto
                        {
                            LeaveRequestId = Convert.ToInt32(reader["LeaveRequestId"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            LeaveTypeName = reader["LeaveTypeName"]?.ToString() ?? "",
                            LeaveTypeId = Convert.ToInt32(reader["LeaveTypeId"]),
                            StartDateTime = Convert.ToDateTime(reader["StartDateTime"]),
                            EndDateTime = Convert.ToDateTime(reader["EndDateTime"]),
                            Reason = reader["Reason"]?.ToString() ?? "",
                            Status = reader["Status"]?.ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            SubmittedAt = reader["SubmittedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["SubmittedAt"])

                        });
                    }
                    return leaveRequests;
                }
            }
        }
        public List<LeaveTypes>GetLeaveTypes()
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_LeaveType_GetAll";
                command.CommandType = CommandType.StoredProcedure;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (var reader = command.ExecuteReader())
                {
                    var leaveTypes = new List<LeaveTypes>();
                    while (reader.Read())
                    {
                        leaveTypes.Add(new LeaveTypes
                        {
                            LeaveTypeId = Convert.ToInt32(reader["LeaveTypeId"]),
                            Name = reader["Name"]?.ToString() ?? "",
                            Code = reader["Code"]?.ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        });
                    }
                    return leaveTypes;
                }
            }
        }
        public List<LeaveRequestListDto> GetPendingLeaveRequests()
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_LeaveRequest_GetPending";
                command.CommandType = CommandType.StoredProcedure;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (var reader = command.ExecuteReader())
                {
                    var leaveRequests = new List<LeaveRequestListDto>();
                    while (reader.Read())
                    {
                        leaveRequests.Add(new LeaveRequestListDto
                        {
                            LeaveRequestId = Convert.ToInt32(reader["LeaveRequestId"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            FirstName = reader["FirstName"]?.ToString() ?? "",
                            LastName = reader["LastName"]?.ToString() ?? "",
                            LeaveTypeName = reader["LeaveTypeName"]?.ToString() ?? "",
                            LeaveTypeId = Convert.ToInt32(reader["LeaveTypeId"]),
                            StartDateTime = Convert.ToDateTime(reader["StartDateTime"]),
                            EndDateTime = Convert.ToDateTime(reader["EndDateTime"]),
                            Reason = reader["Reason"]?.ToString() ?? "",
                            Status = reader["Status"]?.ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            SubmittedAt = reader["SubmittedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["SubmittedAt"])

                        });
                    }
                    return leaveRequests;
                }
            }
        }
    }
}