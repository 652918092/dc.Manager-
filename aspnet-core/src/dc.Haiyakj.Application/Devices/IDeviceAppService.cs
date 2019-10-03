using Abp.Application.Services;
using Abp.Application.Services.Dto;
using dc.Haiyakj.Devices.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Devices
{
    public interface IDeviceAppService: IApplicationService
    {
        /// <summary>
        /// 获取设备信息列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<DeviceListOutput>> GetDevicesInfoList(DeviceListInput input);
        /// <summary>
        /// 添加设备(添加前先查询是否已存在该设备编号，如果没有则添加，有则返回添加失败)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="remark">设备描述</param>
        /// <returns></returns>
        Task<bool> AddDevice(string deviceNo, string remark);
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteDevice(int id);
        /// <summary>
        /// 检测板重新标定指令下发
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <returns>SendCommandDto:是否下发成功及时间戳</returns>
        Task<SendCommandDto> ReCalibration(string deviceNo);
        /// <summary>
        /// 查询指令回复情况
        /// </summary>
        /// <param name="device">设备编号</param>
        /// <param name="timeStamp">指令时间戳</param>
        /// <returns></returns>
        Task<CheckCommandReviceDto> CheckCommandReviceRet(string deviceNo, string timeStamp);
        /// <summary>
        /// 获取设备信息列表并生成excel
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> GetDevicesToExcel(DeviceListToExcelInput input);
    }
}
