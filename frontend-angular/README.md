# Monitoring Platform - Angular 17 Frontend

## Project Structure

This project follows **Clean Architecture** principles with **NgModules** (no standalone components).

```
src/
├── app/
│   ├── core/                 # Singleton services, guards, interceptors
│   ├── shared/               # Reusable components, pipes, directives
│   ├── layout/               # Application layout components
│   ├── features/             # Feature modules (lazy-loaded)
│   ├── app.module.ts
│   └── app-routing.module.ts
├── assets/
├── environments/
└── styles/
```

## Architecture Layers

### 1. Core Module
- **Services**: AuthService, ApiService, TokenService
- **Interceptors**: AuthInterceptor, ErrorInterceptor
- **Guards**: AuthGuard, RoleGuard
- **Models**: Core interfaces and types

### 2. Shared Module
- Reusable UI components
- Common pipes and directives
- Utility functions

### 3. Layout Module
- Application shell components
- Navigation components
- Header, Sidebar, Footer

### 4. Feature Modules (Lazy-loaded)
- Auth Module
- Dashboard Module
- Monitors Module
- Alert Rules Module
- Notification Channels Module
- Organizations Module
- Users Module
- Settings Module

## Technology Stack

- **Angular**: 17.x
- **TypeScript**: 5.x
- **RxJS**: 7.x
- **Angular Material**: 17.x (UI components)
- **NgRx Signal Store**: State management
- **Chart.js**: Dashboard charts

## Getting Started

```bash
# Install dependencies
npm install

# Development server
ng serve

# Build for production
ng build --configuration production
```

## Module Loading Strategy

All feature modules use **lazy loading** for optimal performance:

```typescript
{
  path: 'dashboard',
  loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
}
```

## Authentication Flow

1. User logs in → JWT + Refresh Token stored
2. AuthInterceptor attaches JWT to all requests
3. On 401 error → Attempt token refresh
4. On refresh failure → Redirect to login

## Development Guidelines

- **No standalone components** - Use NgModules only
- **Lazy load all feature modules**
- **Keep core module singleton** - Import only in AppModule
- **Share common functionality** via SharedModule
- **Follow Angular style guide**
- **Use TypeScript strict mode**
