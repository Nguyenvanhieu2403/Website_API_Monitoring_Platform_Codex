import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../core/services/users.service';
import { User, UpdateProfileRequest, ChangePasswordRequest } from '../../core/models/user.model';

@Component({
  selector: 'app-profile-settings',
  templateUrl: './profile-settings.component.html',
  styleUrls: ['./profile-settings.component.scss']
})
export class ProfileSettingsComponent implements OnInit {
  currentUser: User | null = null;
  isLoading = true;
  isSavingProfile = false;
  isSavingPassword = false;
  errorMessage = '';
  successMessage = '';

  // Profile form
  email = '';
  firstName = '';
  lastName = '';

  // Password form
  currentPassword = '';
  newPassword = '';
  confirmNewPassword = '';

  // Validation
  emailError = '';
  passwordError = '';
  confirmPasswordError = '';

  constructor(private usersService: UsersService) {}

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  loadCurrentUser(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getCurrentUser().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.currentUser = response.data;
          this.email = this.currentUser.email;
          this.firstName = this.currentUser.firstName || '';
          this.lastName = this.currentUser.lastName || '';
        } else {
          this.errorMessage = response.message || 'Failed to load profile';
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred while loading profile';
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

    if (!this.currentPassword) {
      this.passwordError = 'Current password is required';
      return false;
    }

    if (!this.newPassword) {
      this.passwordError = 'New password is required';
      return false;
    }

    if (this.newPassword.length < 8) {
      this.passwordError = 'Password must be at least 8 characters';
      return false;
    }

    if (this.newPassword !== this.confirmNewPassword) {
      this.confirmPasswordError = 'Passwords do not match';
      return false;
    }

    return true;
  }

  updateProfile(): void {
    if (!this.validateEmail()) {
      return;
    }

    this.isSavingProfile = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: UpdateProfileRequest = {
      firstName: this.firstName || undefined,
      lastName: this.lastName || undefined,
      email: this.email
    };

    this.usersService.updateProfile(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Profile updated successfully';
          this.loadCurrentUser();
        } else {
          this.errorMessage = response.message || 'Failed to update profile';
        }
        this.isSavingProfile = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred while updating profile';
        this.isSavingProfile = false;
      }
    });
  }

  changePassword(): void {
    if (!this.validatePassword()) {
      return;
    }

    this.isSavingPassword = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: ChangePasswordRequest = {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    };

    this.usersService.changePassword(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Password changed successfully';
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmNewPassword = '';
        } else {
          this.errorMessage = response.message || 'Failed to change password';
        }
        this.isSavingPassword = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred while changing password';
        this.isSavingPassword = false;
      }
    });
  }

  getRoleLabel(role: number): string {
    switch (role) {
      case 1: return 'Owner';
      case 2: return 'Admin';
      case 3: return 'Member';
      case 4: return 'Viewer';
      default: return 'Unknown';
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Active';
      case 2: return 'Inactive';
      case 3: return 'Suspended';
      default: return 'Unknown';
    }
  }
}
