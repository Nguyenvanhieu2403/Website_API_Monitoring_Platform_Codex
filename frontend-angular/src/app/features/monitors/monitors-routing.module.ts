import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MonitorsListComponent } from './monitors-list.component';
import { MonitorDetailComponent } from './monitor-detail.component';
import { MonitorFormComponent } from './monitor-form.component';

const routes: Routes = [
  {
    path: '',
    component: MonitorsListComponent
  },
  {
    path: 'new',
    component: MonitorFormComponent
  },
  {
    path: ':id',
    component: MonitorDetailComponent
  },
  {
    path: ':id/edit',
    component: MonitorFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MonitorsRoutingModule { }
