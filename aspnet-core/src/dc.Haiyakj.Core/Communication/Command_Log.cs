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
/// 指令下发、接收日志
/// </summary>
namespace dc.Haiyakj.Communication
{
    [Table("Command_Log")]
    public class Command_Log: Entity, IHasModificationTime
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [StringLength(9)]
        public string DeviceNo { get; set; }
        /// <summary>
        /// ===========命令类型==============
        /// 
        /// 设备主动上传====
        /// 设备入网请求：        接收：01    ；  发送：81
        /// 泊位状态检测事件：    接收：02    ；  发送：82
        /// 设备心跳：            接收：03    ；  发送：83
        /// 自检测异常报警数据：  接收：04    ；  发送：84
        /// 传感器波动数据：      接收：05    ；  发送：85
        /// 
        /// 后台主动下发=====
        /// 设备时间同步：            接收：07    ；  发送：87
        /// 设备基本参数查询/设置：   接收：08    ；  发送：88
        /// 设备激活：                接收：09    ；  发送：89
        /// 设定设备的客户端 IP/PORT：接收：21    ；  发送：A1
        /// 设备检测参数查询：        接收：24    ；  发送：A4
        /// 设备检测参数设置：        接收：25    ；  发送：A5
        /// 检测板重新标定：          接收：26    ；  发送：A6
        /// 重启检测板：              接收：27    ；  发送：A7
        /// 
        /// </summary>
        [StringLength(9)]
        public string CommandType { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [StringLength(20)]
        public string Tstamp { get; set; }
        /// <summary>
        /// 发送或接收的消息内容
        /// </summary>
        [StringLength(256)]
        public string SendMessage { get; set; }
        /// <summary>
        /// 回复的消息内容
        /// </summary>
        [StringLength(256)]
        public string ResultMessage { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? AddTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }
        /// <summary>
        /// 指令上下行类型（0：后台下发； 1：来自设备回复或主动上传）
        /// </summary>
        public int? SendType { get; set; }
        /// <summary>
        /// 指令状态（0：初始化下发； 1：指令已送达或已收到; 2:已回复）
        /// </summary>
        public int? CmdStatus { get; set; }

    }
}
