import { Pipe, PipeTransform } from '@angular/core';
import { LeaveRequest } from '../models/leave.model';

@Pipe({
    name: 'filterByStatus',
    standalone: true
})
export class FilterByStatusPipe implements PipeTransform {
    transform(requests: LeaveRequest[] | null | undefined, status: string): LeaveRequest[] {
        if (!requests) return [];
        return requests.filter(req => req.status?.toLowerCase() === status.toLowerCase());
    }
}
