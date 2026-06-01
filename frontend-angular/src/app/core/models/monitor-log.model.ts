export interface MonitorLog {
  logId: string;
  monitorId: string;
  isUp: boolean;
  responseTimeMs: number;
  statusCode: number | null;
  errorMessage: string | null;
  responseBody: string | null;
  checkedAt: string;
}

export interface GetMonitorLogsParams {
  monitorId?: string;
  isUp?: boolean;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}
