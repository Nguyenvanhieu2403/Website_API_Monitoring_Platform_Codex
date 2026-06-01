import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './main-layout/main-layout.component';

const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('../features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'monitors',
        loadChildren: () => import('../features/monitors/monitors.module').then(m => m.MonitorsModule)
      },
      {
        path: 'alert-rules',
        loadChildren: () => import('../features/alert-rules/alert-rules.module').then(m => m.AlertRulesModule)
      },
      {
        path: 'alerts',
        loadChildren: () => import('../features/alerts/alerts.module').then(m => m.AlertsModule)
      },
      {
        path: 'logs',
        loadChildren: () => import('../features/logs/logs.module').then(m => m.LogsModule)
      },
      {
        path: 'notification-channels',
        loadChildren: () => import('../features/notification-channels/notification-channels.module').then(m => m.NotificationChannelsModule)
      },
      {
        path: 'organizations',
        loadChildren: () => import('../features/organizations/organizations.module').then(m => m.OrganizationsModule)
      },
      {
        path: 'users',
        loadChildren: () => import('../features/users/users.module').then(m => m.UsersModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('../features/settings/settings.module').then(m => m.SettingsModule)
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LayoutRoutingModule { }
