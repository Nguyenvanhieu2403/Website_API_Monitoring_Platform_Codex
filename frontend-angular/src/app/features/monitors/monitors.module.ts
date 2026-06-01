import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MonitorsRoutingModule } from './monitors-routing.module';
import { MonitorsListComponent } from './monitors-list.component';
import { MonitorDetailComponent } from './monitor-detail.component';
import { MonitorFormComponent } from './monitor-form.component';

@NgModule({
  declarations: [
    MonitorsListComponent,
    MonitorDetailComponent,
    MonitorFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MonitorsRoutingModule
  ]
})
export class MonitorsModule { }
