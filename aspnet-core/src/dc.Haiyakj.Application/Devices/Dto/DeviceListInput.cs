using dc.Haiyakj.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Devices.Dto
{
    public class DeviceListInput: PagedAndSortedInputDto
    {
        /// <summary>
        /// 添加时间开始时间
        /// </summary>
        public DateTime? CreationDateStart { get; set; }
        /// <summary>
        /// 添加时间结束时间
        /// </summary>
        public DateTime? CreationDateEnd { get; set; }
        /// <summary>
        /// 筛选设备在线状态: 0未激活(初始化),  1在线,  2离线,
        /// </summary>
        public List<int> DeviceStatus { get; set; }
        /// <summary>
        /// 筛选综合状态：0 无车，1 有车，2 等待激活，3 初始化中, -1全部
        /// </summary>
        public List<int> SynStatus { get; set; }
        /// <summary>
        /// 搜索字符串(设备编号)
        /// </summary>
        public String SearchValue { get; set; }
        /// <summary>
        /// 升序 or  倒序
        /// </summary>
        [Required]
        public String AscOrDesc { get; set; }
    }
    public class DeviceListToExcelInput
    {
        /// <summary>
        /// 添加时间开始时间
        /// </summary>
        public DateTime? CreationDateStart { get; set; }
        /// <summary>
        /// 添加时间结束时间
        /// </summary>
        public DateTime? CreationDateEnd { get; set; }
        /// <summary>
        /// 筛选设备在线状态: 0未激活(初始化),  1在线,  2离线,
        /// </summary>
        public List<int> DeviceStatus { get; set; }
        /// <summary>
        /// 筛选综合状态：0 无车，1 有车，2 等待激活，3 初始化中, -1全部
        /// </summary>
        public List<int> SynStatus { get; set; }
        /// <summary>
        /// 搜索字符串(设备编号)
        /// </summary>
        public String SearchValue { get; set; }
        /// <summary>
        /// 升序 or  倒序
        /// </summary>
        [Required]
        public String AscOrDesc { get; set; }
        /// <summary>
        /// 排序条件
        /// </summary>
        public String Sorting { get; set; }
    }
}
