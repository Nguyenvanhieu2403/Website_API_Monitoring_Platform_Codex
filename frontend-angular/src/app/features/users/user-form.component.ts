import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../../core/services/users.service';
import { User, UserRole, UserStatus, CreateUserRequest, UpdateUserRequest } from '../../core/models/user.model';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit {
  isEditMode = false;
  userId: string | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';

  // Form fields
  email = '';
  password = '';
  confirmPassword = '';
  firstName = '';
  lastName = '';
  selectedRole: UserRole = UserRole.Member;
  selectedStatus: UserStatus = UserStatus.Active;
  organizationId = '';

  // Enums for template
  UserRole = UserRole;
  UserStatus = UserStatus;

  // Validation
  emailError = '';
  passwordError = '';
  confirmPasswordError = '';

  constructor(
    private usersService: UsersService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.userId;

    if (this.isEditMode && this.userId) {
      this.loadUser(this.userId);
    }
  }

  loadUser(userId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getUserById(userId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          this.email = user.email;
          this.firstName = user.firstName || '';
          this.lastName = user.lastName || '';
          this.selectedRole = user.role;
          this.selectedStatus = user.status;
          this.organizationId = user.organizationId;
        } else {
          this.errorMessage = response.message || 'Failed to load user';
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred while loading user';
        this.isLoading = false;
      }
    });
  }

  validateEmail(): boolean {
    this.emailError = '';
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!this.email) {
      this.emailError = 'Email is required';
      return false;
    }

    if (!emailRegex.test(this.email)) {
      this.emailError = 'Invalid email format';
      return false;
    }

    return true;
  }

  validatePassword(): boolean {
    this.passwordError = '';
    this.confirmPasswordError = '';

    if (!this.isEditMode) {
      if (!this.password) {
        this.passwordError = 'Password is required';
        return false;
      }

      if (this.password.length < 8) {
        this.passwordError = 'Password must be at least 8 characters';
        return false;
      }

      if (this.password !== this.confirmPassword) {
        this.confirmPasswordError = 'Passwords do not match';
        return false;
      }
    }

    return true;
  }

  validateForm(): boolean {
    const isEmailValid = this.validateEmail();
    const isPasswordValid = this.validatePassword();
    return isEmailValid && isPasswordValid;
  }

  saveUser(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.userId) {
      const request: UpdateUserRequest = {
        firstName: this.firstName || undefined,
        lastName: this.lastName || undefined,
        role: this.selectedRole,
        status: this.selectedStatus
      };

      this.usersService.updateUser(this.userId, request).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/users']);
          } else {
            this.errorMessage = response.message || 'Failed to update user';
            this.isSaving = false;
          }
        },
        error: (error) => {
          this.errorMessage = error.message || 'An error occurred while updating user';
          this.isSaving = false;
        }
      });
    } else {
      const request: CreateUserRequest = {
        email: this.email,
        password: this.password,
        firstName: this.firstName,
        lastName: this.lastName,
        role: this.selectedRole,
        organizationId: this.organizationId || '00000000-0000-0000-0000-000000000000'
      };

      this.usersService.createUser(request).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/users']);
          } else {
            this.errorMessage = response.message || 'Failed to create user';
            this.isSaving = false;
          }
        },
        error: (error) => {
          this.errorMessage = error.message || 'An error occurred while creating user';
          this.isSaving = false;
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/users']);
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

  getStatusLabel(status: UserStatus): string {
    switch (status) {
      case UserStatus.Active: return 'Active';
      case UserStatus.Inactive: return 'Inactive';
      case UserStatus.Suspended: return 'Suspended';
      default: return 'Unknown';
    }
  }
}
