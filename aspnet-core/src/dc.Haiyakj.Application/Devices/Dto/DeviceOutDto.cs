using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Devices.Dto
{
    public class DeviceOutDto
    {
    }
    /// <summary>
    ///执行指令发送回复的Dto
    /// </summary>
    public class SendCommandDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 指令的时间戳
        /// </summary>
        public string TimeStamp { get; set; }
    }
    
    public class CheckCommandReviceDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 指令执行情况: 0指令发送不成功   1指令已发送   2指令已回复
        /// </summary>
        public int? CmdStatus { get; set; }
        /// <summary>
        /// 指令回复的内容
        /// </summary>
        public string ReviceMsg { get; set; }
    }
}
