import { Component, OnInit, Injector } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { DeviceServiceProxy, DeviceListOutput } from '@shared/service-proxies/service-proxies';
import { FileUploader, FileUploaderOptions } from 'ng2-file-upload';
import { AppConsts } from '@shared/AppConsts';
import { IAjaxResponse } from '@abp/abpHttpInterceptor';
import { TokenService } from '@abp/auth/token.service';
import { timestamp } from 'rxjs/operators';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';

@Component({
  selector: 'app-device-list',
  templateUrl: './device-list.component.html',
  styleUrls: ['./device-list.component.less'],
  animations: [appModuleAnimation()]
})
export class DeviceListComponent extends AppComponentBase implements OnInit {

  _pageIndex = 1;
  _pageSize = 8;
  _total = 0;
  _searchValue = '';
  _deviceList: DeviceListOutput[];
  _loading = true;
  _sortValue: string = 'StatusUpdateTime';
  _sortKey: string = 'ASC';
  //设备添加时间
  creationDateStart: moment.Moment;
  creationDateEnd: moment.Moment;
   //清除筛选是否可见
   isShow = false;
  //使用状态筛选
  deviceStatus: Array<number>;
  //使用状态数据绑定
  deviceStatusOptions = [
    {value: 0, label: '新增(未连接)', checked: false},
    {value: 1, label: '在线', checked: false},
    {value: 2, label: '离线', checked: false},
  ];
  //泊位状态的筛选与绑定synStatus
  synStatus: Array<number>;
  //使用状态数据绑定
  synStatusOptions = [
    {value: 0, label: '无车', checked: false},
    {value: 1, label: '有车', checked: false},
    {value: 2, label: '等待激活', checked: false},
    {value: 3, label: '初始化中', checked: false},
  ];

  //添加设备模态框是否显示
  _visibleCreate = false;
  _deviceNo = '';
  _deviceDescribe = '';
  _addLoading = false;

  //文件上传
  _uploading = false;
  _uploader: FileUploader;

  constructor(
    injector: Injector,
    private deviceServiceProxy: DeviceServiceProxy,
    private _tokenService: TokenService) {
    super(injector);
    this._deviceList = [];
    this.deviceStatus = [];
    this.synStatus = [];
    //文件上传配置
    this.initUploaders();
  }

