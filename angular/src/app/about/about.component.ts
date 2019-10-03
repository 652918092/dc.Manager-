import { Component, Injector, AfterViewInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as moment from 'moment';
@Component({
    templateUrl: './about.component.html',
    animations: [appModuleAnimation()]
})
export class AboutComponent extends AppComponentBase {
    // creationDateEnd: moment.Moment;
    // _pageIndex = 1;
    // _pageSize = 8;
    // _total = 13;
    // _loading = true;
    // _sortValue: string = 'name';
    // _sortKey: string = 'ASC';

    // listOfData = [
    //     {
    //       key: '1',
    //       name: 'John Brown',
    //       age: 32,
    //       address: 'New York No. 1 Lake Park'
    //     },
    //     {
    //       key: '2',
    //       name: 'Jim Green',
    //       age: 42,
    //       address: 'London No. 1 Lake Park'
    //     },
    //     {
    //       key: '3',
    //       name: 'Joe Black',
    //       age: 32,
    //       address: 'Sidney No. 1 Lake Park'
    //     },
    //     {
    //         key: '4',
    //         name: 'Joe Blac4k',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '5',
    //         name: 'Joe Black',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '6',
    //         name: 'Joe Black6',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '7',
    //         name: 'Joe Black7',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '8',
    //         name: 'Joe Black8',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '9',
    //         name: 'Joe Black9',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '10',
    //         name: 'Joe Black10',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '11',
    //         name: 'Joe Black11',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '12',
    //         name: 'Joe Black12',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //       {
    //         key: '13',
    //         name: 'Joe Black13',
    //         age: 32,
    //         address: 'Sidney No. 1 Lake Park'
    //       },
    //   ];
    constructor(
        injector: Injector
    ) {
        super(injector);
    }
    // doChange(){
    //     this._loading =false;
    //     abp.message.success("OK");
    // }
    // getDeviceList(reset = false) {
    //     abp.notify.success("OK");
    // }
    // sort(sort: { key: string; value: string }): void {
    //     if (sort.value != null) {
    //       this._sortValue = sort.key;
    //       this._sortKey = sort.value.replace('ascend', 'ASC').replace('descend', 'desc');
    //       this.getDeviceList();
    //     }
    //   }
}
