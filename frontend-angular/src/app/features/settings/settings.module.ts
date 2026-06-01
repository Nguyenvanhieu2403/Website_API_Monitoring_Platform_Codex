import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsRoutingModule } from './settings-routing.module';
import { ProfileSettingsComponent } from './profile-settings.component';
import { SystemSettingsComponent } from './system-settings.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [
    ProfileSettingsComponent,
    SystemSettingsComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    SettingsRoutingModule,
    SharedModule
  ]
})
export class SettingsModule { }
