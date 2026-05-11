import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LeaveService } from '../../../services/leave.service';
import { AuthService } from '../../../services/auth';
import { LeaveRequest, LeaveType } from '../../../models/leave.model';
import { FilterByStatusPipe } from '../../../pipes/filter-by-status.pipe';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-my-requests',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FilterByStatusPipe, RouterModule],
    templateUrl: './my-requests.component.html',
    styleUrls: ['./my-requests.component.css']
})
export class MyRequestsComponent implements OnInit {
    myRequests: LeaveRequest[] = [];
    leaveTypes: LeaveType[] = [];

    showNewRequestModal = false;
    leaveForm: FormGroup;
    isSubmitting = false;
    showSuccess = false;
    successMessage = '';

    currentUserId = 0;

    constructor(
        private leaveService: LeaveService,
        private authService: AuthService,
        private fb: FormBuilder,
        private cdr: ChangeDetectorRef,
        @Inject(PLATFORM_ID) private platformId: Object
    ) {
        this.leaveForm = this.fb.group({
            leaveTypeId: ['', Validators.required],
            startDateTime: ['', Validators.required],
            endDateTime: ['', Validators.required],
            reason: ['', Validators.required]
        });
    }

    ngOnInit(): void {
        if (isPlatformBrowser(this.platformId)) {
            this.currentUserId = this.authService.getUserId() ?? 0;
            this.loadMyRequests();
            this.loadLeaveTypes();
        }
    }

    loadMyRequests() {
        this.leaveService.getMyLeaveRequests(this.currentUserId).subscribe({
            next: (data) => {
                this.myRequests = Array.isArray(data) ? data : [];
                this.cdr.detectChanges();
            },
            error: (err) => console.error('İzinler yüklenirken hata:', err)
        });
    }

    loadLeaveTypes() {
        this.leaveService.getLeaveTypes().subscribe({
            next: (data) => {
                this.leaveTypes = Array.isArray(data) ? data : [];
                this.cdr.detectChanges();
            },
            error: (err) => console.error('İzin türleri yüklenirken hata:', err)
        });
    }

    openModal() {
        this.leaveForm.reset();
        this.showNewRequestModal = true;
    }

    closeModal() {
        this.showNewRequestModal = false;
    }

    errorMessage = '';

    onSubmit() {
        if (this.leaveForm.invalid) return;

        const start = new Date(this.leaveForm.value.startDateTime);
        const end = new Date(this.leaveForm.value.endDateTime);

        if (start >= end) {
            this.errorMessage = 'Başlangıç tarihi bitiş tarihinden önce olmalıdır.';
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';
        const requestData = {
            userId: this.currentUserId,
            ...this.leaveForm.value
        };

        this.leaveService.createLeaveRequest(requestData).subscribe({
            next: () => {
                this.isSubmitting = false;
                this.closeModal();
                this.loadMyRequests();
                this.successMessage = '✅ İzin talebiniz başarıyla gönderildi!';
                this.showSuccess = true;
                setTimeout(() => { this.showSuccess = false; }, 3500);
            },
            error: (err) => {
                console.error('İzin oluşturulurken hata:', err);
                this.isSubmitting = false;
                this.errorMessage = err?.error?.message || 'İzin talebi oluşturulurken bir hata oluştu.';
                this.cdr.detectChanges();
            }
        });
    }

    getStatusClass(status: string): string {
        switch (status.toLowerCase()) {
            case 'approved': return 'status-approved';
            case 'rejected': return 'status-rejected';
            case 'pending': return 'status-pending';
            default: return 'status-default';
        }
    }

    getStatusLabel(status: string): string {
        switch (status.toLowerCase()) {
            case 'approved': return 'Onaylandı';
            case 'rejected': return 'Reddedildi';
            case 'pending': return 'Bekliyor';
            default: return status;
        }
    }
}
