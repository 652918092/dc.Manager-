using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 设备检测参数表
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("Device_CheckParam")]
    public class Device_CheckParam: Entity, IHasModificationTime
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// 磁场信号平稳时的采样周期，以毫秒为单位。
        /// </summary>
        public int? NormalT { get; set; }
        /// <summary>
        /// 磁场信号波动时的采样周期，以毫秒为单位
        /// </summary>
        public int? FlunctT { get; set; }
        /// <summary>
        /// 快速检测阈值上限
        /// </summary>
        public int? MagBigThresh { get; set; }
        /// <summary>
        /// 平稳检测阈值上限
        /// </summary>
        public int? MagMidThresh { get; set; }
        /// <summary>
        /// 低磁信号车辆检测阈值
        /// </summary>
        public int? MagLittleThresh { get; set; }
        /// <summary>
        /// 检测阈值下限
        /// </summary>
        public int? DowThresh { get; set; }
        /// <summary>
        /// 检测基线值即标定值,顺序为x,y,z。
        /// </summary>
        [StringLength(20)]
        public string XyzBaseLine { get; set; }
        /// <summary>
        /// 当前检测值,顺序为x,y,z
        /// </summary>
        [StringLength(20)]
        public string XyzNowValue { get; set; }
        /// <summary>
        /// 雷达检测基线值即标定值,前三组。
        /// </summary>
        [StringLength(20)]
        public string RadarBaseLine { get; set; }
        /// <summary>
        /// 雷达当前检测值,顺序为,前三组。
        /// </summary>
        [StringLength(20)]
        public string RadarNowValue { get; set; }
        /// <summary>
        /// 地磁检测状态。00:无车，01:有车，02:等待激活，03:初始化中。
        /// </summary>
        public int? MagStatus { get; set; }
        /// <summary>
        /// 雷达检测状态。00:无车，01:有车，02:遮挡，失效。查询时为0，设置时无效。
        /// </summary>
        public int? RadaStatus { get; set; }
        /// <summary>
        /// 综合检测状态。00:无车，01:有车，02:等待激活，03:初始化中。
        /// </summary>
        public int? SynStatus { get; set; }
        /// <summary>
        /// 当时雷达检测到的距离值mm
        /// </summary>
        public int? Distance { get; set; }
        /// <summary>
        /// 磁传感器增益值{00-07,}
        /// </summary>
        public int? MagGain { get; set; }
        /// <summary>
        /// 雷达传感器增益值{00-09,}
        /// </summary>
        public int? RadarGain { get; set; }
        /// <summary>
        /// 记录添加时间
        /// </summary>
        public DateTime? AddTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }
    }
}