  ngOnInit() {
    this.getDeviceList(true);
  }
  //排序
  sort(sort: { key: string; value: string }): void {
    if (sort.value != null) {
      this._sortValue = sort.key;
      this._sortKey = sort.value.replace('ascend', 'ASC').replace('descend', 'desc');
      this.getDeviceList();
    }
  }
  //搜索框的回车键触发搜索
  onSearch(event: KeyboardEvent) {
    if (event.keyCode == 13) {
      this.getDeviceList(true);
    }
  }
  //点搜索框图标触发搜索
  searchGetInfo() {
    this.getDeviceList(true);
  }
  _szDeviceStatus:string[];
  _szSynStatus:string[];
  //使用状态选择
  deviceStatusChange(item) {
    this.deviceStatus.length = 0;
    this.deviceStatusOptions.forEach(o => {
        if (o.checked) {
            this.deviceStatus.push(o.value);
        }
    });
    this.doChange();
  }
  //泊位状态选择
  synStatusChange(item){
    this.synStatus.length = 0;
    this.synStatusOptions.forEach(o => {
        if (o.checked) {
            this.synStatus.push(o.value);
        }
    });
    this.doChange();
  }
  //清除筛选
  doClear(){
    this.isShow = false;
    this.creationDateStart = undefined;
    this.creationDateEnd = undefined;
    this.synStatusOptions.forEach(o => {o.checked = false; });
    this.deviceStatusOptions.forEach(o => {o.checked = false; });
    this.synStatus.length = 0;
    this.deviceStatus.length = 0;
  }
  //选择变更
  doChange() {
    this.isShow = false;
    if (this.creationDateStart) {
        this.isShow = true;
    }
    if (this.creationDateEnd) {
        this.isShow = true;
    }
    if (this.synStatus && this.synStatus.length > 0) {
        this.isShow = true;
    }
    if (this.deviceStatus && this.deviceStatus.length > 0) {
        this.isShow = true;
    }
  }
  //获取设备信息
  getDeviceList(reset = false) {
    var creationDateStart = this.creationDateStart ? moment(this.creationDateStart) : undefined;
    var creationDateEnd = this.creationDateEnd ? moment(this.creationDateEnd) : undefined;
    this._loading = true;
    if (reset) {
      this._pageIndex = 1;
    }
    let shipCount = (this._pageIndex - 1) * this._pageSize;
    this.deviceServiceProxy.getDevicesInfoList(creationDateStart, creationDateEnd, this.deviceStatus, this.synStatus, this._searchValue, this._sortKey, this._sortValue, this._pageSize, shipCount).subscribe(result => {
      this._loading = false;
      this._total = result.totalCount;
      this._deviceList = result.items;
    });
  }
  delEvent(e) {
    e.stopPropagation();
  }
  //输入长度限制
  onKey(event: any, maxLength: number) { // without type info
    if (event.target.value.length > maxLength) {
      event.target.value = event.target.value.substr(0, maxLength);
    }
  }
  //添加设备
  doAddDevice() {
    this._visibleCreate = true;
    this._addLoading = false;
    this._deviceNo = '';
    this._deviceDescribe = '';
  }
  //取消添加设备
  cancelAdd() {
    this._visibleCreate = false;
    this._deviceNo = '';
    this._deviceDescribe = '';
  }
  //执行添加设备
  onCreateDevice() {
    this._addLoading = true;
    if (this._deviceNo == '' || this._deviceNo == undefined) {
      abp.message.warn("设备编号不能为空!");
      return;
    }
    if (this._deviceNo.length > 9) {
      this._deviceNo = this._deviceNo.substr(0, 9);
    }
    if(this._deviceNo.length!=9){
      abp.message.warn("设备编号为9位!");
      return;
    }
    this.deviceServiceProxy.addDevice(this._deviceNo, this._deviceDescribe).subscribe(result => {
      if (result) {
        abp.notify.success("添加成功！");
        this.getDeviceList(true);
      } else {
        abp.message.error("添加失败！");
      }
    });
    this._visibleCreate = false;
    this._deviceNo = '';
    this._deviceDescribe = '';
  }
  //文件上传
  initUploaders(): void {
    this._uploader = this.createUploader(
      '/api/FileManager/UploadPlanFiles',
      result => {
        //上传成功
        if (result.flag === true) {
          //this.getVersion(this.getPlanDate);
          abp.message.success('导入成功');
          this.getDeviceList(true);
        } else {
          abp.message.error("导入异常");
        }
      },
      () => {
        //失败
        abp.notify.error('导入失败');
        this._uploader.cancelAll();
      },
      () => {
        this._uploading = false;
        this._uploader.clearQueue();
      }
    );
  }

