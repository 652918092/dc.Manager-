using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 心跳数据包记录表
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("Heartbeat_Log")]
    public class Heartbeat_Log: Entity
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// 设备状态：00 无车，01 有车，02 等待激活，03 初始化中
        /// </summary>
        public int? DeviceState { get; set; }
        /// <summary>
        /// 时间戳，节点生成数据时的本地时间。例如20180506110515表示2018年5月6日11时05分15秒。
        /// </summary>
        [StringLength(15)]
        public string Tstamp { get; set; }
        /// <summary>
        /// NB 信号强度
        /// </summary>
        public string NbSignal { get; set; }
        /// <summary>
        /// 温度值(单位 1℃)，例如 +25表示当前温度为25℃，-10 表示-10℃
        /// </summary>
        public int? Temperature { get; set; }
        /// <summary>
        /// 电池电压,单位为V
        /// </summary>
        [StringLength(10)]
        public string BatVoltage { get; set; }
        /// <summary>
        /// 设备健康状态(0：设备无故障;  1：电池电量低; 2：地磁故障)
        /// </summary>
        public int? DevHealth { get; set; }
        /// <summary>
        /// 心跳周期（单位为分钟）
        /// </summary>
        public int? HbeatT { get; set; }
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
        /// NB模块发送次数
        /// </summary>
        public int? NbSendN { get; set; }
        /// <summary>
        /// NB模块接收次数
        /// </summary>
        public int? BbRecvN { get; set; }
        /// <summary>
        /// 传感器采样次数
        /// </summary>
        public int? SampleN { get; set; }
        /// <summary>
        /// 设备重启次数
        /// </summary>
        public int? RebootN { get; set; }
        /// <summary>
        /// NB模块重启次数
        /// </summary>
        public int? NbRebootN { get; set; }
        /// <summary>
        /// 上传后未收到应答的次数
        /// </summary>
        public int? NoackN { get; set; }
        /// <summary>
        /// 记录添加时间
        /// </summary>
        public DateTime? AddTime { get; set; }
    }
}
