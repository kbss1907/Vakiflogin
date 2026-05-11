import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee } from '../models/employee.model';

export interface UpdateEmployeeDto {
    firstName: string;
    lastName: string;
    phone: string;
    departmentId: number;
    positionId: number;
    status: string;
}

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    private apiUrl = '/api/Employee';

    constructor(private http: HttpClient) { }

    getEmployeeList(): Observable<Employee[]> {
        return this.http.get<Employee[]>(`${this.apiUrl}/listDto`);
    }

    getEmployeeById(id: number): Observable<Employee> {
        return this.http.get<Employee>(`${this.apiUrl}/${id}`);
    }

    updateEmployee(id: number, dto: UpdateEmployeeDto): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, dto);
    }

    assignRole(id: number, roleId: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/role`, { roleId: roleId });
    }

    searchEmployee(searchTerm?: string, departmentId?: number, status?: string): Observable<Employee[]> {
        let params = new URLSearchParams();
        if (searchTerm) params.append('searchTerm', searchTerm);
        if (departmentId) params.append('departmentId', departmentId.toString());
        if (status) params.append('status', status);

        const queryString = params.toString();
        const url = queryString ? `${this.apiUrl}/search?${queryString}` : `${this.apiUrl}/search`;
        return this.http.get<Employee[]>(url);
    }
}
