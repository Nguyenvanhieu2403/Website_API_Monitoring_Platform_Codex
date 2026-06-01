import { Component, OnInit } from '@angular/core';

interface SystemConfig {
  siteName: string;
  siteUrl: string;
  supportEmail: string;
  maintenanceMode: boolean;
  allowRegistration: boolean;
  emailNotifications: boolean;
  slackNotifications: boolean;
  webhookNotifications: boolean;
  maxMonitorsPerOrg: number;
  defaultCheckInterval: number;
  retentionDays: number;
}

@Component({
  selector: 'app-system-settings',
  templateUrl: './system-settings.component.html',
  styleUrls: ['./system-settings.component.scss']
})
export class SystemSettingsComponent implements OnInit {
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  config: SystemConfig = {
    siteName: 'Monitoring Platform',
    siteUrl: 'https://monitoring.example.com',
    supportEmail: 'support@example.com',
    maintenanceMode: false,
    allowRegistration: true,
    emailNotifications: true,
    slackNotifications: false,
    webhookNotifications: true,
    maxMonitorsPerOrg: 100,
    defaultCheckInterval: 60,
    retentionDays: 90
  };

  constructor() {}

  ngOnInit(): void {
    this.loadSystemConfig();
  }

  loadSystemConfig(): void {
    // In a real implementation, this would fetch from the backend
    // For now, we'll use the default config
    this.isLoading = false;
  }

  saveSystemConfig(): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Simulate API call
    setTimeout(() => {
      this.successMessage = 'System configuration saved successfully';
      this.isSaving = false;
    }, 1000);

    // In a real implementation:
    // this.settingsService.updateSystemConfig(this.config).subscribe({
    //   next: (response) => {
    //     if (response.success) {
    //       this.successMessage = 'System configuration saved successfully';
    //     } else {
    //       this.errorMessage = response.message || 'Failed to save configuration';
    //     }
    //     this.isSaving = false;
    //   },
    //   error: (error) => {
    //     this.errorMessage = error.message || 'An error occurred while saving configuration';
    //     this.isSaving = false;
    //   }
    // });
  }

  resetToDefaults(): void {
    if (confirm('Are you sure you want to reset all settings to default values?')) {
      this.config = {
        siteName: 'Monitoring Platform',
        siteUrl: 'https://monitoring.example.com',
        supportEmail: 'support@example.com',
        maintenanceMode: false,
        allowRegistration: true,
        emailNotifications: true,
        slackNotifications: false,
        webhookNotifications: true,
        maxMonitorsPerOrg: 100,
        defaultCheckInterval: 60,
        retentionDays: 90
      };
      this.successMessage = 'Settings reset to defaults';
    }
  }
}
