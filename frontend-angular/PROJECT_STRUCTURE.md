# Monitoring Platform - Angular 17 Frontend

Complete Angular 17 project skeleton with **NgModules** (no standalone components) following **Clean Architecture** principles.

## 📁 Project Structure

```
frontend-angular/
├── src/
│   ├── app/
│   │   ├── core/                          # Singleton services, guards, interceptors
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── models/
│   │   │   │   ├── api.model.ts
│   │   │   │   ├── auth.model.ts
│   │   │   │   ├── monitor.model.ts
│   │   │   │   ├── alert-rule.model.ts
│   │   │   │   ├── notification-channel.model.ts
│   │   │   │   └── dashboard.model.ts
│   │   │   ├── services/
│   │   │   │   ├── api.service.ts
│   │   │   │   ├── auth.service.ts
│   │   │   │   └── token.service.ts
│   │   │   └── core.module.ts
│   │   │
│   │   ├── shared/                        # Reusable components, pipes, directives
│   │   │   ├── directives/
│   │   │   │   └── has-role.directive.ts
│   │   │   ├── pipes/
│   │   │   │   └── time-ago.pipe.ts
│   │   │   └── shared.module.ts
│   │   │
│   │   ├── layout/                        # Application layout
│   │   │   ├── header/
│   │   │   │   ├── header.component.ts
│   │   │   │   ├── header.component.html
│   │   │   │   └── header.component.scss
│   │   │   ├── sidebar/
│   │   │   │   ├── sidebar.component.ts
│   │   │   │   ├── sidebar.component.html
│   │   │   │   └── sidebar.component.scss
│   │   │   ├── main-layout/
│   │   │   │   ├── main-layout.component.ts
│   │   │   │   ├── main-layout.component.html
│   │   │   │   └── main-layout.component.scss
│   │   │   ├── layout-routing.module.ts
│   │   │   └── layout.module.ts
│   │   │
│   │   ├── features/                      # Feature modules (lazy-loaded)
│   │   │   ├── auth/
│   │   │   │   ├── auth-routing.module.ts
│   │   │   │   └── auth.module.ts
│   │   │   ├── dashboard/
│   │   │   │   ├── dashboard-routing.module.ts
│   │   │   │   └── dashboard.module.ts
│   │   │   ├── monitors/
│   │   │   │   ├── monitors-routing.module.ts
│   │   │   │   └── monitors.module.ts
│   │   │   ├── alert-rules/
│   │   │   │   ├── alert-rules-routing.module.ts
│   │   │   │   └── alert-rules.module.ts
│   │   │   ├── notification-channels/
│   │   │   │   ├── notification-channels-routing.module.ts
│   │   │   │   └── notification-channels.module.ts
│   │   │   ├── organizations/
│   │   │   │   ├── organizations-routing.module.ts
│   │   │   │   └── organizations.module.ts
│   │   │   ├── users/
│   │   │   │   ├── users-routing.module.ts
│   │   │   │   └── users.module.ts
│   │   │   └── settings/
│   │   │       ├── settings-routing.module.ts
│   │   │       └── settings.module.ts
│   │   │
│   │   ├── app-routing.module.ts
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   └── app.module.ts
│   │
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   │
│   ├── assets/
│   ├── styles.scss
│   ├── index.html
│   └── main.ts
│
├── angular.json
├── package.json
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.spec.json
└── README.md
```

## ✅ What's Included

### 1. Core Module (Singleton)
- **Services**: 
  - `ApiService` - HTTP wrapper
  - `AuthService` - Authentication logic
  - `TokenService` - JWT token management
- **Guards**: 
  - `AuthGuard` - Route protection
  - `RoleGuard` - Role-based access control
- **Interceptors**: 
  - `AuthInterceptor` - JWT token injection + refresh
  - `ErrorInterceptor` - Global error handling
- **Models**: TypeScript interfaces for all DTOs

### 2. Shared Module
- **Directives**: `HasRoleDirective` - Conditional rendering based on roles
- **Pipes**: `TimeAgoPipe` - Relative time formatting
- Exports `CommonModule`, `FormsModule`, `ReactiveFormsModule`

### 3. Layout Module
- **MainLayoutComponent** - Application shell
- **HeaderComponent** - Top navigation with user menu
- **SidebarComponent** - Side navigation menu
- Collapsible sidebar functionality

### 4. Feature Modules (Lazy-loaded)
- ✅ Auth Module
- ✅ Dashboard Module
- ✅ Monitors Module
- ✅ Alert Rules Module
- ✅ Notification Channels Module
- ✅ Organizations Module
- ✅ Users Module
- ✅ Settings Module

### 5. Configuration Files
- ✅ `angular.json` - Angular CLI configuration
- ✅ `package.json` - Dependencies
- ✅ `tsconfig.json` - TypeScript configuration
- ✅ Environment files (dev + prod)

## 🚀 Getting Started

### Prerequisites
- Node.js 18+ and npm

### Installation

```bash
cd frontend-angular

# Install dependencies
npm install

# Start development server
npm start

# Navigate to http://localhost:4200
```

### Build

```bash
# Development build
npm run build

# Production build
npm run build:prod
```

## 🔐 Authentication Flow

1. User logs in via `AuthService.login()`
2. JWT access token stored in memory
3. Refresh token stored in localStorage
4. `AuthInterceptor` attaches JWT to all requests
5. On 401 error → automatic token refresh
6. On refresh failure → redirect to login

## 🛡️ Route Guards

### AuthGuard
Protects routes requiring authentication:
```typescript
{ path: 'dashboard', canActivate: [AuthGuard], ... }
```

### RoleGuard
Protects routes requiring specific roles:
```typescript
{ 
  path: 'users', 
  canActivate: [RoleGuard], 
  data: { roles: [UserRole.Owner, UserRole.Admin] }
}
```

## 📦 Module Loading Strategy

All feature modules use **lazy loading**:

```typescript
{
  path: 'dashboard',
  loadChildren: () => import('./features/dashboard/dashboard.module')
    .then(m => m.DashboardModule)
}
```

## 🎨 Styling

- Global styles in `src/styles.scss`
- Component-specific styles use SCSS
- Utility classes for spacing

## 📝 Next Steps

1. **Install dependencies**: `npm install`
2. **Update API URL** in `environment.ts` to match your backend
3. **Implement feature components** in each module
4. **Add UI library** (optional): Angular Material, PrimeNG, etc.
5. **Implement state management** (optional): NgRx Signal Store
6. **Add form validation** in auth and CRUD forms
7. **Implement dashboard charts** using Chart.js or similar
8. **Add unit tests** for services and components

## 🔧 Development Guidelines

- **No standalone components** - Use NgModules only
- **Lazy load all feature modules**
- **Keep CoreModule singleton** - Import only in AppModule
- **Share common functionality** via SharedModule
- **Follow Angular style guide**
- **Use TypeScript strict mode**

## 📚 Architecture Principles

### Clean Architecture Layers

1. **Core Layer** - Business logic, services, models
2. **Shared Layer** - Reusable UI components
3. **Feature Layer** - Feature-specific components
4. **Layout Layer** - Application shell

### Dependency Rule
- Core → Independent
- Shared → Depends on Core
- Features → Depend on Core + Shared
- Layout → Depends on Core + Shared

---

**Status**: ✅ Project skeleton complete - Ready for feature implementation
