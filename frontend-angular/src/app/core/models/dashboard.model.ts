import { MonitorStatus, MonitorType } from './monitor.model';

export interface DashboardSummary {
  organizationId: string;
  totalMonitors: number;
  totalChecks: number;
  averageUptimePercentage: number;
  averageResponseTime: number;
  averageFailureRate: number;
}

export interface MonitorOverview {
  monitorId: string;
  monitorName: string;
  uptimePercentage: number;
  averageResponseTime: number;
  minResponseTime: number;
  maxResponseTime: number;
  failureRate: number;
  statusTimeline: MonitorStatusTimeline[];
}

export interface MonitorStatusTimeline {
  timestamp: string;
  status: string;
}

export interface RecentMonitor {
  monitorId: string;
  organizationId: string;
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port: number | null;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries: number | null;
  followRedirects: boolean;
  expectedStatusCode: string | null;
  expectedKeyword: string | null;
  httpMethod: string | null;
  status: MonitorStatus;
  lastCheckedAt: string | null;
  lastDownAt: string | null;
  responseTimeMs: number | null;
  isUp: boolean;
  consecutiveFailures: number;
  uptimePercentage: number;
  createdAt: string;
  updatedAt: string | null;
}

export type TimeRange = '24h' | '7d' | '30d' | '90d';
