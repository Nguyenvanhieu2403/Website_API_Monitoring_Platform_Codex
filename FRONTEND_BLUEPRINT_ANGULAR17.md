# Frontend Blueprint for Angular 17

## Website & API Monitoring Platform

**Version:** 1.0.0  
**Date:** 2026-06-01  
**Backend Repository:** https://github.com/Nguyenvanhieu2403/Website_API_Monitoring_Platform_Codex

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Backend Architecture Analysis](#backend-architecture-analysis)
3. [API Endpoints Mapping](#api-endpoints-mapping)
4. [Frontend Module Structure](#frontend-module-structure)
5. [Data Models (TypeScript Interfaces)](#data-models-typescript-interfaces)
6. [Authentication & Authorization Flow](#authentication--authorization-flow)
7. [Role-Based Access Control (RBAC)](#role-based-access-control-rbac)
8. [Suggested Angular 17 Architecture](#suggested-angular-17-architecture)
9. [State Management Strategy](#state-management-strategy)
10. [Routing Structure](#routing-structure)
11. [API Service Layer](#api-service-layer)
12. [UI/UX Considerations](#uiux-considerations)

---

## 1. Executive Summary

This document provides a complete architectural blueprint for building an Angular 17 frontend application that consumes the .NET 9 backend API for the Website & API Monitoring Platform.

### Key Findings

- **Backend Pattern:** Clean Architecture with CQRS (MediatR)
- **Authentication:** JWT + Refresh Token mechanism
- **Multi-tenancy:** Organization-based isolation
- **API Version:** v1
- **Total Controllers:** 5 main controllers
- **Total Feature Modules:** 7 backend features
- **Suggested Frontend Modules:** 8 feature modules

---

## 2. Backend Architecture Analysis

### 2.1 Architecture Pattern

```
┌─────────────────────────────────────────────────────────┐
│                    BACKEND LAYERS                       │
├─────────────────────────────────────────────────────────┤
│  API Layer (Controllers)                                │
│    ├─ AuthController                                    │
│    ├─ MonitorsController                                │
│    ├─ AlertRulesController                              │
│    ├─ NotificationChannelsController                    │
│    └─ DashboardController                               │
│                                                          │
│  Application Layer (CQRS)                               │
│    ├─ Commands (Create, Update, Delete)                 │
│    ├─ Queries (Get, List)                               │
│    └─ Handlers (MediatR)                                │
│                                                          │
│  Domain Layer                                           │
│    ├─ Entities (Monitor, AlertRule, User, etc.)        │
│    ├─ Enums (MonitorType, UserRole, etc.)              │
│    └─ Value Objects                                     │
│                                                          │
│  Infrastructure Layer                                   │
│    ├─ Data Access (EF Core + PostgreSQL)               │
│    ├─ External Services (Email, Webhook)                │
│    └─ Security (JWT, Password Hashing)                  │
└─────────────────────────────────────────────────────────┘
```

### 2.2 CQRS Pattern

The backend uses **MediatR** for CQRS implementation:

- **Commands:** Write operations (Create, Update, Delete)
- **Queries:** Read operations (Get, List, Search)
- **Handlers:** Process commands/queries and return results

**Frontend Implication:** All API calls return a standardized `ApiResponse<T>` wrapper.

### 2.3 Controllers Summary

| Controller                     | Base Route                                                    | Features                             | Auth Required |
| ------------------------------ | ------------------------------------------------------------- | ------------------------------------ | ------------- |
| AuthController                 | `/api/v1/auth`                                                | Register, Login, Refresh, Logout, Me | Partial       |
| MonitorsController             | `/api/v1/monitors`                                            | CRUD monitors, List with filters     | Yes           |
| AlertRulesController           | `/api/organizations/{orgId}/monitors/{monitorId}/alert-rules` | CRUD alert rules                     | Yes           |
| NotificationChannelsController | `/api/organizations/{orgId}/notification-channels`            | CRUD notification channels           | Yes           |
| DashboardController            | `/api/v1/dashboard`                                           | Get summary analytics                | Yes           |

---

## 3. API Endpoints Mapping

### 3.1 Authentication Module

**Base URL:** `/api/v1/auth`

| Method | Endpoint    | Description              | Request Body          | Response           |
| ------ | ----------- | ------------------------ | --------------------- | ------------------ |
| POST   | `/register` | Register new user        | `RegisterRequest`     | `RegisterResponse` |
| POST   | `/login`    | User login               | `LoginRequest`        | `LoginResponse`    |
| POST   | `/refresh`  | Refresh access token     | `RefreshTokenRequest` | `AuthTokens`       |
| POST   | `/logout`   | Logout user              | -                     | Success message    |
| GET    | `/me`       | Get current user profile | -                     | User profile       |

### 3.2 Monitors Module

**Base URL:** `/api/v1/monitors`

| Method | Endpoint | Description                  | Request Body           | Response                    |
| ------ | -------- | ---------------------------- | ---------------------- | --------------------------- |
| GET    | `/`      | Get paginated monitors list  | Query params           | `PagedResponse<MonitorDto>` |
| GET    | `/{id}`  | Get monitor by ID            | -                      | `MonitorDto`                |
| POST   | `/`      | Create new monitor           | `CreateMonitorRequest` | `MonitorDto`                |
| PUT    | `/{id}`  | Update monitor               | `UpdateMonitorRequest` | `MonitorDto`                |
| DELETE | `/{id}`  | Delete monitor (soft delete) | -                      | Success message             |

**Query Parameters for List:**

- `search` (string, optional)
- `type` (MonitorType enum, optional)
- `status` (MonitorStatus enum, optional)
- `categoryId` (Guid, optional)
- `tagId` (Guid, optional)
- `isUp` (boolean, optional)
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10)
- `sortBy` (string, default: "CreatedAt")
- `sortDescending` (boolean, default: true)

### 3.3 Alert Rules Module

**Base URL:** `/api/organizations/{organizationId}/monitors/{monitorId}/alert-rules`

| Method | Endpoint    | Description                 | Request Body             | Response         |
| ------ | ----------- | --------------------------- | ------------------------ | ---------------- |
| GET    | `/`         | Get alert rules for monitor | -                        | `AlertRuleDto[]` |
| GET    | `/{ruleId}` | Get alert rule by ID        | -                        | `AlertRuleDto`   |
| POST   | `/`         | Create alert rule           | `CreateAlertRuleCommand` | `AlertRuleDto`   |
| PUT    | `/{ruleId}` | Update alert rule           | `UpdateAlertRuleCommand` | `AlertRuleDto`   |
| DELETE | `/{ruleId}` | Delete alert rule           | -                        | Success message  |

### 3.4 Notification Channels Module

**Base URL:** `/api/organizations/{organizationId}/notification-channels`

| Method | Endpoint       | Description                 | Request Body                       | Response                   |
| ------ | -------------- | --------------------------- | ---------------------------------- | -------------------------- |
| GET    | `/`            | Get notification channels   | -                                  | `NotificationChannelDto[]` |
| GET    | `/{channelId}` | Get channel by ID           | -                                  | `NotificationChannelDto`   |
| POST   | `/`            | Create notification channel | `CreateNotificationChannelCommand` | `NotificationChannelDto`   |
| PUT    | `/{channelId}` | Update notification channel | `UpdateNotificationChannelCommand` | `NotificationChannelDto`   |
| DELETE | `/{channelId}` | Delete notification channel | -                                  | Success message            |

### 3.5 Dashboard Module

**Base URL:** `/api/v1/dashboard`

| Method | Endpoint   | Description              | Query Params                 | Response                 |
| ------ | ---------- | ------------------------ | ---------------------------- | ------------------------ |
| GET    | `/summary` | Get aggregated analytics | `timeRange` (default: "24h") | `AggregatedAnalyticsDto` |

---

## 4. Frontend Module Structure

Based on backend features, the Angular 17 application should have the following feature modules:

```
src/app/
├── core/                          # Singleton services, guards, interceptors
│   ├── auth/
│   │   ├── guards/
│   │   │   ├── auth.guard.ts
│   │   │   └── role.guard.ts
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts
│   │   │   ├── error.interceptor.ts
│   │   │   └── loading.interceptor.ts
│   │   └── services/
│   │       ├── auth.service.ts
│   │       ├── token.service.ts
│   │       └── user.service.ts
│   ├── services/
│   │   ├── api.service.ts
│   │   ├── notification.service.ts
│   │   └── storage.service.ts
│   └── models/
│       ├── api-response.model.ts
│       └── paged-response.model.ts
│
├── shared/                        # Shared components, directives, pipes
│   ├── components/
│   │   ├── page-header/
│   │   ├── data-table/
│   │   ├── status-badge/
│   │   ├── loading-spinner/
│   │   └── confirmation-dialog/
│   ├── directives/
│   ├── pipes/
│   │   ├── time-ago.pipe.ts
│   │   ├── uptime-percentage.pipe.ts
│   │   └── response-time.pipe.ts
│   └── models/
│       └── enums.ts
│
├── features/                      # Feature modules (lazy-loaded)
│   ├── auth/
│   │   ├── pages/
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   └── forgot-password/
│   │   ├── auth-routing.module.ts
│   │   └── auth.module.ts
│   │
│   ├── dashboard/
│   │   ├── pages/
│   │   │   └── dashboard-overview/
│   │   ├── components/
│   │   │   ├── stats-card/
│   │   │   ├── uptime-chart/
│   │   │   └── recent-incidents/
│   │   ├── services/
│   │   │   └── dashboard.service.ts
│   │   ├── models/
│   │   │   └── dashboard.model.ts
│   │   ├── dashboard-routing.module.ts
│   │   └── dashboard.module.ts
│   │
│   ├── monitors/
│   │   ├── pages/
│   │   │   ├── monitor-list/
│   │   │   ├── monitor-detail/
│   │   │   ├── monitor-create/
│   │   │   └── monitor-edit/
│   │   ├── components/
│   │   │   ├── monitor-form/
│   │   │   ├── monitor-card/
│   │   │   ├── monitor-status/
│   │   │   └── monitor-filters/
│   │   ├── services/
│   │   │   └── monitor.service.ts
│   │   ├── models/
│   │   │   └── monitor.model.ts
│   │   ├── monitors-routing.module.ts
│   │   └── monitors.module.ts
│   │
│   ├── alert-rules/
│   │   ├── pages/
│   │   │   ├── alert-rule-list/
│   │   │   └── alert-rule-form/
│   │   ├── components/
│   │   │   ├── alert-rule-card/
│   │   │   └── condition-builder/
│   │   ├── services/
│   │   │   └── alert-rule.service.ts
│   │   ├── models/
│   │   │   └── alert-rule.model.ts
│   │   ├── alert-rules-routing.module.ts
│   │   └── alert-rules.module.ts
│   │
│   ├── notification-channels/
│   │   ├── pages/
│   │   │   ├── channel-list/
│   │   │   └── channel-form/
│   │   ├── components/
│   │   │   ├── channel-card/
│   │   │   └── channel-config/
│   │   ├── services/
│   │   │   └── notification-channel.service.ts
│   │   ├── models/
│   │   │   └── notification-channel.model.ts
│   │   ├── notification-channels-routing.module.ts
│   │   └── notification-channels.module.ts
│   │
│   └── settings/
│       ├── pages/
│       │   ├── profile/
│       │   ├── organization/
│       │   └── team-members/
│       ├── settings-routing.module.ts
│       └── settings.module.ts
│
├── layout/                        # Layout components
│   ├── main-layout/
│   │   ├── header/
│   │   ├── sidebar/
│   │   └── footer/
│   └── auth-layout/
│
└── app-routing.module.ts
```

---

## 5. Data Models (TypeScript Interfaces)

### 5.1 Core API Response Models

```typescript
// src/app/core/models/api-response.model.ts
export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data?: T;
  traceId: string;
  timestamp: Date;
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

export interface Result<T> {
  isSuccess: boolean;
  value?: T;
  error?: string;
}
```

### 5.2 Authentication Models

```typescript
// src/app/features/auth/models/auth.model.ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName?: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  organizationId: string;
  organizationName: string;
  role: UserRole;
  tokens: AuthTokens;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  organizationId: string;
  organizationName: string;
  role: UserRole;
  tokens: AuthTokens;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: Date;
  refreshTokenExpiresAt: Date;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}
```

### 5.3 Monitor Models

```typescript
// src/app/features/monitors/models/monitor.model.ts
export interface MonitorDto {
  monitorId: string;
  organizationId: string;
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port?: number;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries?: number;
  followRedirects: boolean;
  expectedStatusCode?: string;
  expectedKeyword?: string;
  httpMethod?: string;
  status: MonitorStatus;
  lastCheckedAt?: Date;
  lastDownAt?: Date;
  responseTimeMs?: number;
  isUp: boolean;
  consecutiveFailures: number;
  uptimePercentage: number;
  createdAt: Date;
  updatedAt?: Date;
  categories: MonitorCategoryDto[];
  tags: MonitorTagDto[];
}

export interface CreateMonitorRequest {
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port?: number;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries?: number;
  followRedirects: boolean;
  expectedStatusCode?: string;
  expectedKeyword?: string;
  httpMethod?: string;
  categoryIds: string[];
  tagIds: string[];
}

export interface UpdateMonitorRequest {
  name: string;
  description: string;
  type: MonitorType;
  target: string;
  port?: number;
  intervalSeconds: number;
  timeoutSeconds: number;
  retries?: number;
  followRedirects: boolean;
  expectedStatusCode?: string;
  expectedKeyword?: string;
  httpMethod?: string;
  categoryIds: string[];
  tagIds: string[];
}

export interface MonitorCategoryDto {
  categoryId: string;
  name: string;
  color: string;
}

export interface MonitorTagDto {
  tagId: string;
  name: string;
  color: string;
}
```

### 5.4 Alert Rule Models

```typescript
// src/app/features/alert-rules/models/alert-rule.model.ts
export interface AlertRuleDto {
  ruleId: string;
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue?: string;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  createdAt: Date;
  updatedAt?: Date;
  notificationChannels: NotificationChannelDto[];
}

export interface CreateAlertRuleCommand {
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue?: string;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  notificationChannelIds: string[];
}

export interface UpdateAlertRuleCommand {
  ruleId: string;
  organizationId: string;
  monitorId: string;
  name: string;
  conditionType: AlertConditionType;
  thresholdValue?: string;
  severity: AlertSeverity;
  isEnabled: boolean;
  cooldownMinutes: number;
  notificationChannelIds: string[];
}
```

### 5.5 Notification Channel Models

```typescript
// src/app/features/notification-channels/models/notification-channel.model.ts
export interface NotificationChannelDto {
  channelId: string;
  organizationId: string;
  name: string;
  type: NotificationChannelType;
  configuration: string;
  isEnabled: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateNotificationChannelCommand {
  organizationId: string;
  name: string;
  type: NotificationChannelType;
  configuration: string;
  isEnabled: boolean;
}

export interface UpdateNotificationChannelCommand {
  channelId: string;
  organizationId: string;
  name: string;
  type: NotificationChannelType;
  configuration: string;
  isEnabled: boolean;
}
```

### 5.6 Dashboard Models

```typescript
// src/app/features/dashboard/models/dashboard.model.ts
export interface AggregatedAnalyticsDto {
  organizationId: string;
  totalMonitors: number;
  totalChecks: number;
  averageUptimePercentage: number;
  averageResponseTime: number;
  averageFailureRate: number;
}

export interface MonitorOverviewDto {
  monitorId: string;
  monitorName: string;
  uptimePercentage: number;
  averageResponseTime: number;
  minResponseTime: number;
  maxResponseTime: number;
  failureRate: number;
  statusTimeline: MonitorStatusTimelineDto[];
}

export interface MonitorStatusTimelineDto {
  timestamp: Date;
  status: string;
}
```

### 5.7 Enums

```typescript
// src/app/shared/models/enums.ts
export enum MonitorType {
  Http = 1,
  Https = 2,
  ApiEndpoint = 3,
  TcpPort = 4,
  Ping = 5,
}

export enum MonitorStatus {
  Active = 1,
  Paused = 2,
  Down = 3,
  Maintenance = 4,
}

export enum UserRole {
  Owner = 1,
  Admin = 2,
  Member = 3,
  Viewer = 4,
}

export enum NotificationChannelType {
  Email = 1,
  Webhook = 2,
}

export enum AlertSeverity {
  Info = 1,
  Warning = 2,
  Critical = 3,
}

export enum AlertConditionType {
  MonitorDown = 1,
  MonitorUp = 2,
  ResponseTimeThreshold = 3,
  FailureCountThreshold = 4,
}

export enum OrganizationStatus {
  Active = 1,
  Suspended = 2,
  Deleted = 3,
}

export enum PlanType {
  Starter = 1,
  Professional = 2,
  Enterprise = 3,
}

export enum UserStatus {
  Active = 1,
  Inactive = 2,
  Suspended = 3,
}
```

---

## 6. Authentication & Authorization Flow

### 6.1 Authentication Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   AUTHENTICATION FLOW                       │
└─────────────────────────────────────────────────────────────┘

1. USER LOGIN
   ┌──────────┐         POST /api/v1/auth/login          ┌──────────┐
   │  Angular │  ────────────────────────────────────────> │  Backend │
   │  Client  │  { email, password }                      │   API    │
   │          │                                            │          │
   │          │  <────────────────────────────────────────│          │
   │          │  { userId, email, role, tokens }          │          │
   └──────────┘                                            └──────────┘
        │
        ├─ Store accessToken in memory (service)
        ├─ Store refreshToken in localStorage/sessionStorage
        ├─ Store user info in state management
        └─ Navigate to /dashboard

2. AUTHENTICATED REQUEST
   ┌──────────┐         GET /api/v1/monitors             ┌──────────┐
   │  Angular │  ────────────────────────────────────────> │  Backend │
   │  Client  │  Authorization: Bearer {accessToken}     │   API    │
   │          │                                            │          │
   │          │  <────────────────────────────────────────│          │
   │          │  { success: true, data: [...] }           │          │
   └──────────┘                                            └──────────┘

3. TOKEN REFRESH (when accessToken expires)
   ┌──────────┐         POST /api/v1/auth/refresh        ┌──────────┐
   │  Angular │  ────────────────────────────────────────> │  Backend │
   │  Client  │  { refreshToken }                         │   API    │
   │          │                                            │          │
   │          │  <────────────────────────────────────────│          │
   │          │  { accessToken, refreshToken, ... }       │          │
   └──────────┘                                            └──────────┘
        │
        ├─ Update accessToken in memory
        ├─ Update refreshToken in storage
        └─ Retry original request

4. LOGOUT
   ┌──────────┐         POST /api/v1/auth/logout         ┌──────────┐
   │  Angular │  ────────────────────────────────────────> │  Backend │
   │  Client  │  Authorization: Bearer {accessToken}     │   API    │
   │          │                                            │          │
   │          │  <────────────────────────────────────────│          │
   │          │  { success: true }                        │          │
   └──────────┘                                            └──────────┘
        │
        ├─ Clear accessToken from memory
        ├─ Clear refreshToken from storage
        ├─ Clear user state
        └─ Navigate to /login
```

### 6.2 Token Storage Strategy

**Recommended Approach:**

- **Access Token:** Store in memory (service variable) - More secure, lost on page refresh
- **Refresh Token:** Store in `httpOnly` cookie (if backend supports) OR `localStorage` with encryption
- **User Info:** Store in state management (NgRx/Signal Store)

**Security Considerations:**

- Never store tokens in `localStorage` without encryption
- Implement token refresh logic before expiration
- Clear all tokens on logout
- Implement CSRF protection for refresh token endpoints

---

## 7. Role-Based Access Control (RBAC)

### 7.1 User Roles

| Role       | Level | Description                                                           |
| ---------- | ----- | --------------------------------------------------------------------- |
| **Owner**  | 1     | Full access to everything including billing and organization settings |
| **Admin**  | 2     | Manage monitors, alerts, channels, and team members (except billing)  |
| **Member** | 3     | Create and manage own monitors and alerts                             |
| **Viewer** | 4     | Read-only access to dashboards and reports                            |

### 7.2 Permission Matrix

| Feature                   | Owner | Admin | Member  | Viewer |
| ------------------------- | ----- | ----- | ------- | ------ |
| **Dashboard**             |
| View dashboard            | ✓     | ✓     | ✓       | ✓      |
| **Monitors**              |
| View monitors             | ✓     | ✓     | ✓       | ✓      |
| Create monitor            | ✓     | ✓     | ✓       | ✗      |
| Edit monitor              | ✓     | ✓     | ✓ (own) | ✗      |
| Delete monitor            | ✓     | ✓     | ✓ (own) | ✗      |
| **Alert Rules**           |
| View alert rules          | ✓     | ✓     | ✓       | ✓      |
| Create alert rule         | ✓     | ✓     | ✓       | ✗      |
| Edit alert rule           | ✓     | ✓     | ✓ (own) | ✗      |
| Delete alert rule         | ✓     | ✓     | ✓ (own) | ✗      |
| **Notification Channels** |
| View channels             | ✓     | ✓     | ✓       | ✓      |
| Create channel            | ✓     | ✓     | ✓       | ✗      |
| Edit channel              | ✓     | ✓     | ✓ (own) | ✗      |
| Delete channel            | ✓     | ✓     | ✓ (own) | ✗      |
| **Settings**              |
| Organization settings     | ✓     | ✗     | ✗       | ✗      |
| Billing                   | ✓     | ✗     | ✗       | ✗      |
| Team management           | ✓     | ✓     | ✗       | ✗      |
| Profile settings          | ✓     | ✓     | ✓       | ✓      |

### 7.3 Frontend Route Guards

```typescript
// src/app/core/auth/guards/role.guard.ts
import { inject } from "@angular/core";
import { Router, CanActivateFn } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { UserRole } from "@shared/models/enums";

export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const currentUser = authService.currentUser();

    if (!currentUser) {
      router.navigate(["/auth/login"]);
      return false;
    }

    if (allowedRoles.includes(currentUser.role)) {
      return true;
    }

    router.navigate(["/unauthorized"]);
    return false;
  };
};
```

---

## 8. Suggested Angular 17 Architecture

### 8.1 Project Structure

```
monitoring-platform-frontend/
├── src/
│   ├── app/
│   │   ├── core/                  # Singleton services (provided in root)
│   │   ├── shared/                # Shared components, pipes, directives
│   │   ├── features/              # Feature modules (lazy-loaded)
│   │   ├── layout/                # Layout components
│   │   ├── app.component.ts
│   │   ├── app.config.ts          # Angular 17 standalone config
│   │   └── app.routes.ts          # Route configuration
│   ├── assets/
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   ├── styles/
│   │   ├── _variables.scss
│   │   ├── _mixins.scss
│   │   └── styles.scss
│   └── index.html
├── angular.json
├── package.json
└── tsconfig.json
```

### 8.2 Key Architectural Decisions

**1. Standalone Components (Angular 17)**

- DO NOT use standalone components anywhere in the application
- MUST use Angular NgModules architecture (AppModule + Feature Modules)
- Every component must be declared inside a module (declarations array)

- Use AppModule as the root module
- Use Feature Modules for each business domain (AuthModule, DashboardModule, etc.)
- Use SharedModule for reusable components

- Use AppRoutingModule for routing configuration
- Use RouterModule.forRoot() and RouterModule.forChild() ONLY
- Use lazy loading via NgModule-based routing (loadChildren)

- Use CoreModule for singleton services (AuthService, Interceptors, Guards)

- HTTP setup:
  - Use HttpClientModule inside AppModule
  - Do NOT use provideHttpClient

- Routing setup:
  - DO NOT use provideRouter
  - DO NOT use app.config.ts
  - All routing must be defined in AppRoutingModule

**2. Signals for State Management**

- Use Angular Signals for reactive state
- Consider NgRx Signal Store for complex state
- Avoid RxJS where Signals suffice

**3. Dependency Injection**

- Use `inject()` function instead of constructor injection
- Provide services at appropriate levels (root, route, component)

**4. Lazy Loading**

- All feature modules should be lazy-loaded
- Use route-level code splitting

**5. TypeScript Strict Mode**

- Enable strict mode in tsconfig.json
- Use strong typing for all models

### 8.3 Core Services Architecture

```typescript
// src/app/core/services/api.service.ts
import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@env/environment";
import { ApiResponse } from "@core/models/api-response.model";

@Injectable({ providedIn: "root" })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  get<T>(endpoint: string, params?: HttpParams): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, {
      params,
    });
  }

  post<T>(endpoint: string, body: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body);
  }

  put<T>(endpoint: string, body: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body);
  }

  delete<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}${endpoint}`);
  }
}
```

---

## 9. State Management Strategy

### 9.1 Recommended Approach: NgRx Signal Store

For this application, **NgRx Signal Store** is recommended for the following reasons:

- Built on Angular Signals (native reactivity)
- Lightweight compared to full NgRx
- Type-safe and easy to test
- Perfect for medium-complexity applications

### 9.2 State Structure

```typescript
// src/app/core/store/auth.store.ts
import { signalStore, withState, withMethods, patchState } from "@ngrx/signals";
import { inject } from "@angular/core";
import { AuthService } from "@core/auth/services/auth.service";
import { LoginResponse } from "@features/auth/models/auth.model";

interface AuthState {
  user: LoginResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
};

export const AuthStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withMethods((store, authService = inject(AuthService)) => ({
    async login(email: string, password: string) {
      patchState(store, { isLoading: true });
      try {
        const response = await authService.login(email, password);
        patchState(store, {
          user: response,
          isAuthenticated: true,
          isLoading: false,
        });
      } catch (error) {
        patchState(store, { isLoading: false });
        throw error;
      }
    },
    logout() {
      patchState(store, initialState);
    },
  })),
);
```

### 9.3 Alternative: Standalone Services with Signals

For simpler state management without external libraries:

```typescript
// src/app/core/auth/services/auth.service.ts
import { Injectable, signal, computed } from "@angular/core";
import { LoginResponse } from "@features/auth/models/auth.model";

@Injectable({ providedIn: "root" })
export class AuthService {
  private userSignal = signal<LoginResponse | null>(null);

  // Public read-only signals
  user = this.userSignal.asReadonly();
  isAuthenticated = computed(() => this.userSignal() !== null);
  userRole = computed(() => this.userSignal()?.role);

  setUser(user: LoginResponse) {
    this.userSignal.set(user);
  }

  clearUser() {
    this.userSignal.set(null);
  }
}
```

---

## 10. Routing Structure

### 10.1 App Routes Configuration

```typescript
// src/app/app.routes.ts
import { Routes } from "@angular/router";
import { authGuard } from "@core/auth/guards/auth.guard";
import { roleGuard } from "@core/auth/guards/role.guard";
import { UserRole } from "@shared/models/enums";

export const routes: Routes = [
  {
    path: "",
    redirectTo: "/dashboard",
    pathMatch: "full",
  },
  {
    path: "auth",
    loadChildren: () =>
      import("./features/auth/auth.routes").then((m) => m.AUTH_ROUTES),
  },
  {
    path: "",
    canActivate: [authGuard],
    children: [
      {
        path: "dashboard",
        loadComponent: () =>
          import("./features/dashboard/pages/dashboard-overview/dashboard-overview.component").then(
            (m) => m.DashboardOverviewComponent,
          ),
      },
      {
        path: "monitors",
        loadChildren: () =>
          import("./features/monitors/monitors.routes").then(
            (m) => m.MONITOR_ROUTES,
          ),
      },
      {
        path: "alert-rules",
        loadChildren: () =>
          import("./features/alert-rules/alert-rules.routes").then(
            (m) => m.ALERT_RULE_ROUTES,
          ),
      },
      {
        path: "notification-channels",
        loadChildren: () =>
          import("./features/notification-channels/notification-channels.routes").then(
            (m) => m.NOTIFICATION_CHANNEL_ROUTES,
          ),
      },
      {
        path: "settings",
        canActivate: [roleGuard([UserRole.Owner, UserRole.Admin])],
        loadChildren: () =>
          import("./features/settings/settings.routes").then(
            (m) => m.SETTINGS_ROUTES,
          ),
      },
    ],
  },
  {
    path: "unauthorized",
    loadComponent: () =>
      import("./shared/pages/unauthorized/unauthorized.component").then(
        (m) => m.UnauthorizedComponent,
      ),
  },
  {
    path: "**",
    loadComponent: () =>
      import("./shared/pages/not-found/not-found.component").then(
        (m) => m.NotFoundComponent,
      ),
  },
];
```

### 10.2 Feature Routes Example

```typescript
// src/app/features/monitors/monitors.routes.ts
import { Routes } from "@angular/router";

export const MONITOR_ROUTES: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./pages/monitor-list/monitor-list.component").then(
        (m) => m.MonitorListComponent,
      ),
  },
  {
    path: "create",
    loadComponent: () =>
      import("./pages/monitor-create/monitor-create.component").then(
        (m) => m.MonitorCreateComponent,
      ),
  },
  {
    path: ":id",
    loadComponent: () =>
      import("./pages/monitor-detail/monitor-detail.component").then(
        (m) => m.MonitorDetailComponent,
      ),
  },
  {
    path: ":id/edit",
    loadComponent: () =>
      import("./pages/monitor-edit/monitor-edit.component").then(
        (m) => m.MonitorEditComponent,
      ),
  },
];
```

---

## 11. API Service Layer

### 11.1 Authentication Service

```typescript
// src/app/core/auth/services/auth.service.ts
import { Injectable, inject, signal, computed } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";
import { environment } from "@env/environment";
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  AuthTokens,
} from "@features/auth/models/auth.model";
import { ApiResponse } from "@core/models/api-response.model";
import { TokenService } from "./token.service";

@Injectable({ providedIn: "root" })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private tokenService = inject(TokenService);

  private userSignal = signal<LoginResponse | null>(null);

  user = this.userSignal.asReadonly();
  isAuthenticated = computed(() => this.userSignal() !== null);
  currentUserRole = computed(() => this.userSignal()?.role);
  organizationId = computed(() => this.userSignal()?.organizationId);

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http
      .post<
        ApiResponse<LoginResponse>
      >(`${environment.apiUrl}/api/v1/auth/login`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data) {
            this.userSignal.set(response.data);
            this.tokenService.setTokens(response.data.tokens);
          }
        }),
      );
  }

  register(
    request: RegisterRequest,
  ): Observable<ApiResponse<RegisterResponse>> {
    return this.http
      .post<
        ApiResponse<RegisterResponse>
      >(`${environment.apiUrl}/api/v1/auth/register`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data) {
            this.userSignal.set(response.data);
            this.tokenService.setTokens(response.data.tokens);
          }
        }),
      );
  }

  logout(): Observable<ApiResponse<void>> {
    return this.http
      .post<ApiResponse<void>>(`${environment.apiUrl}/api/v1/auth/logout`, {})
      .pipe(
        tap(() => {
          this.userSignal.set(null);
          this.tokenService.clearTokens();
          this.router.navigate(["/auth/login"]);
        }),
      );
  }

  refreshToken(): Observable<ApiResponse<AuthTokens>> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.http
      .post<
        ApiResponse<AuthTokens>
      >(`${environment.apiUrl}/api/v1/auth/refresh`, { refreshToken })
      .pipe(
        tap((response) => {
          if (response.success && response.data) {
            this.tokenService.setTokens(response.data);
          }
        }),
      );
  }
}
```

### 11.2 Monitor Service

```typescript
// src/app/features/monitors/services/monitor.service.ts
import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@env/environment";
import {
  MonitorDto,
  CreateMonitorRequest,
  UpdateMonitorRequest,
} from "../models/monitor.model";
import { ApiResponse, PagedResponse } from "@core/models/api-response.model";
import { MonitorType, MonitorStatus } from "@shared/models/enums";

@Injectable({ providedIn: "root" })
export class MonitorService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/v1/monitors`;

  getMonitors(params: {
    search?: string;
    type?: MonitorType;
    status?: MonitorStatus;
    categoryId?: string;
    tagId?: string;
    isUp?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<ApiResponse<PagedResponse<MonitorDto>>> {
    let httpParams = new HttpParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<PagedResponse<MonitorDto>>>(this.baseUrl, {
      params: httpParams,
    });
  }

  getMonitorById(id: string): Observable<ApiResponse<MonitorDto>> {
    return this.http.get<ApiResponse<MonitorDto>>(`${this.baseUrl}/${id}`);
  }

  createMonitor(
    request: CreateMonitorRequest,
  ): Observable<ApiResponse<MonitorDto>> {
    return this.http.post<ApiResponse<MonitorDto>>(this.baseUrl, request);
  }

  updateMonitor(
    id: string,
    request: UpdateMonitorRequest,
  ): Observable<ApiResponse<MonitorDto>> {
    return this.http.put<ApiResponse<MonitorDto>>(
      `${this.baseUrl}/${id}`,
      request,
    );
  }

  deleteMonitor(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`);
  }
}
```

### 11.3 Alert Rule Service

```typescript
// src/app/features/alert-rules/services/alert-rule.service.ts
import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@env/environment";
import {
  AlertRuleDto,
  CreateAlertRuleCommand,
  UpdateAlertRuleCommand,
} from "../models/alert-rule.model";
import { ApiResponse } from "@core/models/api-response.model";

@Injectable({ providedIn: "root" })
export class AlertRuleService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getAlertRules(
    organizationId: string,
    monitorId: string,
  ): Observable<ApiResponse<AlertRuleDto[]>> {
    return this.http.get<ApiResponse<AlertRuleDto[]>>(
      `${this.baseUrl}/api/organizations/${organizationId}/monitors/${monitorId}/alert-rules`,
    );
  }

  getAlertRuleById(
    organizationId: string,
    ruleId: string,
  ): Observable<ApiResponse<AlertRuleDto>> {
    return this.http.get<ApiResponse<AlertRuleDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/monitors/*/alert-rules/${ruleId}`,
    );
  }

  createAlertRule(
    organizationId: string,
    monitorId: string,
    command: CreateAlertRuleCommand,
  ): Observable<ApiResponse<AlertRuleDto>> {
    return this.http.post<ApiResponse<AlertRuleDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/monitors/${monitorId}/alert-rules`,
      command,
    );
  }

  updateAlertRule(
    organizationId: string,
    monitorId: string,
    ruleId: string,
    command: UpdateAlertRuleCommand,
  ): Observable<ApiResponse<AlertRuleDto>> {
    return this.http.put<ApiResponse<AlertRuleDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/monitors/${monitorId}/alert-rules/${ruleId}`,
      command,
    );
  }

  deleteAlertRule(
    organizationId: string,
    ruleId: string,
  ): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/api/organizations/${organizationId}/monitors/*/alert-rules/${ruleId}`,
    );
  }
}
```

### 11.4 Notification Channel Service

```typescript
// src/app/features/notification-channels/services/notification-channel.service.ts
import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@env/environment";
import {
  NotificationChannelDto,
  CreateNotificationChannelCommand,
  UpdateNotificationChannelCommand,
} from "../models/notification-channel.model";
import { ApiResponse } from "@core/models/api-response.model";

@Injectable({ providedIn: "root" })
export class NotificationChannelService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getNotificationChannels(
    organizationId: string,
  ): Observable<ApiResponse<NotificationChannelDto[]>> {
    return this.http.get<ApiResponse<NotificationChannelDto[]>>(
      `${this.baseUrl}/api/organizations/${organizationId}/notification-channels`,
    );
  }

  getNotificationChannelById(
    organizationId: string,
    channelId: string,
  ): Observable<ApiResponse<NotificationChannelDto>> {
    return this.http.get<ApiResponse<NotificationChannelDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/notification-channels/${channelId}`,
    );
  }

  createNotificationChannel(
    organizationId: string,
    command: CreateNotificationChannelCommand,
  ): Observable<ApiResponse<NotificationChannelDto>> {
    return this.http.post<ApiResponse<NotificationChannelDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/notification-channels`,
      command,
    );
  }

  updateNotificationChannel(
    organizationId: string,
    channelId: string,
    command: UpdateNotificationChannelCommand,
  ): Observable<ApiResponse<NotificationChannelDto>> {
    return this.http.put<ApiResponse<NotificationChannelDto>>(
      `${this.baseUrl}/api/organizations/${organizationId}/notification-channels/${channelId}`,
      command,
    );
  }

  deleteNotificationChannel(
    organizationId: string,
    channelId: string,
  ): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/api/organizations/${organizationId}/notification-channels/${channelId}`,
    );
  }
}
```

### 11.5 Dashboard Service

```typescript
// src/app/features/dashboard/services/dashboard.service.ts
import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "@env/environment";
import { AggregatedAnalyticsDto } from "../models/dashboard.model";
import { ApiResponse } from "@core/models/api-response.model";

@Injectable({ providedIn: "root" })
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/v1/dashboard`;

  getSummary(
    timeRange: string = "24h",
  ): Observable<ApiResponse<AggregatedAnalyticsDto>> {
    const params = new HttpParams().set("timeRange", timeRange);
    return this.http.get<ApiResponse<AggregatedAnalyticsDto>>(
      `${this.baseUrl}/summary`,
      { params },
    );
  }
}
```

---

## 12. UI/UX Considerations

### 12.1 Recommended UI Component Library

**Option 1: Angular Material**

- Official Angular component library
- Comprehensive set of components
- Built-in accessibility (WCAG 2.1 AA)
- Theming support
- Well-documented

**Option 2: PrimeNG (Recommended)**

- Rich set of components
- Good for data-heavy applications
- Built-in charts and data tables
- Professional themes

**Option 3: Tailwind CSS + Headless UI**

- Maximum flexibility
- Utility-first approach
- Smaller bundle size
- Requires more custom work

### 12.2 Key UI Components Needed

| Component          | Purpose                                      | Priority |
| ------------------ | -------------------------------------------- | -------- |
| Data Table         | Display monitors list with sorting/filtering | High     |
| Status Badge       | Show monitor status (Up/Down/Paused)         | High     |
| Line Chart         | Display uptime trends                        | High     |
| Card Component     | Display monitor cards, stats cards           | High     |
| Form Components    | Create/edit monitors, alerts                 | High     |
| Modal/Dialog       | Confirmation dialogs, forms                  | High     |
| Toast/Snackbar     | Success/error notifications                  | High     |
| Loading Spinner    | Loading states                               | High     |
| Sidebar Navigation | Main navigation                              | High     |
| Breadcrumbs        | Navigation hierarchy                         | Medium   |
| Tabs               | Organize content                             | Medium   |
| Dropdown Menu      | Actions menu                                 | Medium   |
| Date Range Picker  | Filter by date range                         | Medium   |
| Progress Bar       | Uptime percentage                            | Low      |

### 12.3 Color Scheme Suggestions

```scss
// src/styles/_variables.scss
$primary-color: #3b82f6; // Blue
$success-color: #10b981; // Green (Monitor Up)
$warning-color: #f59e0b; // Orange (Warning)
$danger-color: #ef4444; // Red (Monitor Down)
$info-color: #06b6d4; // Cyan
$neutral-color: #6b7280; // Gray (Paused)

$background-primary: #ffffff;
$background-secondary: #f9fafb;
$text-primary: #111827;
$text-secondary: #6b7280;
$border-color: #e5e7eb;
```

### 12.4 Responsive Design Breakpoints

```scss
// src/styles/_mixins.scss
$breakpoint-mobile: 640px;
$breakpoint-tablet: 768px;
$breakpoint-desktop: 1024px;
$breakpoint-wide: 1280px;

@mixin mobile {
  @media (max-width: #{$breakpoint-mobile}) {
    @content;
  }
}

@mixin tablet {
  @media (min-width: #{$breakpoint-tablet}) {
    @content;
  }
}

@mixin desktop {
  @media (min-width: #{$breakpoint-desktop}) {
    @content;
  }
}
```

### 12.5 Page Layout Examples

**Dashboard Layout:**

```
┌─────────────────────────────────────────────────────────┐
│  Header (Logo, User Menu, Notifications)               │
├──────────┬──────────────────────────────────────────────┤
│          │  Dashboard Overview                          │
│          │  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐        │
│          │  │Total │ │Active│ │Down  │ │Avg   │        │
│ Sidebar  │  │Monit.│ │Monit.│ │Monit.│ │Uptime│        │
│          │  └──────┘ └──────┘ └──────┘ └──────┘        │
│ - Dashb. │                                              │
│ - Monit. │  ┌────────────────────────────────────────┐ │
│ - Alerts │  │  Uptime Chart (Last 24h)               │ │
│ - Chann. │  │  [Line chart showing uptime trends]    │ │
│ - Sett.  │  └────────────────────────────────────────┘ │
│          │                                              │
│          │  Recent Incidents                            │
│          │  ┌────────────────────────────────────────┐ │
│          │  │ [Table of recent incidents]            │ │
│          │  └────────────────────────────────────────┘ │
└──────────┴──────────────────────────────────────────────┘
```

**Monitor List Layout:**

```
┌─────────────────────────────────────────────────────────┐
│  Header                                                 │
├──────────┬──────────────────────────────────────────────┤
│          │  Monitors                    [+ New Monitor] │
│          │  ┌────────────────────────────────────────┐ │
│          │  │ Search: [____] Type: [All▼] Status:[▼]│ │
│ Sidebar  │  └────────────────────────────────────────┘ │
│          │                                              │
│          │  ┌────────────────────────────────────────┐ │
│          │  │ Name    │ Type │ Status │ Uptime │ ... │ │
│          │  ├─────────┼──────┼────────┼────────┼─────┤ │
│          │  │ API Srv │ HTTP │ ● Up   │ 99.9%  │ ... │ │
│          │  │ DB Srv  │ TCP  │ ● Up   │ 100%   │ ... │ │
│          │  │ Web App │ HTTPS│ ● Down │ 95.2%  │ ... │ │
│          │  └────────────────────────────────────────┘ │
│          │  [Pagination: 1 2 3 ... 10]                 │
└──────────┴──────────────────────────────────────────────┘
```

### 12.6 Accessibility Requirements

- **WCAG 2.1 Level AA Compliance**
- Keyboard navigation support
- Screen reader compatibility
- Proper ARIA labels
- Color contrast ratios (4.5:1 for text)
- Focus indicators
- Alt text for images

---

## 13. HTTP Interceptors

### 13.1 Auth Interceptor

```typescript
// src/app/core/auth/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { TokenService } from "../services/token.service";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getAccessToken();

  if (token && !req.url.includes("/auth/")) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req);
};
```

### 13.2 Error Interceptor

```typescript
// src/app/core/auth/interceptors/error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
        router.navigate(["/auth/login"]);
      }

      if (error.status === 403) {
        router.navigate(["/unauthorized"]);
      }

      return throwError(() => error);
    }),
  );
};
```

---

## 14. Environment Configuration

```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: "http://localhost:5000",
  tokenRefreshThreshold: 300000, // 5 minutes in ms
  defaultPageSize: 10,
  maxPageSize: 100,
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: "https://api.monitoring-platform.com",
  tokenRefreshThreshold: 300000,
  defaultPageSize: 10,
  maxPageSize: 100,
};
```

---

## 15. Implementation Roadmap

### Phase 1: Foundation (Week 1-2)

- [ ] Set up Angular 17 project with standalone components
- [ ] Configure routing and lazy loading
- [ ] Implement authentication (login, register, logout)
- [ ] Create core services (API, Auth, Token)
- [ ] Set up HTTP interceptors
- [ ] Implement layout components (header, sidebar, footer)

### Phase 2: Core Features (Week 3-4)

- [ ] Dashboard module with analytics
- [ ] Monitors module (list, create, edit, delete)
- [ ] Monitor detail page with charts
- [ ] Implement pagination and filtering
- [ ] Add loading states and error handling

### Phase 3: Advanced Features (Week 5-6)

- [ ] Alert Rules module
- [ ] Notification Channels module
- [ ] Settings module (profile, organization)
- [ ] Role-based access control
- [ ] Real-time updates (SignalR/WebSocket)

### Phase 4: Polish & Testing (Week 7-8)

- [ ] Unit tests for services
- [ ] Integration tests for components
- [ ] E2E tests for critical flows
- [ ] Performance optimization
- [ ] Accessibility audit
- [ ] Documentation

---

## 16. Key Takeaways

### Backend Summary

- **Architecture:** Clean Architecture + CQRS
- **Authentication:** JWT + Refresh Token
- **Multi-tenancy:** Organization-based
- **API Response:** Standardized `ApiResponse<T>` wrapper

### Frontend Recommendations

- **Framework**: Angular 17 (NO standalone components, NgModule-based architecture only)
- **State Management**: RxJS Services (Signals optional for local state only, NOT NgRx required)
- **UI Library**: PrimeNG (fully adopted across application)
- **HTTP Client**: Angular HttpClient with interceptors (JWT + Error handling)
- **Routing**: Lazy-loaded feature modules using NgModule-based routing
- **Testing**: Jest + Testing Library

### Critical Implementation Notes

1. **Organization ID:** Required in most API calls (from user context)
2. **Token Refresh:** Implement automatic refresh before expiration
3. **Error Handling:** Centralized error interceptor
4. **Loading States:** Global loading indicator + local spinners
5. **Pagination:** Backend supports pagination, implement on frontend
6. **Real-time Updates:** Consider WebSocket for live monitor status

---

## 17. Next Steps

1. **Review this blueprint** with the development team
2. **Set up development environment** (Node.js, Angular CLI)
3. **Create Angular 17 project** with recommended structure
4. **Implement authentication flow** first (critical path)
5. **Build dashboard** as the main landing page
6. **Iterate on monitors module** (most complex feature)
7. **Add alert rules and notification channels**
8. **Polish UI/UX** and add animations
9. **Write tests** for critical paths
10. **Deploy to staging** for QA testing

---

**Document Version:** 1.0.0  
**Last Updated:** 2026-06-01  
**Author:** Senior Solution Architect  
**Status:** Ready for Implementation
