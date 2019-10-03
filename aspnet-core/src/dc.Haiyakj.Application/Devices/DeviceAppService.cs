using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using dc.Haiyakj.Authorization.Users;
using dc.Haiyakj.Communication;
using dc.Haiyakj.Devices.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using System.IO;
using Abp.Dependency;
using System.Net;

namespace dc.Haiyakj.Devices
{
    public class DeviceAppService: AbpProjectNameAppServiceBase, IDeviceAppService
    {
        private readonly IRepository<User, long> _userManager;
        private readonly IRepository<Device_Info> _deviceInfo;
        private readonly IRepository<Device_IPEndPort> _deviceIPEndPort;
        private readonly IRepository<Command_Log> _commandLog;
        private AppFolders _appFolders;

        public DeviceAppService(
            IRepository<User, long> userManager,
            IRepository<Device_Info> deviceInfo,
            IRepository<Device_IPEndPort> deviceIPEndPort,
            IRepository<Command_Log> commandLog)
        {
            this._userManager = userManager;
            this._deviceInfo = deviceInfo;
            this._deviceIPEndPort = deviceIPEndPort;
            this._commandLog = commandLog;
            this._appFolders = IocManager.Instance.Resolve<AppFolders>();
        }
        /// <summary>
        /// 获取设备信息列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<DeviceListOutput>> GetDevicesInfoList(DeviceListInput input)
        {
            var query = _deviceInfo.GetAll()
                        .WhereIf(input.CreationDateStart.HasValue, d => Convert.ToDateTime(d.CreationTime.ToString("yyyy-MM-dd")) >= Convert.ToDateTime(input.CreationDateStart.Value.ToString("yyyy-MM-dd")))
                        .WhereIf(input.CreationDateEnd.HasValue, d => Convert.ToDateTime(d.CreationTime.ToString("yyyy-MM-dd")) <= Convert.ToDateTime(input.CreationDateEnd.Value.ToString("yyyy-MM-dd")))
                        .WhereIf(!String.IsNullOrWhiteSpace(input.SearchValue), d => d.DeviceNo.Contains(input.SearchValue))
                        .WhereIf(input.SynStatus != null && input.SynStatus.Count > 0, d => d.SynStatus != null && input.SynStatus.Contains(Convert.ToInt32(d.SynStatus)))
                        .WhereIf(input.DeviceStatus != null && input.DeviceStatus.Count > 0, d => d.DeviceStatus != null && input.DeviceStatus.Contains(Convert.ToInt32(d.DeviceStatus)));
            var taskCount = query.Count();
            var deviceListDto = await query.OrderBy(input.Sorting + "  " + input.AscOrDesc).PageBy(input).ToListAsync();
            var deviceList = ObjectMapper.Map<List<DeviceListOutput>>(deviceListDto);

            return new PagedResultDto<DeviceListOutput>(
                taskCount,
                deviceList
                );
        }
     
