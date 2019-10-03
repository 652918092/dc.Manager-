using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dc.Haiyakj.Communication
{
    public static class UdpCommunication
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public static string _IpAddress = string.Empty;
        /// <summary>
        /// 端口号
        /// </summary>
        public static int _Port = 8899;
        /// <summary>
        /// UDP通讯端
        /// </summary>
        public static UdpClient _UpdServer = null;
        /// <summary>
        /// UDP服务是否已初始化开启
        /// </summary>
        public static bool _IsUdpInit = false;
        /// <summary>
        /// 存储数据对接的设备IP地址及端口
        /// </summary>
        public static List<IPEndPoint> _RemotePointList = new List<IPEndPoint>();
        /// <summary>
        /// 数据帧的开始标记符
        /// </summary>
        public const string CmmStartFlag = "BE,";
        /// <summary>
        /// 数据帧的结束标记符
        /// </summary>
        public const string CmmEndFlag = "\r\n";
        /// <summary>
        /// UDP连接监测
        /// </summary>
        public static void ReConnMonitorUPD()
        {
            try
            {
                _UpdServer = new UdpClient(new IPEndPoint(IPAddress.Any, _Port));
                WriteLog.WriteIPConnect("UDP已重新连接>>>>");
            }
            catch (SocketException ex)
            {
                WriteLog.WriteIPConnect("UDP通讯监控中...");
            }
        }
        /// <summary>
        /// 初始化UPD通讯监听
        /// </summary>
        public static bool InitUpdServer()
        {
            bool isOpen = false;
            if (_UpdServer != null)
            {
                _UpdServer.Close();
                _UpdServer = null;
            }
            try
            {
                _UpdServer = new UdpClient(new IPEndPoint(IPAddress.Any, _Port));
                isOpen = true;
                WriteLog.WriteInfo("UPD通讯服务已开启，监听端口：" + _Port);
            }
            catch(SocketException ex)
            {
                WriteLog.WriteError("初始化失败，UPD监听异常：" + ex.Message);
            }
            return isOpen;
        }
        /// <summary>
        /// UDP发送数据
        /// </summary>
        /// <param name="msg">发送的数据</param>
        /// <param name="remotePoint">发送的IP地址及端口号</param>
        /// <returns></returns>
        public static bool UpdSendMessage(string msg, IPEndPoint remotePoint)
        {
            bool bRet = false;
            try
            {
                byte[] bSend = System.Text.Encoding.Default.GetBytes(msg);
                for (int i = 0; i < _RemotePointList.Count; i++)
                {
                    if (remotePoint.ToString() != _RemotePointList[i].ToString())
                        continue;
                    try { 
                        IAsyncResult iarSend = _UpdServer.BeginSend(bSend, bSend.Length, _RemotePointList[i], null, null);
                        int sendCount = _UpdServer.EndSend(iarSend);
                        bRet = sendCount > 0 ? true : false;
                        WriteLog.WriteSendLog(string.Format("向{0}发送了数据：{1}", _RemotePointList[i], msg));
                        break;
                    }
                    catch (SocketException ex)
                    {
                        WriteLog.WriteError(string.Format("向{0}发送了数据：【{1}】时异常，异常信息{2}", _RemotePointList[i], msg, ex.Message));
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLog.WriteError("发送UDP数据异常：" + ex.Message);
            }
            return bRet;
        }
    }
}
