export interface NotificationChannel {
  channelId: string;
  organizationId: string;
  name: string;
  type: NotificationChannelType;
  configuration: EmailConfiguration | WebhookConfiguration;
  isEnabled: boolean;
  createdAt: string;
  updatedAt: string;
}

export enum NotificationChannelType {
  Email = 1,
  Webhook = 2
}

export interface EmailConfiguration {
  recipients: string[];
  subject?: string;
}

export interface WebhookConfiguration {
  url: string;
  method: 'GET' | 'POST' | 'PUT';
  headers?: Record<string, string>;
  body?: string;
}

export interface CreateNotificationChannelRequest {
  name: string;
  type: NotificationChannelType;
  configuration: EmailConfiguration | WebhookConfiguration;
  isEnabled: boolean;
}

export interface UpdateNotificationChannelRequest extends CreateNotificationChannelRequest {
  channelId: string;
}
