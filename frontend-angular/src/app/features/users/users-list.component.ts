import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UsersService } from '../../core/services/users.service';
import { User, UserRole, UserStatus } from '../../core/models/user.model';

@Component({
  selector: 'app-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit {
  users: User[] = [];
  isLoading = true;
  errorMessage = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Filters
  selectedRole: UserRole | undefined;
  selectedStatus: UserStatus | undefined;
  searchTerm = '';
  organizationId = '';

  // Enums for template
  UserRole = UserRole;
  UserStatus = UserStatus;

  constructor(
    private usersService: UsersService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getUsers({
      organizationId: this.organizationId || undefined,
      role: this.selectedRole,
      status: this.selectedStatus,
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'CreatedAt',
      sortDescending: true
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users = response.data.items as User[];
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load users';
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred while loading users';
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  clearFilters(): void {
    this.selectedRole = undefined;
    this.selectedStatus = undefined;
    this.searchTerm = '';
    this.organizationId = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadUsers();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadUsers();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadUsers();
    }
  }

  createUser(): void {
    this.router.navigate(['/users/create']);
  }

  editUser(userId: string): void {
    this.router.navigate(['/users/edit', userId]);
  }

  deleteUser(userId: string, email: string): void {
    if (confirm(`Are you sure you want to delete user "${email}"?`)) {
      this.usersService.deleteUser(userId).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadUsers();
          } else {
            alert(response.message || 'Failed to delete user');
          }
        },
        error: (error) => {
          alert(error.message || 'An error occurred while deleting user');
        }
      });
    }
  }

  getRoleLabel(role: UserRole): string {
    switch (role) {
      case UserRole.Owner: return 'Owner';
      case UserRole.Admin: return 'Admin';
      case UserRole.Member: return 'Member';
      case UserRole.Viewer: return 'Viewer';
      default: return 'Unknown';
    }
  }

  getRoleColor(role: UserRole): 'purple' | 'blue' | 'green' | 'gray' {
    switch (role) {
      case UserRole.Owner: return 'purple';
      case UserRole.Admin: return 'blue';
      case UserRole.Member: return 'green';
      case UserRole.Viewer: return 'gray';
      default: return 'gray';
    }
  }

  getStatusLabel(status: UserStatus): string {
    switch (status) {
      case UserStatus.Active: return 'Active';
      case UserStatus.Inactive: return 'Inactive';
      case UserStatus.Suspended: return 'Suspended';
      default: return 'Unknown';
    }
  }

  getStatusColor(status: UserStatus): 'green' | 'gray' | 'red' {
    switch (status) {
      case UserStatus.Active: return 'green';
      case UserStatus.Inactive: return 'gray';
      case UserStatus.Suspended: return 'red';
      default: return 'gray';
    }
  }

  getFullName(user: User): string {
    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    return user.firstName || user.lastName || 'N/A';
  }

  getStartRecord(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getEndRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }
}
