import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LogsRoutingModule } from './logs-routing.module';
import { LogsListComponent } from './logs-list.component';

@NgModule({
  declarations: [
    LogsListComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    LogsRoutingModule
  ]
})
export class LogsModule { }
