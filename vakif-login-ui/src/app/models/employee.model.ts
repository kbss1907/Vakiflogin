// Represents the data structure returned by the backend's User class
export interface Employee {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    departmentId: number;
    departmentName: string;
    positionId: number;
    positionName: string;
    managerUserId?: number;
    phone: string;
    createdAt: Date;
    isActive: boolean;
    updatedAt?: Date;
    status: string;
    hireDate?: Date;
    roleId?: number;
    roleName?: string;
}
