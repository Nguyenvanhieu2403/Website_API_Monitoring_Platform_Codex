import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LogsListComponent } from './logs-list.component';

const routes: Routes = [
  {
    path: '',
    component: LogsListComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LogsRoutingModule { }
