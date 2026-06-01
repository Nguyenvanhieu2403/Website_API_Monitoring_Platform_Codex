import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AlertsService } from '../../core/services/alerts.service';
import { AlertRule, AlertConditionType, AlertSeverity } from '../../core/models/alert-rule.model';

@Component({
  selector: 'app-alerts-list',
  templateUrl: './alerts-list.component.html',
  styleUrls: ['./alerts-list.component.scss']
})
export class AlertsListComponent implements OnInit {
  alertRules: AlertRule[] = [];
  isLoading = true;
  errorMessage = '';

  organizationId = '';
  monitorId = '';

  AlertConditionType = AlertConditionType;
  AlertSeverity = AlertSeverity;

  constructor(
    private alertsService: AlertsService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.organizationId = params['organizationId'] || '';
      this.monitorId = params['monitorId'] || '';

      if (this.organizationId && this.monitorId) {
        this.loadAlertRules();
      } else {
        this.errorMessage = 'Organization ID and Monitor ID are required';
        this.isLoading = false;
      }
    });
  }

  loadAlertRules(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.alertsService.getAlertRules(this.organizationId, this.monitorId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.alertRules = response.data;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load alert rules';
        this.isLoading = false;
      }
    });
  }

  createAlertRule(): void {
    this.router.navigate(['/alerts/new'], {
      queryParams: {
        organizationId: this.organizationId,
        monitorId: this.monitorId
      }
    });
  }

  viewDetails(ruleId: string): void {
    this.router.navigate(['/alerts', ruleId], {
      queryParams: { organizationId: this.organizationId }
    });
  }

  editAlertRule(ruleId: string): void {
    this.router.navigate(['/alerts', ruleId, 'edit'], {
      queryParams: {
        organizationId: this.organizationId,
        monitorId: this.monitorId
      }
    });
  }

  deleteAlertRule(rule: AlertRule): void {
    if (!confirm(`Are you sure you want to delete "${rule.name}"?`)) {
      return;
    }

    this.alertsService.deleteAlertRule(this.organizationId, rule.ruleId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadAlertRules();
        }
      },
      error: (error) => {
        alert(error.error?.message || 'Failed to delete alert rule');
      }
    });
  }

  getConditionTypeLabel(type: AlertConditionType): string {
    switch (type) {
      case AlertConditionType.MonitorDown: return 'Monitor Down';
      case AlertConditionType.MonitorUp: return 'Monitor Up';
      case AlertConditionType.ResponseTimeThreshold: return 'Response Time';
      case AlertConditionType.FailureCountThreshold: return 'Failure Count';
      default: return 'Unknown';
    }
  }

  getSeverityLabel(severity: AlertSeverity): string {
    switch (severity) {
      case AlertSeverity.Info: return 'Info';
      case AlertSeverity.Warning: return 'Warning';
      case AlertSeverity.Critical: return 'Critical';
      default: return 'Unknown';
    }
  }

  getSeverityColor(severity: AlertSeverity): 'blue' | 'yellow' | 'red' {
    switch (severity) {
      case AlertSeverity.Info: return 'blue';
      case AlertSeverity.Warning: return 'yellow';
      case AlertSeverity.Critical: return 'red';
      default: return 'blue';
    }
  }

  getStatusColor(isEnabled: boolean): 'green' | 'gray' {
    return isEnabled ? 'green' : 'gray';
  }

  getStatusText(isEnabled: boolean): string {
    return isEnabled ? 'Enabled' : 'Disabled';
  }
}
