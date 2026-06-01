import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardComponent } from './dashboard.component';
import { StatsCardComponent } from '../../shared/components/stats-card/stats-card.component';
import { StatusCardComponent } from '../../shared/components/status-card/status-card.component';
import { ChartContainerComponent } from '../../shared/components/chart-container/chart-container.component';

@NgModule({
  declarations: [
    DashboardComponent,
    StatsCardComponent,
    StatusCardComponent,
    ChartContainerComponent
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule
  ]
})
export class DashboardModule { }
