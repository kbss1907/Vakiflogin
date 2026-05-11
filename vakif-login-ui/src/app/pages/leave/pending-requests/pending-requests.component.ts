import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveService } from '../../../services/leave.service';
import { AuthService } from '../../../services/auth';
import { LeaveRequest } from '../../../models/leave.model';
import { Employee } from '../../../models/employee.model';
import { EmployeeService } from '../../../services/employee.service';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-pending-requests',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './pending-requests.component.html',
    styleUrls: ['./pending-requests.component.css']
})
export class PendingRequestsComponent implements OnInit {
    pendingRequests: LeaveRequest[] = [];
    selectedRequest: LeaveRequest | null = null;

    showApprovalModal = false;
    rejectionReason = '';
    isSubmitting = false;

    adminUserId = 0;
    currentUserProfile: Employee | null = null;
    isLoading = true;

    constructor(
        private leaveService: LeaveService,
        private authService: AuthService,
        private employeeService: EmployeeService,
        private cdr: ChangeDetectorRef,
        @Inject(PLATFORM_ID) private platformId: Object
    ) { }

    ngOnInit(): void {
        if (isPlatformBrowser(this.platformId)) {
            this.adminUserId = this.authService.getUserId() ?? 0;
            if (this.adminUserId > 0) {
                this.loadUserProfileAndRequests();
            } else {
                this.isLoading = false;
            }
        }
    }

    loadUserProfileAndRequests() {
        this.employeeService.getEmployeeById(this.adminUserId).subscribe({
            next: (profile) => {
                this.currentUserProfile = profile;
                this.loadPendingRequests();
            },
            error: (err) => {
                console.error('Kullanıcı profili yüklenirken hata:', err);
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    loadPendingRequests() {
        this.leaveService.getPendingLeaveRequests().subscribe({
            next: (data) => {
                const allRequests = Array.isArray(data) ? data : [];
                this.filterRequestsBasedOnRole(allRequests);
                this.isLoading = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Bekleyen talepler yüklenirken hata:', err);
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    filterRequestsBasedOnRole(allRequests: LeaveRequest[]) {
        if (!this.currentUserProfile) {
            this.pendingRequests = [];
            return;
        }

        const roleId = this.currentUserProfile.roleId || 0;
        // RoleId: 1=Admin, 2=HR_Manager, 3=Employee, 4=Manager, 5=Intern

        // 1. Admin (RoleId=1): Tüm talepleri görebilir (Pending + PendingHR)
        if (roleId === 1) {
            this.pendingRequests = allRequests;
            return;
        }

        // 2. HR_Manager (RoleId=2): Sadece 'PendingHR' olanları görür
        if (roleId === 2) {
            this.pendingRequests = allRequests.filter(req => req.status === 'PendingHR');
            return;
        }

        // 3. Manager (RoleId=4): Sadece 'Pending' olanları görebilir
        if (roleId === 4) {
            this.pendingRequests = allRequests.filter(req => req.status === 'Pending');
            return;
        }

        // 4. Employee (3) ve Intern (5): Hiçbir şey görememeli
        this.pendingRequests = [];
    }

    openApprovalModal(request: LeaveRequest) {
        this.selectedRequest = request;
        this.rejectionReason = '';
        this.showApprovalModal = true;
    }

    closeModal() {
        this.showApprovalModal = false;
        this.selectedRequest = null;
        this.rejectionReason = '';
    }

    approveRequest() {
        if (!this.selectedRequest) return;
        this.submitDecision('Approved', 'Talebiniz onaylandı.');
    }

    rejectRequest() {
        if (!this.selectedRequest) return;
        if (!this.rejectionReason.trim()) {
            alert('Lütfen bir ret nedeni giriniz.');
            return;
        }
        this.submitDecision('Rejected', this.rejectionReason);
    }

    private submitDecision(decision: string, comment: string) {
        this.isSubmitting = true;

        const dto = {
            leaveRequestId: this.selectedRequest!.leaveRequestId,
            approverUserId: this.adminUserId,
            decision: decision,
            comment: comment
        };

        this.leaveService.approveLeaveRequest(dto).subscribe({
            next: () => {
                this.isSubmitting = false;
                this.closeModal();
                this.loadPendingRequests();
            },
            error: (err) => {
                console.error('İşlem sırasında hata:', err);
                this.isSubmitting = false;
                alert('İşlem başarısız oldu. Lütfen tekrar deneyin.');
            }
        });
    }
}
