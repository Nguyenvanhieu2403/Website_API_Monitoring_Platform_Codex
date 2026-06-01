export enum AlertConditionType {
  MonitorDown = 1,
  MonitorUp = 2,
  ResponseTimeThreshold = 3,
  FailureCountThreshold = 4
}

export enum AlertSeverity {
  Info = 1,
  Warning = 2,
  Critical = 3
}

export enum NotificationChannelType {
  Email = 1,
  Webhook = 2
}

export interface NotificationChannel {
  channelId: string;
  name: string;
  type: NotificationChannelType;
  configuration: string;
  isEnabled: boolean;
}

export interface AlertRule {
  ruleId: string;
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue: string | null;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  createdAt: string;
  updatedAt: string | null;
  notificationChannels: NotificationChannel[];
}

export interface AlertEvent {
  eventId: string;
  organizationId: string;
  monitorId: string;
  alertRuleId: string;
  severity: AlertSeverity;
  conditionType: AlertConditionType;
  message: string;
  triggeredAt: string;
  resolvedAt: string | null;
  isResolved: boolean;
  attemptCount: number;
  lastAttemptedAt: string | null;
  isNotificationSent: boolean;
}

export interface CreateAlertRuleRequest {
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue?: string | null;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  channelIds: string[];
}

export interface UpdateAlertRuleRequest {
  ruleId: string;
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue?: string | null;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  channelIds: string[];
}
