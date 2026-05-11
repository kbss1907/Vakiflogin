import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
    LeaveType,
    LeaveRequest,
    CreateLeaveRequestDto,
    ApproveLeaveRequestDto
} from '../models/leave.model';

@Injectable({
    providedIn: 'root'
})
export class LeaveService {
    private apiUrl = '/api/leave';

    constructor(private http: HttpClient) { }

    getLeaveTypes(): Observable<LeaveType[]> {
        return this.http.get<LeaveType[]>(`${this.apiUrl}/types`);
    }

    getMyLeaveRequests(userId: number): Observable<LeaveRequest[]> {
        return this.http.get<LeaveRequest[]>(`${this.apiUrl}/user/${userId}`);
    }

    getPendingLeaveRequests(): Observable<LeaveRequest[]> {
        return this.http.get<LeaveRequest[]>(`${this.apiUrl}/pending`);
    }

    createLeaveRequest(data: CreateLeaveRequestDto): Observable<any> {
        return this.http.post(`${this.apiUrl}/request`, data, { observe: 'response' });
    }

    approveLeaveRequest(data: ApproveLeaveRequestDto): Observable<any> {
        return this.http.post(`${this.apiUrl}/approve`, data);
    }
}
