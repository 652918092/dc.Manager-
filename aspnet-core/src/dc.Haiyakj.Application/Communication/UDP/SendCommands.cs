using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Communication
{
    /// <summary>
    /// 数据下发或数据回复命令帧组成类
    /// </summary>
    public class SendCommands
    {
        /// <summary>
        /// 组成数据帧的帧头
        /// </summary>
        /// <param name="message">设备ID(10)+命令字(4)+时间戳(15)+消息内容(n)=》组成的字符串</param>
        /// <returns>数据帧的帧头</returns>
        private static string GetDataFrameHead(string message)
        {
            return string.Format("{0}{1:d3},", UdpCommunication.CmmStartFlag, message.Length);
        }
        #region 设备主动上传的回复帧组成
        /// <summary>
        /// 组成入网请求回复帧（回复设备请求入网的回复帧）
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="tstamp">入网请求帧的时间戳</param>
        /// <param name="seqno">帧序号</param>
        /// <param name="isSuccess">入网是否成功</param>
        /// <returns>入网请求回复帧</returns>
        public static string RAccessNetwork(string deviceNo, string tstamp, string seqno, bool isSuccess)
        {
            string cmd = string.Format("{0},81,{1},{2},{3},", deviceNo, tstamp, seqno, isSuccess ? "00" : "01");
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 泊位状态检测事件回复帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="tstamp">上传命令的时间戳</param>
        /// <param name="seqno">帧序号</param>
        /// <returns>泊位状态检测回复帧</returns>
        public static string RStateDetection(string deviceNo, string tstamp, string seqno)
        {
            string cmd = string.Format("{0},82,{1},{2},", deviceNo, tstamp, seqno);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设备心跳回复帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="tstamp">上传命令的时间戳</param>
        /// <param name="seqno">帧序号</param>
        /// <returns>设备心跳回复帧</returns>
        public static string RDeviceHeartbeat(string deviceNo, string tstamp, string seqno)
        {
            string cmd = string.Format("{0},83,{1},{2},", deviceNo, tstamp, seqno);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 自检测异常报警数据回复帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="tstamp">上传命令的时间戳</param>
        /// <returns>自检测异常报警数据回复帧</returns>
        public static string RSelfCheckingAlarm(string deviceNo, string tstamp)
        {
            string cmd = string.Format("{0},84,{1},", deviceNo, tstamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 传感器波动数据回复帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="tstamp">上传命令的时间戳</param>
        /// <returns>传感器波动数据回复帧</returns>
        public static string RSensorFluctuation(string deviceNo, string tstamp)
        {
            string cmd = string.Format("{0},85,{1},", deviceNo, tstamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        #endregion 设备主动上传的回复帧组成

        #region 下发设备帧组成
        /// <summary>
        /// 时间同步数据下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <returns>时间同步下发帧</returns>
        public static string STimeSync(string deviceNo, string timeStamp)
        {
            string cmd = string.Format("{0},87,{1},", deviceNo.PadLeft(9, '0'), timeStamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设备基本参数查询/设置下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <param name="operateType">操作类型：1:查询，2:配置</param>
        /// <param name="msg">消息内容(不包含操作类型)</param>
        /// <returns>设备基本参数查询/设置下发帧</returns>
        public static string SDeviceParamSetOrQuery(string deviceNo, string timeStamp, int operateType, string msg)
        {
            string cmd = string.Format("{0},88,{1},{2:d2},{3},", deviceNo.PadLeft(9, '0'), timeStamp, operateType, msg);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设备激活功能下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <returns>设备激活功能下发帧</returns>
        public static string SDeviceActivate(string deviceNo, string timeStamp)
        {
            string cmd = string.Format("{0},89,{1},", deviceNo.PadLeft(9, '0'), timeStamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设定设备的客户端IP及Port下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <param name="strIPAndPort">IP地址及端口号(格式：192.168.1.1,5000)</param>
        /// <returns>设定设备的客户端IP及Port下发帧</returns>
        public static string SDeviceIPEndPoint(string deviceNo, string timeStamp, string strIPAndPort)
        {
            string cmd = string.Format("{0},A1,{1},{2},", deviceNo.PadLeft(9, '0'), timeStamp, strIPAndPort);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设备检测参数查询下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <returns>设备检测参数查询下发帧</returns>
        public static string SDeviceDetectionParamQuery(string deviceNo, string timeStamp)
        {
            string cmd = string.Format("{0},A4,{1},", deviceNo.PadLeft(9, '0'), timeStamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 设备检测参数设置下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <param name="msg">消息内容</param>
        /// <returns>设备检测参数设置下发帧</returns>
        public static string SDeviceDetectionParamSet(string deviceNo, string timeStamp, string msg)
        {
            string cmd = string.Format("{0},A5,{1},{2},", deviceNo.PadLeft(9, '0'), timeStamp, msg);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 检测板重新标定下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <returns></returns>
        public static string STestingBoardAgainSet(string deviceNo, string timeStamp)
        {
            string cmd = string.Format("{0},A6,{1},", deviceNo.PadLeft(9,'0'), timeStamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        /// <summary>
        /// 重启检测板下发帧组成
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="timeStamp">时间戳(如：20190928110000 即yyyyMMddHHmmss)</param>
        /// <returns></returns>
        public static string SRestartTestingBoard(string deviceNo, string timeStamp )
        {
            string cmd = string.Format("{0},A7,{1},", deviceNo.PadLeft(9, '0'), timeStamp);
            cmd = GetDataFrameHead(cmd) + cmd + UdpCommunication.CmmEndFlag;
            return cmd;
        }
        #endregion 下发设备帧组成
    }
}
