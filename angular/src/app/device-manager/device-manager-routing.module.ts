import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppRouteGuard } from '@shared/auth/auth-route-guard';
import { DeviceListComponent } from './device-list/device-list.component';


@NgModule({
  imports: [
    RouterModule.forChild([
        {
            path: '',
            children: [
                { path: 'home', component: DeviceListComponent ,  canActivate: [AppRouteGuard]},
            ]
        }
    ])
],
exports: [
    RouterModule
]
})
export class DeviceManagerRoutingModule { }
