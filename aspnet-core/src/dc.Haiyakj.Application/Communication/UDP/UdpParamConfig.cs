using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Communication
{
    public class UdpParamConfig: ISingletonDependency
    {
        /// <summary>
        /// UDP通讯绑定的IP地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// UDP通讯绑定的端口
        /// </summary>
        public string Port { get; set; }
    }
}
