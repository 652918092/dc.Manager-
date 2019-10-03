using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 传感器波动数据记录
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("SensorWave_Log")]
    public class SensorWave_Log : Entity
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// 时间戳，节点生成数据时的本地时间。例如20180506110515表示2018年5月6日11时05分15秒。
        /// </summary>
        [StringLength(15)]
        public string Tstamp { get; set; }
        /// <summary>
        /// 波动的磁场信号每 10 次采样为一组，上传到服务器。数据顺序为 x1, y1, z1, x2, y2, z2,…… x10,y10,z10
        /// </summary>
        [StringLength(50)]
        public string Magnetic10 { get; set; }
        /// <summary>
        /// 检测算法的 3 轴基线值，顺序为 x,y,z
        /// </summary>
        [StringLength(20)]
        public string MagneticXYZ { get; set; }
        /// <summary>
        /// 磁场信号的采样平滑值，数据顺序为 x,y,z。
        /// </summary>
        [StringLength(20)]
        public string SmoothXYZ { get; set; }
        /// <summary>
        /// 雷达前三组数据XYZ
        /// </summary>
        [StringLength(20)]
        public string RadarDataXYZ { get; set; }
        /// <summary>
        /// 雷达检测到的距离值（mm）
        /// </summary>
        public int? Distance { get; set; }
        /// <summary>
        /// 车位状态：00 无车，01 有车，02 等待激活，03 强磁
        /// </summary>
        public int? BerthStatus { get; set; }
        /// <summary>
        /// 记录添加时间
        /// </summary>
        public DateTime? AddTime { get; set; }
    }
}
