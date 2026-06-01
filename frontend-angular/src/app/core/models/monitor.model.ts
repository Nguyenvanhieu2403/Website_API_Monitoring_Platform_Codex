export interface Monitor {
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
  categories: MonitorCategory[];
  tags: MonitorTag[];
}

export interface MonitorCategory {
  categoryId: string;
  name: string;
  color: string;
}

export interface MonitorTag {
  tagId: string;
  name: string;
  color: string;
}

export enum MonitorType {
  Http = 1,
  Https = 2,
  ApiEndpoint = 3,
  TcpPort = 4,
  Ping = 5
}

export enum MonitorStatus {
  Active = 1,
  Paused = 2,
  Down = 3,
  Maintenance = 4
}

export interface CreateMonitorRequest {
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port?: number | null;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries?: number | null;
  followRedirects: boolean;
  expectedStatusCode?: string | null;
  expectedKeyword?: string | null;
  httpMethod?: string | null;
  categoryIds: string[];
  tagIds: string[];
}

export interface UpdateMonitorRequest {
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port?: number | null;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries?: number | null;
  followRedirects: boolean;
  expectedStatusCode?: string | null;
  expectedKeyword?: string | null;
  httpMethod?: string | null;
  categoryIds: string[];
  tagIds: string[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
