using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 设备信息表
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("Device_Info")]
    public class Device_Info : FullAuditedEntity<int>
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// 设备描述
        /// </summary>
        [StringLength(250)]
        public string DeviceDescribe { get; set; }
        /// <summary>
        /// 设备在线状态（0：未激活(初始化)； 1：在线；  2：离线)
        /// </summary>
        public int? DeviceStatus { get; set; }
        /// <summary>
        /// 心跳周期（单位为分钟）
        /// </summary>
        public int? HbeatT { get; set; }
        /// <summary>
        /// 故障检查周期，以分钟为单位，示例为060=60分钟
        /// </summary>
        public int? healthT { get; set; }
        /// <summary>
        /// NB 信号强度
        /// </summary>
        [StringLength(10)]
        public string NbSignal { get; set; }
        /// <summary>
        /// 射频故障代码：00 正常，01 信号差
        /// </summary>
        public int? NbSignalCode { get; set; }
        /// <summary>
        /// 磁地和雷达传感器故障代码：  00 正常，01磁传感器故障，02雷达传感器故障，03磁和雷达传感器故障
        /// </summary>
        public int? SensorCode { get; set; }
        /// <summary>
        /// 存储器故障代码：00 正常，01 无法读写，02 已写满
        /// </summary>
        public int? FlashCode { get; set; }
        /// <summary>
        /// 电池故障代码：00 正常，01 电池电压低
        /// </summary>
        public int? BatteryCode { get; set; }
        /// <summary>
        /// 电池电压,单位为V
        /// </summary>
        [StringLength(10)]
        public string BatVoltage { get; set; }
        /// <summary>
        /// 设备健康状态(00：设备无故障;  01：电池电量低; 02：地磁故障)
        /// </summary>
        public int? DevHealth { get; set; }
        /// <summary>
        /// 地磁检测状态：00 无车，01 有车，02 等待激活，03 强磁
        /// </summary>
        public int? MagStatus { get; set; }
        /// <summary>
        /// 雷达检测状态：00 无车，01 有车，02 遮挡，其他：失效
        /// </summary>
        public int? RadaStatus { get; set; }
        /// <summary>
        /// 综合状态：00 无车，01 有车，02 等待激活，03 初始化中
        /// </summary>
        public int? SynStatus { get; set; }
        /// <summary>
        /// 温度值(单位 1℃)，例如 +25表示当前温度为25℃，-10 表示-10℃
        /// </summary>
        public int? Temperature { get; set; }
        /// <summary>
        /// 磁场三轴采样(XYZ)
        /// </summary>
        [StringLength(15)]
        public string MagneticXYZ { get; set; }
        /// <summary>
        /// 雷达三轴采样(XYZ)
        /// </summary>
        [StringLength(15)]
        public string RadarXYZ { get; set; }
        /// <summary>
        /// 雷达检测到的距离值（mm）
        /// </summary>
        public int? Distance { get; set; }
        /// <summary>
        /// 设备的软件版本，示例 10 即 1.0 版
        /// </summary>
        [StringLength(10)]
        public string FwVer { get; set; }
        /// <summary>
        /// 设备的硬件版本，示例 10 即 1.0 版
        /// </summary>
        [StringLength(10)]
        public string HwVer { get; set; }
        /// <summary>
        /// 设备更新时间
        /// </summary>
        public DateTime? StatusUpdateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(256)]
        public string Remark { get; set; }
    }
}
