import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../../../services/employee.service';
import { Employee } from '../../../models/employee.model';

@Component({
    selector: 'app-employee-list',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule],
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.css']
})
export class ListComponent implements OnInit {
    employees: Employee[] = [];
    isLoading = true;

    // Search filters
    searchTerm: string = '';
    departmentId: number | null = null;
    status: string = '';

    constructor(
        private employeeService: EmployeeService,
        private cdr: ChangeDetectorRef,
        @Inject(PLATFORM_ID) private platformId: Object
    ) { }

    ngOnInit(): void {
        if (isPlatformBrowser(this.platformId)) {
            this.loadEmployees();
        } else {
            // SSR: don't fetch, just mark as not loading
            this.isLoading = false;
        }
    }

    loadEmployees(): void {
        this.isLoading = true;
        this.employeeService.getEmployeeList().subscribe({
            next: (data) => {
                this.employees = Array.isArray(data) ? data : [];
                this.isLoading = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Çalışan listesi yüklenirken hata oluştu:', err);
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    onSearch(): void {
        this.isLoading = true;
        const depId = this.departmentId ? Number(this.departmentId) : undefined;
        const st = this.status ? this.status : undefined;
        const term = this.searchTerm ? this.searchTerm : undefined;

        this.employeeService.searchEmployee(term, depId, st).subscribe({
            next: (data) => {
                this.employees = Array.isArray(data) ? data : [];
                this.isLoading = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Arama sırasında hata oluştu:', err);
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    resetSearch(): void {
        this.searchTerm = '';
        this.departmentId = null;
        this.status = '';
        this.loadEmployees();
    }

    getStatusClass(status: string): string {
        if (!status) return 'status-default';
        switch (status.toLowerCase()) {
            case 'active':
            case 'aktif': return 'status-active';
            case 'passive':
            case 'pasif': return 'status-passive';
            case 'onleave':
            case 'leave':
            case 'izinde': return 'status-leave';
            default: return 'status-default';
        }
    }

    getStatusLabel(status: string): string {
        if (!status) return 'Bilinmiyor';
        switch (status.toLowerCase()) {
            case 'active': return 'Aktif';
            case 'passive': return 'Pasif';
            case 'leave':
            case 'onleave': return 'İzinde';
            default: return status;
        }
    }
}
