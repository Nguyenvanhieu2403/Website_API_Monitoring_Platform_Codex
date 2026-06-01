import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlertsRoutingModule } from './alerts-routing.module';
import { AlertsListComponent } from './alerts-list.component';

@NgModule({
  declarations: [
    AlertsListComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    AlertsRoutingModule
  ]
})
export class AlertsModule { }
