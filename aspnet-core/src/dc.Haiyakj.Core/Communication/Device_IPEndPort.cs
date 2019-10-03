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
/// 设备IP映射表
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("Device_IPEndPort")]
    public class Device_IPEndPort: Entity, IHasModificationTime, IHasCreationTime
    {
        /// <summary>
        /// 设备绑定
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// IP及端口号
        /// </summary>
        [StringLength(50)]
        public string IpEndPort { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }
        /// <summary>
        /// 是否还在使用
        /// </summary>
        public bool IsUseing { get; set; }
    }
}
