export interface LeaveType {
    leaveTypeId: number;
    name: string;
    code: string;
    isActive: boolean;
    // C# PascalCase fallbacks
    LeaveTypeId?: number;
    Name?: string;
    Code?: string;
    IsActive?: boolean;
}

export interface LeaveRequest {
    leaveRequestId: number;
    userId: number;
    firstName?: string;
    lastName?: string;
    leaveTypeName: string;
    leaveTypeId: number;
    startDateTime: string;
    endDateTime: string;
    reason: string;
    status: string;
    createdAt: string;
    submittedAt: string;
}

export interface CreateLeaveRequestDto {
    userId: number;
    leaveTypeId: number;
    startDateTime: string;
    endDateTime: string;
    reason: string;
}

export interface ApproveLeaveRequestDto {
    leaveRequestId: number;
    approverUserId: number;
    decision: string; // 'Approved' | 'Rejected'
    comment?: string;
}