  createUploader(url: string, success?: (result: any) => void, error?: () => void, complete?: () => void): FileUploader {
    const uploader = new FileUploader({ url: AppConsts.remoteServiceBaseUrl + url, removeAfterUpload: true });
    /*
    *  触发时机：添加一个文件之后的回调
    *  file - 添加的文件信息，FileItem类型。
    */
    uploader.onAfterAddingFile = (file) => {
      let flag = false;
      let acceptFile = file.file.type;
      if (acceptFile == "application/vnd.ms-excel") {
        if (file.file.name.lastIndexOf('.csv') >= 0) {
          abp.message.error('选择的文件类型不正确，支持.xls或.xlsx文件');
          this._uploader.removeFromQueue(file);
        } else {
          flag = true;
        }
      } else if (acceptFile == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
        flag = true;
      }
      if (flag == false) {
        abp.notify.error('选择的文件类型不正确，请重新选择!');
        this._uploader.removeFromQueue(file);
      }
    };
    /*
    *   触发时机：上传一个文件成功后回调
    *   item - 上传成功的文件信息，FileItem类型；
    *   response - 上传成功后服务器的返回信息；
    *   status - 状态码；
    */
    uploader.onSuccessItem = (item, response, status) => {
      const ajaxResponse = <IAjaxResponse>JSON.parse(response);
      if (ajaxResponse.success) {
        if (success) {
          success(ajaxResponse.result);
        }
      } else {
        //this.message.error(ajaxResponse.error.message);
      }
    };
    uploader.onErrorItem = (item, response, status) => {
      if (error) {
        error();
      }
    }
    uploader.onCompleteAll = () => {
      if (complete) {
        complete();
      }
    };

    const uploaderOptions: FileUploaderOptions = {};
    uploaderOptions.authToken = 'Bearer ' + this._tokenService.getToken();
    uploaderOptions.removeAfterUpload = true;
    uploader.setOptions(uploaderOptions);
    return uploader;
  }
  onUpload() {
    if (this._uploader.queue.length == 1) {
      abp.notify.info("正在导入，请稍等...");
      this._uploading = true;
      this._uploader.queue.forEach(o => { o.url });
      this._uploader.uploadAll();
    } else if (this._uploader.queue.length > 1) {
      abp.message.error("仅支持单文件上传");
      this._uploader.clearQueue();
      this._uploading = false;
    }
  }
  //导出数据
  doExportExcel() {
    abp.notify.info("正在执行，请稍等...");
    var creationDateStart = this.creationDateStart ? moment(this.creationDateStart) : undefined;
    var creationDateEnd = this.creationDateEnd ? moment(this.creationDateEnd) : undefined;
    this.deviceServiceProxy.getDevicesToExcel(creationDateStart, creationDateEnd, this.deviceStatus, this.synStatus, this._searchValue, this._sortKey, this._sortValue).subscribe(result => {
      if (result) {
        abp.notify.success("正在生成报表并下载，请稍等...");
        let r = encodeURI(AppConsts.remoteServiceBaseUrl + '/api/FileManager/DownloadDevicesExcelFile');
        this.downloadFun(r);
      } else {
        abp.message.warn("未查询到数据，无法导出！")
      }
    });
  }
  //下载文件
  downloadFun(url) {
    let a = document.createElement('a');
    a.href = url;
    //a.target = '_blank';
    a.id = 'exppub';
    //a.download = filename;
    document.body.appendChild(a);
    let alink = document.getElementById('exppub');
    alink.click();
    alink.parentNode.removeChild(a);
  }
  //删除设备
  onDeleteDevice(id: number) {
    var thisClone = this;
    abp.message.confirm('', '您确定要删除吗？', function (isConfirmed) {
      if (isConfirmed) {
        thisClone.deviceServiceProxy.deleteDevice(id).subscribe(result => {
          if (result) {
            abp.notify.success('删除成功');
            thisClone.getDeviceList(true);
          } else {
            abp.message.error("删除失败！");
          }
        });
      }
    });
  }
  //下发检测板重新标定
  onReCalibration(deviceNo: string, deviceStatus :number){
    if(deviceStatus !=1){
      abp.message.warn("非在线设备不能发送指令!");
      return;
    }
    var thisClone = this;
    thisClone.deviceServiceProxy.reCalibration(deviceNo).subscribe(result=>{
      if(result.isSuccess){
        abp.notify.success("指令已发送");
        thisClone.count = 0;
        setTimeout(() => {
          this.checkCommand(deviceNo, result.timeStamp,"重新标定");
        }, 3000);
      }else{
        abp.notify.error("指令发送失败");
      }
    });
  }
  count:number=0;
  checkCommand(deviceNo: string, timeStamp:string, msg:string){
    var thisClone = this;
    thisClone.deviceServiceProxy.checkCommandReviceRet(deviceNo, timeStamp).subscribe(result=>{
      if(result.isSuccess){
        thisClone.count++;
       if(result.cmdStatus == 2){
         if(msg == "重新标定"){
           let res = '';
           if(result.reviceMsg=="00,"){
            res="执行成功";
            abp.notify.success(msg +"指令" + res);
           }else if(result.reviceMsg=="01,"){
            res="执行错误";
            abp.notify.error(msg +"指令" + res);
           }
         }
       }
       if(thisClone.count<2 && result.cmdStatus!=2){
        setTimeout(() => {
          this.checkCommand(deviceNo, timeStamp,"重新标定");
        }, 3000);
       }
      }else{
        abp.notify.error("指令发送失败");
      }
    });
  }

}
