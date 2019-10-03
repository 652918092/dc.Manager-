import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeviceManagerRoutingModule } from './device-manager-routing.module';
import { DeviceListComponent } from './device-list/device-list.component';
import { DeviceServiceProxy } from '@shared/service-proxies/service-proxies';
import { MomentFormatPipe } from '@shared/pipes/moment-format.pipe';
import { FileUploadModule } from '@node_modules/ng2-file-upload';
import { ModalModule } from 'ngx-bootstrap';
import { SharedModule } from '@shared/shared.module';
import { NgZorroAntdModule } from 'ng-zorro-antd';

@NgModule({
  declarations: [
    DeviceListComponent,
    MomentFormatPipe
  ],
  imports: [
    CommonModule,
    SharedModule,
    DeviceManagerRoutingModule,
    FormsModule,
    FileUploadModule,
    NgZorroAntdModule,
    ModalModule.forRoot()
  ],
  providers: [
    DeviceServiceProxy
  ],
  entryComponents: [
    // tenant
    DeviceListComponent
  ]
})
export class DeviceManagerModule { }
