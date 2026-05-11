import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EmployeeService, UpdateEmployeeDto } from '../../../services/employee.service';
import { Employee } from '../../../models/employee.model';
import { AuthService } from '../../../services/auth';

@Component({
    selector: 'app-employee-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule],
    templateUrl: './detail.component.html',
    styleUrls: ['./detail.component.css']
})
export class DetailComponent implements OnInit {
    employee: Employee | null = null;
    isLoading = true;
    errorMessage = '';

    // Permissions
    adminUserId = 0;
    currentUserProfile: Employee | null = null;
    canEdit = false;
    isAdmin = false;

    // Edit mode
    isEditing = false;
    isSaving = false;
    successMessage = '';
    editForm: UpdateEmployeeDto = {
        firstName: '',
        lastName: '',
        phone: '',
        departmentId: 0,
        positionId: 0,
        status: ''
    };

    // Role Assignment
    isAssigningRole = false;
    isSavingRole = false;
    selectedRoleId: number = 0;

    constructor(
        private route: ActivatedRoute,
        private employeeService: EmployeeService,
        private authService: AuthService,
        private cdr: ChangeDetectorRef,
        @Inject(PLATFORM_ID) private platformId: Object
    ) { }

    ngOnInit(): void {
        if (isPlatformBrowser(this.platformId)) {
            this.adminUserId = this.authService.getUserId() ?? 0;
            const id = Number(this.route.snapshot.paramMap.get('id'));
            if (id) {
                this.loadEmployeeAndPermissions(id);
            }
        } else {
            this.isLoading = false;
        }
    }

    loadEmployeeAndPermissions(employeeId: number): void {
        this.isLoading = true;
        // First load the employee we are viewing
        this.employeeService.getEmployeeById(employeeId).subscribe({
            next: (empData) => {
                this.employee = empData;

                // Now load current user's profile to check permissions
                if (this.adminUserId > 0) {
                    this.employeeService.getEmployeeById(this.adminUserId).subscribe({
                        next: (currUser) => {
                            this.currentUserProfile = currUser;
                            this.evaluatePermissions();
                            this.isLoading = false;
                            this.cdr.detectChanges();
                        },
                        error: () => {
                            this.isLoading = false;
                            this.cdr.detectChanges();
                        }
                    });
                } else {
                    this.isLoading = false;
                    this.cdr.detectChanges();
                }
            },
            error: (err) => {
                console.error('Çalışan bilgisi yüklenirken hata:', err);
                this.errorMessage = 'Çalışan bilgisi yüklenemedi.';
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    evaluatePermissions(): void {
        if (!this.currentUserProfile || !this.employee) {
            this.canEdit = false;
            return;
        }

        const roleId = this.currentUserProfile.roleId || 0;

        // RoleId: 1=Admin, 2=HR_Manager, 3=Employee, 4=Manager, 5=Intern

        // 1. Admin (RoleId=1) herkesi düzenleyebilir + yetki verebilir
        if (roleId === 1) {
            this.canEdit = true;
            this.isAdmin = true;
            return;
        }

        // 2. HR_Manager (RoleId=2) herkesi düzenleyebilir
        if (roleId === 2) {
            this.canEdit = true;
            return;
        }

        // 3. Manager (RoleId=4) sadece kendi departmanındakileri düzenleyebilir
        if (roleId === 4) {
            if (this.currentUserProfile.departmentId === this.employee.departmentId) {
                this.canEdit = true;
                return;
            }
        }

        // Diğer çalışanlar (Employee=3, Intern=5) düzenleyemez
        this.canEdit = false;
    }

    startEditing(): void {
        if (this.employee) {
            this.editForm = {
                firstName: this.employee.firstName,
                lastName: this.employee.lastName,
                phone: this.employee.phone || '',
                departmentId: this.employee.departmentId,
                positionId: this.employee.positionId,
                status: this.employee.status
            };
            this.isEditing = true;
            this.successMessage = '';
            this.errorMessage = '';
        }
    }

    cancelEditing(): void {
        this.isEditing = false;
        this.errorMessage = '';
    }

    saveChanges(): void {
        if (!this.employee) return;

        this.isSaving = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.employeeService.updateEmployee(this.employee.id, this.editForm).subscribe({
            next: () => {
                this.isSaving = false;
                this.isEditing = false;
                this.successMessage = 'Çalışan bilgileri başarıyla güncellendi!';
                // Reload employee and permissions to reflect updated info
                this.loadEmployeeAndPermissions(this.employee!.id);
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Güncelleme hatası:', err);
                this.isSaving = false;
                this.errorMessage = 'Güncelleme sırasında bir hata oluştu.';
                this.cdr.detectChanges();
            }
        });
    }

    startAssigningRole(): void {
        this.isAssigningRole = true;
        this.selectedRoleId = 0;
        this.successMessage = '';
        this.errorMessage = '';
    }

    cancelAssigningRole(): void {
        this.isAssigningRole = false;
        this.errorMessage = '';
    }

    saveRole(): void {
        if (!this.employee || !this.selectedRoleId) return;

        this.isSavingRole = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.employeeService.assignRole(this.employee.id, Number(this.selectedRoleId)).subscribe({
            next: () => {
                this.isSavingRole = false;
                this.isAssigningRole = false;
                this.successMessage = 'Yetki başarıyla atandı!';
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Yetki atama hatası:', err);
                this.isSavingRole = false;
                this.errorMessage = err?.error?.message || 'Yetki atanırken bir hata oluştu.';
                this.cdr.detectChanges();
            }
        });
    }

    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'active': return 'status-active';
            case 'passive': return 'status-passive';
            case 'leave': return 'status-leave';
            default: return 'status-default';
        }
    }

    getStatusLabel(status: string): string {
        switch (status?.toLowerCase()) {
            case 'active': return 'Aktif';
            case 'passive': return 'Pasif';
            case 'leave': return 'İzinli';
            default: return status || 'Belirsiz';
        }
    }
}