        /// <summary>
        /// 添加设备(添加前先查询是否已存在该设备编号，如果没有则添加，有则返回添加失败)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="remark">设备描述</param>
        /// <returns></returns>
        public async Task<bool> AddDevice(string deviceNo, string deviceDescribe)
        {
            var task = new Task<bool>(() =>
            {
                var info = this._deviceInfo.GetAll().Where(d => d.DeviceNo == deviceNo).FirstOrDefault();
                if(info != null)
                {
                    return false;
                }
                Device_Info model = new Device_Info
                {
                    DeviceNo = deviceNo,
                    DeviceDescribe = deviceDescribe,
                    DeviceStatus = 0,
                };
                this._deviceInfo.InsertAsync(model);
                return true;
            });
            task.Start();
            return await task;
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDevice(int id)
        {
            var model = _deviceInfo.GetAll().Where(c => c.Id == id).FirstOrDefault();
            if (model != null)
            {
                await _deviceInfo.DeleteAsync(model);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检测板重新标定指令下发
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <returns>SendCommandDto:是否下发成功及时间戳</returns>
        public async Task<SendCommandDto> ReCalibration(string deviceNo)
        {
            Task<SendCommandDto> taskDto = new Task<SendCommandDto>(() =>
            {
                SendCommandDto dto = new SendCommandDto();
                dto.IsSuccess = false;
                if (string.IsNullOrWhiteSpace(deviceNo))
                {
                    return dto;
                }
                //查找设备是否存在且在线连接
                var model = this._deviceInfo.GetAll().Where(d => d.DeviceNo == deviceNo && d.DeviceStatus == 1).FirstOrDefault();
                if (model == null)
                {
                    return dto;
                }
                //查看设备绑定的IP地址
                var iPmodel = this._deviceIPEndPort.GetAll().Where(p => p.IsUseing == true && p.DeviceNo == deviceNo).FirstOrDefault();
                if (iPmodel == null || string.IsNullOrWhiteSpace(iPmodel.IpEndPort))
                {
                    return dto;
                }
                IPAddress IPadr = IPAddress.Parse(iPmodel.IpEndPort.Split(':')[0]);//先把string类型转换成IPAddress类型
                IPEndPoint ipEndPort = new IPEndPoint(IPadr, int.Parse(iPmodel.IpEndPort.Split(':')[1]));

                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string cmd = SendCommands.STestingBoardAgainSet(deviceNo, timeStamp);

                //插入数据记录帧
                Command_Log cmdLog = new Command_Log
                {
                    DeviceNo = deviceNo,
                    CommandType = "A6",
                    Tstamp = timeStamp,
                    AddTime = DateTime.Now,
                    SendType = 0,
                    CmdStatus = 0
                };
                _commandLog.InsertAsync(cmdLog);
                //指令下发
                bool ret = UdpCommunication.UpdSendMessage(cmd, ipEndPort);
                if(ret)
                {
                    cmdLog.CmdStatus = 1;
                    _commandLog.InsertOrUpdateAsync(cmdLog);
                    dto.IsSuccess = true;
                    dto.TimeStamp = timeStamp;
                }
                return dto;
            });
            taskDto.Start();
            return await taskDto;
        }
        /// <summary>
        /// 查询指令回复情况
        /// </summary>
        /// <param name="device">设备编号</param>
        /// <param name="timeStamp">指令时间戳</param>
        /// <returns></returns>
        public async Task<CheckCommandReviceDto> CheckCommandReviceRet(string deviceNo, string timeStamp)
        {
            var task = new Task<CheckCommandReviceDto>(() =>
            {
                CheckCommandReviceDto dto = new CheckCommandReviceDto();
                dto.IsSuccess = false;
                var cmdModel = this._commandLog.GetAll().Where(c => c.DeviceNo == deviceNo && c.Tstamp==timeStamp).FirstOrDefault();
                if (cmdModel == null)
                {
                    return dto;
                }
                dto.ReviceMsg = cmdModel.ResultMessage;
                dto.CmdStatus = cmdModel.CmdStatus;
                dto.IsSuccess = true;
                return dto;
            });
            task.Start();
            return await task;
        }
        /// <summary>
        /// 获取设备信息列表并生成excel
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> GetDevicesToExcel(DeviceListToExcelInput input)
        {
            var task = new Task<bool>(() =>
            {
                var query = _deviceInfo.GetAll()
                      .WhereIf(input.CreationDateStart.HasValue, d => Convert.ToDateTime(d.CreationTime.ToString("yyyy-MM-dd")) >= Convert.ToDateTime(input.CreationDateStart.Value.ToString("yyyy-MM-dd")))
                      .WhereIf(input.CreationDateEnd.HasValue, d => Convert.ToDateTime(d.CreationTime.ToString("yyyy-MM-dd")) <= Convert.ToDateTime(input.CreationDateEnd.Value.ToString("yyyy-MM-dd")))
                      .WhereIf(!String.IsNullOrWhiteSpace(input.SearchValue), d => d.DeviceNo.Contains(input.SearchValue))
                      .WhereIf(input.SynStatus != null && input.SynStatus.Count > 0, d => d.SynStatus != null && input.SynStatus.Contains(Convert.ToInt32(d.SynStatus)))
                      .WhereIf(input.DeviceStatus != null && input.DeviceStatus.Count > 0, d => d.DeviceStatus != null && input.DeviceStatus.Contains(Convert.ToInt32(d.DeviceStatus)));

                var deviceListDto = query.OrderBy(input.Sorting + "  " + input.AscOrDesc);
                var deviceList = ObjectMapper.Map<List<DeviceListOutput>>(deviceListDto);
                if (deviceList.Count > 0)
                {
                    TaskBuildExcel(deviceList);
                    return true;
                }
                else
                {
                    return false;
                }
            });
            task.Start();
            return await task;
        }
        #region NPOI插件生成Excel
        /// <summary>
        /// 生成任务的Excel报表
        /// </summary>
        /// <param name="units"></param>
        private void TaskBuildExcel(List<DeviceListOutput> devices)
        {
            /////文件生成存放路径
            string filePath = this._appFolders.ExcelFolder + "/导出报表/设备列表.xlsx";


            //内存中创建一个hssfworkbook对象流（创建一个Excel文件）
            XSSFWorkbook xssfworkbook = new XSSFWorkbook();
            //创建sheet
            XSSFSheet Sheet1 = (XSSFSheet)xssfworkbook.CreateSheet("Sheet1");
            #region
            //第一行
            XSSFRow fcell = (XSSFRow)Sheet1.CreateRow(0);
            fcell.CreateCell(0).SetCellValue("设备明细表");
            //合并单元格
            Sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 23));
            #endregion

            XSSFCellStyle fCellStyle = (XSSFCellStyle)xssfworkbook.CreateCellStyle();
            XSSFFont ffont = (XSSFFont)xssfworkbook.CreateFont();
            ffont.FontHeightInPoints = 16;
            ffont.FontName = "宋体";
            ffont.Boldweight = (short)FontBoldWeight.Bold;//加粗;
            fCellStyle.SetFont(ffont);
            fCellStyle.Alignment = HorizontalAlignment.Left;
            fCellStyle.VerticalAlignment = VerticalAlignment.Center;
            fCellStyle.BorderBottom = BorderStyle.Thin;
            fCellStyle.BorderLeft = BorderStyle.Thin;
            fCellStyle.BorderRight = BorderStyle.Thin;
            fCellStyle.BorderTop = BorderStyle.Thin;
            Sheet1.GetRow(0).Height = 30 * 20;

            #region 标题风格
            fcell.GetCell(0).CellStyle = fCellStyle;
            fcell.CreateCell(1).CellStyle = fCellStyle;
            fcell.CreateCell(2).CellStyle = fCellStyle;
            fcell.CreateCell(3).CellStyle = fCellStyle;
            fcell.CreateCell(4).CellStyle = fCellStyle;
            fcell.CreateCell(5).CellStyle = fCellStyle;
            fcell.CreateCell(6).CellStyle = fCellStyle;
            fcell.CreateCell(7).CellStyle = fCellStyle;
            fcell.CreateCell(8).CellStyle = fCellStyle;
            fcell.CreateCell(9).CellStyle = fCellStyle;
            fcell.CreateCell(10).CellStyle = fCellStyle;
            fcell.CreateCell(11).CellStyle = fCellStyle;
            fcell.CreateCell(12).CellStyle = fCellStyle;
            fcell.CreateCell(13).CellStyle = fCellStyle;
            fcell.CreateCell(14).CellStyle = fCellStyle;
            fcell.CreateCell(15).CellStyle = fCellStyle;
            fcell.CreateCell(16).CellStyle = fCellStyle;
            fcell.CreateCell(17).CellStyle = fCellStyle;
            fcell.CreateCell(18).CellStyle = fCellStyle;
            fcell.CreateCell(19).CellStyle = fCellStyle;
            fcell.CreateCell(20).CellStyle = fCellStyle;
            fcell.CreateCell(21).CellStyle = fCellStyle;
            fcell.CreateCell(22).CellStyle = fCellStyle;
            fcell.CreateCell(23).CellStyle = fCellStyle;
            #endregion 标题风格

            //设置Sheet1的每一列的宽度
            #region 每一列的宽度
            Sheet1.SetColumnWidth(0, 7 * 256);
            Sheet1.SetColumnWidth(1, 11 * 256);
            Sheet1.SetColumnWidth(2, 23 * 256);
            Sheet1.SetColumnWidth(3, 11 * 256);
            Sheet1.SetColumnWidth(4, 9 * 256);
            Sheet1.SetColumnWidth(5, 9 * 256);
            Sheet1.SetColumnWidth(6, 9 * 256);
            Sheet1.SetColumnWidth(7, 9 * 256);
            Sheet1.SetColumnWidth(8, 9 * 256);
            Sheet1.SetColumnWidth(9, 9 * 256);
            Sheet1.SetColumnWidth(10, 18 * 256);
            Sheet1.SetColumnWidth(11, 9 * 256);
            Sheet1.SetColumnWidth(12, 9 * 256);
            Sheet1.SetColumnWidth(13, 9 * 256);
            Sheet1.SetColumnWidth(14, 9 * 256);
            Sheet1.SetColumnWidth(15, 9 * 256);
            Sheet1.SetColumnWidth(16, 9 * 256);
            Sheet1.SetColumnWidth(17, 9 * 256);
            Sheet1.SetColumnWidth(18, 9 * 256);
            Sheet1.SetColumnWidth(19, 9 * 256);
            Sheet1.SetColumnWidth(20, 9 * 256);
            Sheet1.SetColumnWidth(21, 9 * 256);
            Sheet1.SetColumnWidth(22, 9 * 256);
            Sheet1.SetColumnWidth(23, 18 * 256);
            #endregion 每一列的宽度

            //创建第二行
            XSSFRow rowob2 = (XSSFRow)Sheet1.CreateRow(1);
            #region 字段名
            rowob2.CreateCell(0).SetCellValue("序号");
            rowob2.CreateCell(1).SetCellValue("设备编号");
            rowob2.CreateCell(2).SetCellValue("设备描述");
            rowob2.CreateCell(3).SetCellValue("使用状态");
            rowob2.CreateCell(4).SetCellValue("泊位状态(综合)");
            rowob2.CreateCell(5).SetCellValue("心跳周期");
            rowob2.CreateCell(6).SetCellValue("检查周期");
            rowob2.CreateCell(7).SetCellValue("温度值");
            rowob2.CreateCell(8).SetCellValue("电压");
            rowob2.CreateCell(9).SetCellValue("信号强度");
            rowob2.CreateCell(10).SetCellValue("更新时间");
            rowob2.CreateCell(11).SetCellValue("地磁检测状态");
            rowob2.CreateCell(12).SetCellValue("雷达检测状态");
            rowob2.CreateCell(13).SetCellValue("射频故障代码");
            rowob2.CreateCell(14).SetCellValue("磁地和雷达故障代码");
            rowob2.CreateCell(15).SetCellValue("存储器故障代码");
            rowob2.CreateCell(16).SetCellValue("电池故障代码");
            rowob2.CreateCell(17).SetCellValue("设备健康状态");
            rowob2.CreateCell(18).SetCellValue("磁场三轴采样");
            rowob2.CreateCell(19).SetCellValue("雷达三轴采样");
            rowob2.CreateCell(20).SetCellValue("雷达检测距离值");
            rowob2.CreateCell(21).SetCellValue("设备软件版本");
            rowob2.CreateCell(22).SetCellValue("设备硬件版本");
            rowob2.CreateCell(23).SetCellValue("添加时间");
            #endregion 字段名

            //设置第二行的样式
            XSSFCellStyle Style = (XSSFCellStyle)xssfworkbook.CreateCellStyle();
            XSSFFont ffont1 = (XSSFFont)xssfworkbook.CreateFont();
            ffont1.FontHeightInPoints = 10;
            ffont1.Boldweight = (short)FontBoldWeight.Bold;//加粗;
            ffont1.FontName = "宋体";
            Style.Alignment = HorizontalAlignment.Center;
            Style.VerticalAlignment = VerticalAlignment.Center;
            Style.WrapText = true;
            Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            Style.FillPattern = FillPattern.SolidForeground;
            Style.BorderBottom = BorderStyle.Thin;
            Style.BorderLeft = BorderStyle.Thin;
            Style.BorderRight = BorderStyle.Thin;
            Style.BorderTop = BorderStyle.Thin;
            Style.SetFont(ffont1);

            #region 字段名风格
            rowob2.GetCell(0).CellStyle = Style;
            rowob2.GetCell(1).CellStyle = Style;
            rowob2.GetCell(2).CellStyle = Style;
            rowob2.GetCell(3).CellStyle = Style;
            rowob2.GetCell(4).CellStyle = Style;
            rowob2.GetCell(5).CellStyle = Style;
            rowob2.GetCell(6).CellStyle = Style;
            rowob2.GetCell(7).CellStyle = Style;
            rowob2.GetCell(8).CellStyle = Style;
            rowob2.GetCell(9).CellStyle = Style;
            rowob2.GetCell(10).CellStyle = Style;
            rowob2.GetCell(11).CellStyle = Style;
            rowob2.GetCell(12).CellStyle = Style;
            rowob2.GetCell(13).CellStyle = Style;
            rowob2.GetCell(14).CellStyle = Style;
            rowob2.GetCell(15).CellStyle = Style;
            rowob2.GetCell(16).CellStyle = Style;
            rowob2.GetCell(17).CellStyle = Style;
            rowob2.GetCell(18).CellStyle = Style;
            rowob2.GetCell(19).CellStyle = Style;
            rowob2.GetCell(20).CellStyle = Style;
            rowob2.GetCell(21).CellStyle = Style;
            rowob2.GetCell(22).CellStyle = Style;
            rowob2.GetCell(23).CellStyle = Style;
            #endregion 字段名风格
            //设置表格内容显示样式1
            #region 
            XSSFCellStyle ContentStyle = (XSSFCellStyle)xssfworkbook.CreateCellStyle();
            XSSFFont contentFfont = (XSSFFont)xssfworkbook.CreateFont();
            contentFfont.FontHeightInPoints = 9;
            contentFfont.FontName = "宋体";
            ContentStyle.Alignment = HorizontalAlignment.Left;
            ContentStyle.VerticalAlignment = VerticalAlignment.Top;
            ContentStyle.WrapText = true;
            ContentStyle.BorderBottom = BorderStyle.Thin;
            ContentStyle.BorderLeft = BorderStyle.Thin;
            ContentStyle.BorderRight = BorderStyle.Thin;
            ContentStyle.BorderTop = BorderStyle.Thin; 
            ContentStyle.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("text");
            ContentStyle.SetFont(contentFfont);
            #endregion
            //设置表格内容显示样式2
            #region
            XSSFCellStyle ContentStyle2 = (XSSFCellStyle)xssfworkbook.CreateCellStyle();
            XSSFFont contentFfont2 = (XSSFFont)xssfworkbook.CreateFont();
            contentFfont2.FontHeightInPoints = 9;
            contentFfont2.FontName = "宋体";
            ContentStyle2.Alignment = HorizontalAlignment.Center;
            ContentStyle2.VerticalAlignment = VerticalAlignment.Center;
            ContentStyle2.WrapText = true;
            ContentStyle2.BorderBottom = BorderStyle.Thin;
            ContentStyle2.BorderLeft = BorderStyle.Thin;
            ContentStyle2.BorderRight = BorderStyle.Thin;
            ContentStyle2.BorderTop = BorderStyle.Thin;
            ContentStyle2.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("text");
            ContentStyle2.SetFont(contentFfont2);
            #endregion

            int index = 1;
            foreach (var device in devices)
            {

                XSSFRow row = (XSSFRow)Sheet1.CreateRow(index + 1);
                //序号
                row.CreateCell(0).SetCellValue(index);

                #region 写值
                row.CreateCell(1).SetCellValue(device.DeviceNo);           //设备编号
                row.CreateCell(2).SetCellValue(device.DeviceDescribe);     //设备描述
                switch (device.DeviceStatus)                               //使用状态
                {
                    case 0:
                        row.CreateCell(3).SetCellValue("新增(未连接)");
                        break;
                    case 1:
                        row.CreateCell(3).SetCellValue("在线");
                        break;
                    case 2:
                        row.CreateCell(3).SetCellValue("离线");
                        break;
                    default:
                        row.CreateCell(3).SetCellValue("");
                        break;
                }                               
                switch (device.SynStatus)
                {
                    case 0:
                        row.CreateCell(4).SetCellValue("无车");
                        break;
                    case 1:
                        row.CreateCell(4).SetCellValue("有车");
                        break;
                    case 2:
                        row.CreateCell(4).SetCellValue("等待激活");
                        break;
                    case 3:
                        row.CreateCell(4).SetCellValue("初始化中");
                        break;
                    default:
                        row.CreateCell(4).SetCellValue("");
                        break;
                }                               //综合状态
                //心跳周期
                if (device.HbeatT != null)                                 
                    row.CreateCell(5).SetCellValue(device.HbeatT + "分钟");    
                else
                    row.CreateCell(5).SetCellValue("");
                //故障检查周期
                if (device.healthT != null)                                 
                    row.CreateCell(6).SetCellValue(device.healthT + "分钟");
                else
                    row.CreateCell(6).SetCellValue("");
                //温度值
                if (device.Temperature != null)
                    row.CreateCell(7).SetCellValue(device.Temperature + "℃");
                else
                    row.CreateCell(7).SetCellValue("");
                //电压
                if (device.BatVoltage != null)
                    row.CreateCell(8).SetCellValue(device.BatVoltage + "V");
                else
                    row.CreateCell(8).SetCellValue("");

                row.CreateCell(9).SetCellValue(device.NbSignal);    //NB 信号强度

                if (device.StatusUpdateTime == null)//设备更新时间
                    row.CreateCell(10).SetCellValue("");   
                else
                    row.CreateCell(10).SetCellValue(device.StatusUpdateTime.Value.ToString("yyyy/MM/dd HH:mm:ss")); 
                switch (device.MagStatus)                               //地磁检测状态
                {
                    case 0:
                        row.CreateCell(11).SetCellValue("无车");
                        break;
                    case 1:
                        row.CreateCell(11).SetCellValue("有车");
                        break;
                    case 2:
                        row.CreateCell(11).SetCellValue("等待激活");
                        break;
                    case 3:
                        row.CreateCell(11).SetCellValue("强磁");
                        break;
                    default:
                        row.CreateCell(11).SetCellValue("");
                        break;
                }
                switch (device.RadaStatus)       //雷达检测状态
                {
                    case 0:
                        row.CreateCell(12).SetCellValue("无车");
                        break;
                    case 1:
                        row.CreateCell(12).SetCellValue("有车");
                        break;
                    case 2:
                        row.CreateCell(12).SetCellValue("遮挡");
                        break;
                    default:
                        row.CreateCell(12).SetCellValue("失效");
                        break;
                }
                switch (device.NbSignalCode)                               //射频故障代码
                {
                    case 0:
                        row.CreateCell(13).SetCellValue("00(正常)");
                        break;
                    case 1:
                        row.CreateCell(13).SetCellValue("01(信号差)");
                        break;
                    default:
                        row.CreateCell(13).SetCellValue("");
                        break;
                }
                switch (device.SensorCode)                               //磁地和雷达传感器故障代码
                {
                    case 0:
                        row.CreateCell(14).SetCellValue("00(正常)");
                        break;
                    case 1:
                        row.CreateCell(14).SetCellValue(device.FlashCode + "01(磁传感器故障)");
                        break;
                    case 2:
                        row.CreateCell(14).SetCellValue(device.FlashCode + "02(雷达传感器故障)");
                        break;
                    case 3:
                        row.CreateCell(14).SetCellValue(device.FlashCode + "03(磁和雷达传感器故障)");
                        break;
                    default:
                        row.CreateCell(14).SetCellValue("");
                        break;
                }
                switch (device.FlashCode)                               //存储器故障代码
                {
                    case 0:
                        row.CreateCell(15).SetCellValue("01(正常)");
                        break;
                    case 1:
                        row.CreateCell(15).SetCellValue("02(无法读写)");
                        break;
                    case 2:
                        row.CreateCell(15).SetCellValue("03(已写满)");
                        break;
                    default:
                        row.CreateCell(15).SetCellValue("");
                        break;
                }
                switch (device.BatteryCode)                               //电池故障代码
                {
                    case 0:
                        row.CreateCell(16).SetCellValue("00(正常)");
                        break;
                    case 1:
                        row.CreateCell(16).SetCellValue("01(电池电压低)");
                        break;
                    default:
                        row.CreateCell(16).SetCellValue("");
                        break;
                }
                switch (device.DevHealth)                               //设备健康状态
                {
                    case 0:
                        row.CreateCell(17).SetCellValue("00(设备无故障)");
                        break;
                    case 1:
                        row.CreateCell(17).SetCellValue("01(电池电量低)");
                        break;
                    case 2:
                        row.CreateCell(17).SetCellValue("02(地磁故障)");
                        break;
                    default:
                        row.CreateCell(17).SetCellValue("");
                        break;
                }
                row.CreateCell(18).SetCellValue(device.MagneticXYZ);    //磁场三轴采样
                row.CreateCell(19).SetCellValue(device.RadarXYZ);    //雷达三轴采样
                //雷达检测到的距离值
                if (device.Distance != null)
                    row.CreateCell(20).SetCellValue(device.Distance + "mm");
                else
                    row.CreateCell(20).SetCellValue("");
                row.CreateCell(21).SetCellValue(device.FwVer);    //设备软件版本
                row.CreateCell(22).SetCellValue(device.HwVer);    //设备硬件版本
                //设备添加时间
                if (device.CreationTime == null)
                    row.CreateCell(23).SetCellValue("");
                else
                    row.CreateCell(23).SetCellValue(device.CreationTime.Value.ToString("yyyy/MM/dd HH:mm:ss"));
                #endregion 写值

                Sheet1.GetRow(index + 1).Height = 30 * 15;
                row.GetCell(0).CellStyle = ContentStyle2;
                row.GetCell(1).CellStyle = ContentStyle2;
                row.GetCell(2).CellStyle = ContentStyle;
                row.GetCell(3).CellStyle = ContentStyle2;
                row.GetCell(4).CellStyle = ContentStyle2;
                row.GetCell(5).CellStyle = ContentStyle2;
                row.GetCell(6).CellStyle = ContentStyle2;
                row.GetCell(7).CellStyle = ContentStyle2;
                row.GetCell(8).CellStyle = ContentStyle2;
                row.GetCell(9).CellStyle = ContentStyle2;
                row.GetCell(10).CellStyle = ContentStyle;
                row.GetCell(11).CellStyle = ContentStyle2;
                row.GetCell(12).CellStyle = ContentStyle2;
                row.GetCell(13).CellStyle = ContentStyle2;
                row.GetCell(14).CellStyle = ContentStyle2;
                row.GetCell(15).CellStyle = ContentStyle2;
                row.GetCell(16).CellStyle = ContentStyle2;
                row.GetCell(17).CellStyle = ContentStyle2;
                row.GetCell(18).CellStyle = ContentStyle2;
                row.GetCell(19).CellStyle = ContentStyle2;
                row.GetCell(20).CellStyle = ContentStyle2;
                row.GetCell(21).CellStyle = ContentStyle2;
                row.GetCell(22).CellStyle = ContentStyle2;
                row.GetCell(23).CellStyle = ContentStyle;

                index++;
            }
            string directory = Path.GetDirectoryName(filePath);
            // 如果不存在该目录就创建该目录
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            FileStream fs = new FileStream(filePath, FileMode.Create);
            xssfworkbook.Write(fs);
            fs.Close();
        }
        #endregion NPOI插件生成Excel
    }
}
