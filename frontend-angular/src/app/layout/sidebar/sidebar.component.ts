import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  children?: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  @Input() collapsed = false;

  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: '📊',
      route: '/dashboard'
    },
    {
      label: 'Monitors',
      icon: '🖥️',
      route: '/monitors'
    },
    {
      label: 'Alert Rules',
      icon: '🔔',
      route: '/alert-rules'
    },
    {
      label: 'Notifications',
      icon: '📧',
      route: '/notification-channels'
    },
    {
      label: 'Organizations',
      icon: '🏢',
      route: '/organizations'
    },
    {
      label: 'Users',
      icon: '👥',
      route: '/users'
    },
    {
      label: 'Settings',
      icon: '⚙️',
      route: '/settings'
    }
  ];

  constructor(private router: Router) {}

  navigate(route: string): void {
    this.router.navigate([route]);
  }

  isActive(route: string): boolean {
    return this.router.url.startsWith(route);
  }
}
