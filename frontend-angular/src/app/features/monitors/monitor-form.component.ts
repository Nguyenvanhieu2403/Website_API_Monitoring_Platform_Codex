import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MonitorsService } from '../../core/services/monitors.service';
import { Monitor, MonitorType, CreateMonitorRequest, UpdateMonitorRequest } from '../../core/models/monitor.model';

@Component({
  selector: 'app-monitor-form',
  templateUrl: './monitor-form.component.html',
  styleUrls: ['./monitor-form.component.scss']
})
export class MonitorFormComponent implements OnInit {
  monitorForm: FormGroup;
  isEditMode = false;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  monitorId: string = '';
  monitor: Monitor | null = null;

  // Enums for template
  MonitorType = MonitorType;
  monitorTypes = [
    { value: MonitorType.Http, label: 'HTTP' },
    { value: MonitorType.Https, label: 'HTTPS' },
    { value: MonitorType.ApiEndpoint, label: 'API Endpoint' },
    { value: MonitorType.TcpPort, label: 'TCP Port' },
    { value: MonitorType.Ping, label: 'Ping' }
  ];

  httpMethods = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'HEAD', 'OPTIONS'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private monitorsService: MonitorsService
  ) {
    this.monitorForm = this.createForm();
  }

  ngOnInit(): void {
    this.monitorId = this.route.snapshot.paramMap.get('id') || '';
    this.isEditMode = !!this.monitorId && this.monitorId !== 'new';

    if (this.isEditMode) {
      this.loadMonitor();
    }

    // Watch type changes to show/hide relevant fields
    this.monitorForm.get('type')?.valueChanges.subscribe(() => {
      this.updateFieldVisibility();
    });
  }

  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(500)],
      type: [MonitorType.Https, Validators.required],
      target: ['', [Validators.required, Validators.maxLength(500)]],
      port: [null],
      intervalSeconds: [60, [Validators.required, Validators.min(10), Validators.max(86400)]],
      timeoutSeconds: [30, [Validators.required, Validators.min(1), Validators.max(300)]],
      retries: [3, [Validators.min(0), Validators.max(10)]],
      followRedirects: [true],
      expectedStatusCode: [''],
      expectedKeyword: [''],
      httpMethod: ['GET'],
      categoryIds: [[]],
      tagIds: [[]]
    });
  }

  loadMonitor(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.monitorsService.getMonitorById(this.monitorId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.monitor = response.data;
          this.patchFormValues(response.data);
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load monitor';
        this.isLoading = false;
      }
    });
  }

  patchFormValues(monitor: Monitor): void {
    this.monitorForm.patchValue({
      name: monitor.name,
      description: monitor.description,
      type: monitor.type,
      target: monitor.target,
      port: monitor.port,
      intervalSeconds: monitor.intervalSeconds,
      timeoutSeconds: monitor.timeoutSeconds,
      retries: monitor.retries,
      followRedirects: monitor.followRedirects,
      expectedStatusCode: monitor.expectedStatusCode,
      expectedKeyword: monitor.expectedKeyword,
      httpMethod: monitor.httpMethod,
      categoryIds: monitor.categories.map(c => c.categoryId),
      tagIds: monitor.tags.map(t => t.tagId)
    });
    this.updateFieldVisibility();
  }

  updateFieldVisibility(): void {
    const type = this.monitorForm.get('type')?.value;
    const isHttpBased = type === MonitorType.Http || type === MonitorType.Https || type === MonitorType.ApiEndpoint;
    const isTcp = type === MonitorType.TcpPort;

    // HTTP-specific fields
    if (isHttpBased) {
      this.monitorForm.get('httpMethod')?.enable();
      this.monitorForm.get('followRedirects')?.enable();
      this.monitorForm.get('expectedStatusCode')?.enable();
      this.monitorForm.get('expectedKeyword')?.enable();
    } else {
      this.monitorForm.get('httpMethod')?.disable();
      this.monitorForm.get('followRedirects')?.disable();
      this.monitorForm.get('expectedStatusCode')?.disable();
      this.monitorForm.get('expectedKeyword')?.disable();
    }

    // Port field for TCP
    if (isTcp) {
      this.monitorForm.get('port')?.setValidators([Validators.required, Validators.min(1), Validators.max(65535)]);
    } else {
      this.monitorForm.get('port')?.clearValidators();
    }
    this.monitorForm.get('port')?.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.monitorForm.invalid) {
      this.markFormGroupTouched(this.monitorForm);
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formValue = this.monitorForm.getRawValue();

    if (this.isEditMode) {
      this.updateMonitor(formValue);
    } else {
      this.createMonitor(formValue);
    }
  }

  createMonitor(formValue: any): void {
    const request: CreateMonitorRequest = {
      name: formValue.name,
      description: formValue.description,
      type: formValue.type,
      target: formValue.target,
      port: formValue.port || null,
      intervalSeconds: formValue.intervalSeconds,
      timeoutSeconds: formValue.timeoutSeconds,
      retries: formValue.retries || null,
      followRedirects: formValue.followRedirects,
      expectedStatusCode: formValue.expectedStatusCode || null,
      expectedKeyword: formValue.expectedKeyword || null,
      httpMethod: formValue.httpMethod || null,
      categoryIds: formValue.categoryIds || [],
      tagIds: formValue.tagIds || []
    };

    this.monitorsService.createMonitor(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.router.navigate(['/monitors', response.data.monitorId]);
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create monitor';
        this.isSaving = false;
      }
    });
  }

  updateMonitor(formValue: any): void {
    const request: UpdateMonitorRequest = {
      name: formValue.name,
      description: formValue.description,
      type: formValue.type,
      target: formValue.target,
      port: formValue.port || null,
      intervalSeconds: formValue.intervalSeconds,
      timeoutSeconds: formValue.timeoutSeconds,
      retries: formValue.retries || null,
      followRedirects: formValue.followRedirects,
      expectedStatusCode: formValue.expectedStatusCode || null,
      expectedKeyword: formValue.expectedKeyword || null,
      httpMethod: formValue.httpMethod || null,
      categoryIds: formValue.categoryIds || [],
      tagIds: formValue.tagIds || []
    };

    this.monitorsService.updateMonitor(this.monitorId, request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.router.navigate(['/monitors', this.monitorId]);
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update monitor';
        this.isSaving = false;
      }
    });
  }

  cancel(): void {
    if (this.isEditMode) {
      this.router.navigate(['/monitors', this.monitorId]);
    } else {
      this.router.navigate(['/monitors']);
    }
  }

  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.monitorForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.monitorForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) return 'This field is required';
    if (field.errors['maxLength']) return `Maximum length is ${field.errors['maxLength'].requiredLength}`;
    if (field.errors['min']) return `Minimum value is ${field.errors['min'].min}`;
    if (field.errors['max']) return `Maximum value is ${field.errors['max'].max}`;

    return 'Invalid value';
  }

  get isHttpBased(): boolean {
    const type = this.monitorForm.get('type')?.value;
    return type === MonitorType.Http || type === MonitorType.Https || type === MonitorType.ApiEndpoint;
  }

  get isTcpPort(): boolean {
    return this.monitorForm.get('type')?.value === MonitorType.TcpPort;
  }
}
