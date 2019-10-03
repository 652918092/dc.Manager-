using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Castle.Core.Logging;
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
    public class AsyncBackgroundTask : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        public ILogger Logger { get; set; }
        //UDP通讯配置参数
        private UdpParamConfig _UdpConfig;
        // 数据长度所点的字符长度XX,（因3字节不够改为4）
        private const int DataLen = 4;
        //UDP通讯是否已初始化
        private bool isInit = false;
        /// <summary>
        /// 设备IP信息绑定实体映射表
        /// </summary>
        private readonly IRepository<Device_IPEndPort> _deviceIPEndPort;
        /// <summary>
        /// 发送、接收指令记录实体映射表
        /// </summary>
        private readonly IRepository<Command_Log> _commandLog;
        /// <summary>
        /// 设备信息实体映射表
        /// </summary>
        private readonly IRepository<Device_Info> _deviceInfo;
        /// <summary>
        /// 设备心跳实体映射表
        /// </summary>
        private readonly IRepository<Heartbeat_Log> _heartBeatLog;
        /// <summary>
        /// 传感器波动数据实体映射表
        /// </summary>
        private readonly IRepository<SensorWave_Log> _sensorWaveLog;
        /// <summary>
        /// 设备检测参数数据实体映射表
        /// </summary>
        private readonly IRepository<Device_CheckParam> _deviceCheckParam;

        public AsyncBackgroundTask(AbpTimer timer)
            : base(timer)
        {
            Timer.Period = 3000;
            Logger = NullLogger.Instance;

            _deviceIPEndPort = IocManager.Instance.Resolve<IRepository<Device_IPEndPort>>();
            _commandLog = IocManager.Instance.Resolve<IRepository<Command_Log>>();
            _deviceInfo = IocManager.Instance.Resolve<IRepository<Device_Info>>();
            _heartBeatLog = IocManager.Instance.Resolve<IRepository<Heartbeat_Log>>();
            _sensorWaveLog = IocManager.Instance.Resolve<IRepository<SensorWave_Log>>();
            _deviceCheckParam = IocManager.Instance.Resolve<IRepository<Device_CheckParam>>();
            //初始化UDP通讯配置
            _UdpConfig = IocManager.Instance.Resolve<UdpParamConfig>();
            int.TryParse(_UdpConfig.Port, out UdpCommunication._Port);
            UdpCommunication._IpAddress = _UdpConfig.IpAddress;
            //初始化UDP通讯监测
            UdpCommunication._IsUdpInit = UdpCommunication.InitUpdServer();


        }
        protected async override void DoWork()
        {
            if (!isInit)
            {
                await Init();
            }

            Timer.Period = 1000 * 30;
            //UdpCommunication.UpdSendMessage("hello world!");
            await Task.Run(() =>
            {
                //UDP连接监测
                UdpCommunication.ReConnMonitorUPD();

                //如果未初始化则继续初始化UDP通讯
                if (!UdpCommunication._IsUdpInit)
                {
                    UdpCommunication._IsUdpInit = UdpCommunication.InitUpdServer();
                    if (UdpCommunication._IsUdpInit)
                    {
                        Task.Run(() => { AsynRecive(); });
                    }
                }
            });
        }
        /// <summary>
        /// 从数据库中读取已连接过的并且还在使用的设备IP放到内存中,如果UPD测试已打开则开始接收处理线程
        /// </summary>
        /// <returns></returns>
        private async Task Init()
        {
            isInit = true;
            try
            {
                var ipList = await _deviceIPEndPort.GetAllListAsync(d => d.IsUseing);
                foreach (Device_IPEndPort model in ipList)
                {
                    System.Net.IPAddress IPadr = System.Net.IPAddress.Parse(model.IpEndPort.Split(':')[0]);//先把string类型转换成IPAddress类型
                    System.Net.IPEndPoint EndPoint = new System.Net.IPEndPoint(IPadr, int.Parse(model.IpEndPort.Split(':')[1]));
                    if (!UdpCommunication._RemotePointList.Contains(EndPoint))
                    {
                        UdpCommunication._RemotePointList.Add(EndPoint);
                    }
                }
            }
            catch { }
            //如果UDP通讯已初始化则开启UDP数据接收线程
            if (UdpCommunication._IsUdpInit)
            {
                Logger.Info("已初始化UDP通讯");
                await Task.Run(() => { AsynRecive(); });
            }
        }
        /// <summary>
        /// 数据接收线程监控
        /// </summary>
        public void AsynRecive()
        {
            while (true)
            {
                try
                {
                    //异步数据接收处理
                    IAsyncResult result = UdpCommunication._UpdServer.BeginReceive(UdpReceiveDataCallback, UdpCommunication._UpdServer);
                    while (result.IsCompleted == false)
                    {
                        Thread.Sleep(50);
                    }
                }
                catch (SocketException ex)
                {
                    WriteLog.WriteError("数据接收异常：" + ex.Message);
                    Logger.Error("UDP通讯数据接收异常：" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 回调函数，用于异步接收数据
        /// </summary>
        /// <param name="iar"></param>
        private void UdpReceiveDataCallback(IAsyncResult iar)
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
            UdpCommunication._UpdServer = (UdpClient)iar.AsyncState;
            string message = string.Empty;
            try
            {
                byte[] receiveBytes = UdpCommunication._UpdServer.EndReceive(iar, ref remotePoint);

                message = Encoding.Default.GetString(receiveBytes, 0, receiveBytes.Length);
            }
            catch (System.Net.Sockets.SocketException ex)   //如果对方的UDP通讯已断开则会引发异常？
            {
                return;
            }
            //开启数据处理线程
            Task t = new Task(() => { DealDataFrame(message, remotePoint); });
            t.Start();

            //将接收到的IP地址存入到内存中
            if (!UdpCommunication._RemotePointList.Contains(remotePoint))
            {
                UdpCommunication._RemotePointList.Add(remotePoint);
                WriteLog.WriteIPConnect(string.Format("地址【{0}】加入通讯", remotePoint));
            }
            WriteLog.WriteReceiveLog(string.Format("来自{0}:{1}的数据：{2}", remotePoint.Address, remotePoint.Port, message));
        }
        /// <summary>
        /// 合法帧判定
        /// </summary>
        /// <param name="message">收到的数据</param>
        /// <param name="remotePoint">来自于哪个IP及端口</param>
        public void DealDataFrame(string message, IPEndPoint remotePoint)
        {
            try
            {
                int posStart = message.IndexOf(UdpCommunication.CmmStartFlag);
                if (posStart > 0)
                {
                    //去掉前面不需要的数据
                    message = message.Remove(0, posStart);
                }
                else if (posStart < 0)
                {
                    //如果找不到开始符则放弃处理
                    WriteLog.WriteReceiveLog(string.Format("数据帧【{0}】不合法", message));
                    return;
                }
                if (message.Length < UdpCommunication.CmmStartFlag.Length + DataLen - 1)
                {
                    //未得到帧的数据长度时退出此次循环（数据长度：设备ID(10)+命令字(3)+时间戳(15)+消息内容(n)）
                    WriteLog.WriteReceiveLog(string.Format("数据帧【{0}】长度不足11", message));
                    return;
                }
                //取得数据长度
                int dataLenth = 0;
                if (int.TryParse(message.Substring(UdpCommunication.CmmStartFlag.Length, 3), out dataLenth))
                {
                    //数据帧的长度 = 起始字长度 + 数据位的帧长度 + 数据长度 + 结束符长度
                    int dataFrameLen = UdpCommunication.CmmStartFlag.Length + DataLen + dataLenth + UdpCommunication.CmmEndFlag.Length;
                    if (message.Length < dataFrameLen)
                    {
                        WriteLog.WriteReceiveLog(string.Format("数据帧【{0}】长度不足22", message));
                        return;
                    }
                    string endStr = message.Substring(dataFrameLen - UdpCommunication.CmmEndFlag.Length, UdpCommunication.CmmEndFlag.Length);
                    if (endStr == UdpCommunication.CmmEndFlag)
                    {
                        //取得合法的帧
                        string dataFrame = message.Substring(0, dataFrameLen);
                        //合法帧处理
                        Task.Run(() => { RevCommandDeal(dataFrame, remotePoint); });
                    }
                }
                else
                {
                    WriteLog.WriteReceiveLog(string.Format("数据帧【{0}】取数据长度出错", message));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("处理接收数据异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 处理接收到的正确指令
        /// </summary>
        /// <param name="revCommand">接收到的正确指令</param>
        /// <param name="remotePoint">IP址址及端口</param>
        public async void RevCommandDeal(string revCommand, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = revCommand.Split(',');
                string deviceNo = szCommand[2];  //设备号

                //设备号通讯地址绑定
                await UpdateOrAddIPEndPointToDataBase(deviceNo, remotePoint);
                
                switch (szCommand[3])
                {
                    #region 由设备主动上传
                    case "01":  //设备入网请求
                        await AddCommandLog(revCommand);//插入设备主动上传接收到的指令到数据库中
                        await AccessNetwork(deviceNo, revCommand, remotePoint);
                        break;
                    case "02": //泊位状态检测事件
                        await AddCommandLog(revCommand);//插入设备主动上传接收到的指令到数据库中
                        await DeviceStateChange(deviceNo, revCommand, remotePoint);
                        break;
                    case "03": //设备心跳
                        await AddCommandLog(revCommand);//插入设备主动上传接收到的指令到数据库中
                        await DeviceHeartbeat(deviceNo, revCommand, remotePoint);
                        break;
                    case "04": //自检测异常报警数据
                        await AddCommandLog(revCommand);//插入设备主动上传接收到的指令到数据库中
                        await SelfCheckingAlarm(deviceNo, revCommand, remotePoint);
                        break;
                    case "05": //传感器波动数据
                        await AddCommandLog(revCommand);//插入设备主动上传接收到的指令到数据库中
                        await SensorFluctuation(deviceNo, revCommand, remotePoint);
                        break;
                    #endregion 由设备主动上传
                    #region 设备应答上传数据
                    case "07": //设备时间同步
                        await TimeSync(deviceNo, revCommand, remotePoint);
                        break;
                    case "08": //设备基本参数查询/设置
                        await DeviceParamSetOrQuery(deviceNo, revCommand, remotePoint);
                        break;
                    case "09": //设备激活功能
                        await DeviceActivate(deviceNo, revCommand, remotePoint);
                        break;
                    case "21": //设定设备的客户端 IP/PORT
                        await SetDeviceIPEndPoint(deviceNo, revCommand, remotePoint);
                        break;
                    case "24": //设备检测参数查询
                        await DeviceDetectionParamQuery(deviceNo, revCommand, remotePoint);
                        break;
                    case "25": //设备检测参数设置
                        await DeviceDetectionParamSet(deviceNo, revCommand, remotePoint);
                        break;
                    case "26": //检测板重新标定
                        await TestingBoardAgainSet(deviceNo, revCommand, remotePoint);
                        break;
                    case "27": //重启检测板
                        await RestartTestingBoard(deviceNo, revCommand, remotePoint);
                        break;
                        #endregion 设备应答上传数据
                }
            }
            catch (Exception ex)
            {
                Logger.Error("处理合法帧时异常,异常信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 从接收到的帧中提取消息内容
        /// </summary>
        /// <param name="reviceMsg">接收到的完整帧</param>
        /// <returns>消息内容部分</returns>
        private string GetMsgContent(string reviceMsg)
        {
            string msgContent = "";
            try
            {
                //起始字  数据长度    设备ID    命令字  时间戳     消息内容      结束字
                // 3字节   3字节      10 字节   3字节    15字节    n字节(可变)    2字节
                //基本帧长度（不包含消息内容）
                int baseFrameLen = UdpCommunication.CmmStartFlag.Length + DataLen + 10 + 3 + 15 + UdpCommunication.CmmEndFlag.Length;
                if (reviceMsg.Length > baseFrameLen)
                {
                    msgContent = reviceMsg.Substring(baseFrameLen - UdpCommunication.CmmEndFlag.Length, reviceMsg.Length - baseFrameLen);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return msgContent;
        }
        /// <summary>
        /// 接收到的指令插入到数据库记录中 
        /// </summary>
        /// <param name="reviceMsg">接收到的数据帧</param>
        /// <returns></returns>
        private async Task AddCommandLog(string reviceMsg)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string deviceNo = szCommand[2];  //设备号
                string tstamp = szCommand[4];   //时间戳

                //插入数据记录帧
                Command_Log cmdLog = new Command_Log
                {
                    DeviceNo = deviceNo,
                    CommandType = szCommand[3],
                    Tstamp = tstamp,
                    SendMessage = GetMsgContent(reviceMsg),
                    AddTime = DateTime.Now,
                    SendType = 1,
                    CmdStatus = 1
                };
                await _commandLog.InsertAsync(cmdLog);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备号通讯地址绑定
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="remotePoint">UDP通讯地址</param>
        public async Task UpdateOrAddIPEndPointToDataBase(string deviceNo, IPEndPoint remotePoint)
        {
            try
            {
                var modelQuery = await _deviceIPEndPort.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelQuery == null)
                {
                    //数据库中该设备编号不存在则添加记录
                    Device_IPEndPort model = new Device_IPEndPort
                    {
                        DeviceNo = deviceNo,
                        IpEndPort = remotePoint.ToString(),
                        IsUseing = true
                    };
                    await _deviceIPEndPort.InsertAsync(model);
                }
                else if (modelQuery != null && modelQuery.IpEndPort != remotePoint.ToString())
                {
                    //数据库中该设备编号的IP地址已变更则更新IP地址的绑定
                    modelQuery.IsUseing = true;
                    modelQuery.IpEndPort = remotePoint.ToString();
                    await _deviceIPEndPort.UpdateAsync(modelQuery);
                }
                else if (modelQuery != null && modelQuery.IpEndPort == remotePoint.ToString() && modelQuery.IsUseing == false)
                {
                    //备用
                    modelQuery.IsUseing = true;
                    await _deviceIPEndPort.UpdateAsync(modelQuery);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("更新或添加设备IP绑定时出错：" + ex.Message);
            }
        }

        #region 设备主动上传数据处理
        /// <summary>
        /// 设备请求入网(设备上传)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">请求入网帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task AccessNetwork(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳
                string seqno = szCommand[5];    //帧序号(设备入网请求、泊位状态检测事件、设备心跳=》以上命令中才存在帧序号)
                //回复指令
                string command = "";
                //查询设备是否注册，如果未注册则请求入网失败
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelInfo == null)
                {
                    command = SendCommands.RAccessNetwork(deviceNo, tstamp, seqno, false);
                }
                else
                {
                    //获取消息部分内容
                    string message = GetMsgContent(reviceMsg);
                    string[] szMsg = message.Split(',');
                    //更新数据库设备信息
                    modelInfo.NbSignal = szMsg[1];
                    modelInfo.DevHealth = int.Parse(szMsg[2]);
                    modelInfo.BatVoltage = szMsg[3];
                    modelInfo.FwVer = szMsg[4];
                    modelInfo.HwVer = szMsg[5];
                    modelInfo.healthT = int.Parse(szMsg[6]);
                    modelInfo.DeviceStatus = 1;
                    modelInfo.StatusUpdateTime = DateTime.Now;
                    //更新设备信息
                    await _deviceInfo.UpdateAsync(modelInfo);
                    //获取回复指令
                    command = SendCommands.RAccessNetwork(deviceNo, tstamp, seqno, true);
                }
                bool bRet = UdpCommunication.UpdSendMessage(command, remotePoint);
                if (bRet)
                {
                    //更新指令回复信息
                    var cmdModel = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                    if (cmdModel != null)
                    {
                        string rMsg = GetMsgContent(command);
                        cmdModel.ResultMessage = rMsg;
                        cmdModel.CmdStatus = 2;
                        await _commandLog.UpdateAsync(cmdModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 泊位状态检测事件(设备上传)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">泊位状态检测数据帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceStateChange(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳
                string seqno = szCommand[5];    //帧序号(设备入网请求、泊位状态检测事件、设备心跳=》以上命令中才存在帧序号)

                //查询设备是否存在，存在则更新设备状态
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelInfo != null)
                {
                    //获取消息部分内容
                    string message = GetMsgContent(reviceMsg);
                    string[] szMsg = message.Split(',');
                    //更新数据库设备信息
                    modelInfo.MagStatus = int.Parse(szMsg[1]);
                    modelInfo.RadaStatus = int.Parse(szMsg[2]);
                    modelInfo.SynStatus = int.Parse(szMsg[3]);
                    modelInfo.NbSignal = szMsg[4];
                    modelInfo.Temperature = int.Parse(szMsg[5]);
                    modelInfo.MagneticXYZ = string.Format("{0},{1},{2}", szMsg[6], szMsg[7], szMsg[8]);
                    modelInfo.RadarXYZ = string.Format("{0},{1},{2}", szMsg[9], szMsg[10], szMsg[11]);
                    modelInfo.Distance = int.Parse(szMsg[12]);
                    modelInfo.BatVoltage = szMsg[13];
                    modelInfo.FwVer = szMsg[14];
                    modelInfo.HwVer = szMsg[15];
                    modelInfo.DevHealth = int.Parse(szMsg[16]);
                    modelInfo.healthT = int.Parse(szMsg[17]);
                    modelInfo.DeviceStatus = 1;
                    modelInfo.StatusUpdateTime = DateTime.Now;
                    //更新设备信息
                    await _deviceInfo.UpdateAsync(modelInfo);
                }
                //回复指令
                string command = SendCommands.RStateDetection(deviceNo, tstamp, seqno);
                bool bRet = UdpCommunication.UpdSendMessage(command, remotePoint);
                if (bRet)
                {
                    //更新指令回复信息
                    var cmdModel = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                    if (cmdModel != null)
                    {
                        string rMsg = GetMsgContent(command);
                        cmdModel.ResultMessage = rMsg;
                        cmdModel.CmdStatus = 2;
                        await _commandLog.UpdateAsync(cmdModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备心跳(设备上传)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设备心跳数据帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceHeartbeat(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳
                string seqno = szCommand[5];    //帧序号(设备入网请求、泊位状态检测事件、设备心跳=》以上命令中才存在帧序号)

                //查询设备是否存在，存在则更新设备状态
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelInfo != null)
                {
                    //获取消息部分内容
                    string message = GetMsgContent(reviceMsg);
                    string[] szMsg = message.Split(',');
                    //如果设备存在则插入设备心跳记录
                    Heartbeat_Log model = new Heartbeat_Log
                    {
                        DeviceNo = deviceNo,
                        Tstamp = tstamp,
                        DeviceState = int.Parse(szMsg[1]),
                        NbSignal = szMsg[2],
                        Temperature = int.Parse(szMsg[3]),
                        BatVoltage = szMsg[4],
                        FwVer = szMsg[5],
                        HwVer = szMsg[6],
                        DevHealth = int.Parse(szMsg[7]),
                        HbeatT = int.Parse(szMsg[8]),
                        NbSendN = int.Parse(szMsg[9]),
                        BbRecvN = int.Parse(szMsg[10]),
                        SampleN = int.Parse(szMsg[11]),
                        RebootN = int.Parse(szMsg[12]),
                        NbRebootN = int.Parse(szMsg[13]),
                        NoackN = int.Parse(szMsg[14]),
                        AddTime = DateTime.Now
                    };
                    await _heartBeatLog.InsertAsync(model);
                    //更新设备信息
                    modelInfo.DeviceStatus = 1;
                    modelInfo.NbSignal = szMsg[2];
                    modelInfo.Temperature = int.Parse(szMsg[3]);
                    modelInfo.BatVoltage = szMsg[4];
                    modelInfo.FwVer = szMsg[5];
                    modelInfo.HwVer = szMsg[6];
                    modelInfo.DevHealth = int.Parse(szMsg[7]);
                    modelInfo.healthT = int.Parse(szMsg[8]);
                    modelInfo.StatusUpdateTime = DateTime.Now;
                    await _deviceInfo.UpdateAsync(modelInfo);

                }
                //回复指令
                string command = SendCommands.RDeviceHeartbeat(deviceNo, tstamp, seqno);
                bool bRet = UdpCommunication.UpdSendMessage(command, remotePoint);
                if (bRet)
                {
                    //更新指令回复信息
                    var cmdModel = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp && c.CommandType == szCommand[3]);
                    if (cmdModel != null)
                    {
                        string rMsg = GetMsgContent(command);
                        cmdModel.ResultMessage = rMsg;
                        cmdModel.CmdStatus = 2;
                        await _commandLog.UpdateAsync(cmdModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 自检测异常报警数据(设备上传)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">自检测异常报警数据帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task SelfCheckingAlarm(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelInfo != null)
                {
                    //获取消息部分内容
                    string message = GetMsgContent(reviceMsg);
                    string[] szMsg = message.Split(',');

                    //更新设备报警数据
                    modelInfo.NbSignalCode = int.Parse(szMsg[0]);
                    modelInfo.SensorCode = int.Parse(szMsg[1]);
                    modelInfo.FlashCode = int.Parse(szMsg[2]);
                    modelInfo.BatteryCode = int.Parse(szMsg[3]);
                    modelInfo.StatusUpdateTime = DateTime.Now;

                    await _deviceInfo.UpdateAsync(modelInfo);

                }
                //回复指令
                string command = SendCommands.RSelfCheckingAlarm(deviceNo, tstamp);
                bool bRet = UdpCommunication.UpdSendMessage(command, remotePoint);
                if (bRet)
                {
                    //更新指令回复信息
                    var cmdModel = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                    if (cmdModel != null)
                    {
                        string rMsg = GetMsgContent(command);
                        cmdModel.ResultMessage = rMsg;
                        cmdModel.CmdStatus = 2;
                        await _commandLog.UpdateAsync(cmdModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 传感器波动数据(设备上传)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">传感器波动数据帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task SensorFluctuation(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                if (modelInfo != null)
                {
                    //获取消息部分内容
                    string message = GetMsgContent(reviceMsg);
                    string[] szMsg = message.Split(',');

                    SensorWave_Log model = new SensorWave_Log
                    {
                        DeviceNo = deviceNo,
                        Tstamp = tstamp,
                        Magnetic10 = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", szMsg[0], szMsg[1], szMsg[2], szMsg[3], szMsg[4], szMsg[5], szMsg[6], szMsg[7], szMsg[8], szMsg[9]),
                        MagneticXYZ = string.Format("{0},{1},{2}", szMsg[10], szMsg[11], szMsg[12]),
                        SmoothXYZ = string.Format("{0},{1},{2}", szMsg[13], szMsg[14], szMsg[15]),
                        RadarDataXYZ = string.Format("{0},{1},{2}", szMsg[16], szMsg[17], szMsg[18]),
                        Distance = int.Parse(szMsg[19]),
                        BerthStatus = int.Parse(szMsg[20]),
                        AddTime = DateTime.Now
                    };
                    await _sensorWaveLog.InsertAsync(model);
                }
                //回复指令
                string command = SendCommands.RSensorFluctuation(deviceNo, tstamp);
                bool bRet = UdpCommunication.UpdSendMessage(command, remotePoint);
                if (bRet)
                {
                    //更新指令回复信息
                    var cmdModel = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                    if (cmdModel != null)
                    {
                        string rMsg = GetMsgContent(command);
                        cmdModel.ResultMessage = rMsg;
                        cmdModel.CmdStatus = 2;
                        await _commandLog.UpdateAsync(cmdModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        #endregion 设备主动上传数据处理

        #region 后台下发设备回复处理
        /// <summary>
        /// 下发时间同步后设备的回复
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">时间同步设备回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task TimeSync(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg;
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备基本参数查询/设置(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设备基本参数查询/设置回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceParamSetOrQuery(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    string[] szMsg = rMsg.Split(',');
                    //szMsg[1] 为执行结果，00为成功，01为错误
                    //查询
                    if (szMsg[0] == "01" && szMsg[1] == "00")
                    {
                        var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo);
                        if(modelInfo != null)
                        {
                            modelInfo.HbeatT = int.Parse(szMsg[2]);
                            modelInfo.healthT = int.Parse(szMsg[3]);
                            modelInfo.BatVoltage = szMsg[4];
                            modelInfo.StatusUpdateTime = DateTime.Now;
                            await _deviceInfo.UpdateAsync(modelInfo);
                        }
                    }
                    //设置(暂不处理)
                    else if (szMsg[0] == "02") { }

                    model.ResultMessage = rMsg;
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备激活功能(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设备激活功能回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceActivate(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg; //执行结果：00成功，01错误
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设定设备的客户端 IP/PORT(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设定设备的客户端 IP/PORT回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task SetDeviceIPEndPoint(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg; //IP地址及端口：180.101.147.115,5000，
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备检测参数查询(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设备检测参数查询回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceDetectionParamQuery(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //获取消息部分内容
                string rMsg = GetMsgContent(reviceMsg);
                string[] szMsg = rMsg.Split(',');

                //查询设备是否存在，存在则更新设备状态
                var model = await _deviceCheckParam.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo);
                if (model != null)
                {
                    model.NormalT = int.Parse(szMsg[0]);
                    model.FlunctT = int.Parse(szMsg[1]);
                    model.MagBigThresh = int.Parse(szMsg[2]);
                    model.MagMidThresh = int.Parse(szMsg[3]);
                    model.MagLittleThresh = int.Parse(szMsg[4]);
                    model.DowThresh = int.Parse(szMsg[5]);
                    model.XyzBaseLine = string.Format("{0},{1},{2}", szMsg[6], szMsg[7], szMsg[8]);
                    model.XyzNowValue = string.Format("{0},{1},{2}", szMsg[9], szMsg[10], szMsg[11]);
                    model.RadarBaseLine = string.Format("{0},{1},{2}", szMsg[12], szMsg[13], szMsg[14]);
                    model.RadarNowValue = string.Format("{0},{1},{2}", szMsg[15], szMsg[16], szMsg[17]);
                    model.MagStatus = int.Parse(szMsg[18]);
                    model.RadaStatus = int.Parse(szMsg[19]);
                    model.SynStatus = int.Parse(szMsg[20]);
                    model.Distance = int.Parse(szMsg[21]);
                    model.MagGain = int.Parse(szMsg[22]);
                    model.RadarGain = int.Parse(szMsg[23]);
                    model.AddTime = DateTime.Now;

                    await _deviceCheckParam.UpdateAsync(model);
                }
                else
                {
                    Device_CheckParam modelAdd = new Device_CheckParam
                    {
                        DeviceNo = deviceNo,
                        NormalT = int.Parse(szMsg[0]),
                        FlunctT = int.Parse(szMsg[1]),
                        MagBigThresh = int.Parse(szMsg[2]),
                        MagMidThresh = int.Parse(szMsg[3]),
                        MagLittleThresh = int.Parse(szMsg[4]),
                        DowThresh = int.Parse(szMsg[5]),
                        XyzBaseLine = string.Format("{0},{1},{2}", szMsg[6], szMsg[7], szMsg[8]),
                        XyzNowValue = string.Format("{0},{1},{2}", szMsg[9], szMsg[10], szMsg[11]),
                        RadarBaseLine = string.Format("{0},{1},{2}", szMsg[12], szMsg[13], szMsg[14]),
                        RadarNowValue = string.Format("{0},{1},{2}", szMsg[15], szMsg[16], szMsg[17]),
                        MagStatus = int.Parse(szMsg[18]),
                        RadaStatus = int.Parse(szMsg[19]),
                        SynStatus = int.Parse(szMsg[20]),
                        Distance = int.Parse(szMsg[21]),
                        MagGain = int.Parse(szMsg[22]),
                        RadarGain = int.Parse(szMsg[23]),
                        AddTime = DateTime.Now
                    };
                    await _deviceCheckParam.InsertAsync(modelAdd);
                }
                //更新指令信息
                var modelLog = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (modelLog != null)
                {
                    modelLog.ResultMessage = rMsg; 
                    modelLog.CmdStatus = 2;
                    await _commandLog.UpdateAsync(modelLog);
                }

                //如果设备离线，则改为在线
                var modelInfo = await _deviceInfo.FirstOrDefaultAsync(d => d.DeviceNo == deviceNo && d.DeviceStatus != 1);
                if(modelInfo!= null)
                {
                    modelInfo.DeviceStatus = 1;
                    await _deviceInfo.UpdateAsync(modelInfo);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 设备检测参数设置(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">设备检测参数设置回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task DeviceDetectionParamSet(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg; //执行结果：00成功，01错误
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 检测板重新标定(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">检测板重新标定回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task TestingBoardAgainSet(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg; //执行结果:00:成功，01:错误
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        /// <summary>
        /// 重启检测板(后台下发后设备的回复)
        /// </summary>
        /// <param name="deviceNo">设备编号</param>
        /// <param name="reviceMsg">重启检测板回复帧</param>
        /// <param name="remotePoint">数据来源的IP地址及端口</param>
        /// <returns></returns>
        public async Task RestartTestingBoard(string deviceNo, string reviceMsg, IPEndPoint remotePoint)
        {
            try
            {
                string[] szCommand = reviceMsg.Split(',');
                string tstamp = szCommand[4];   //时间戳

                //查询设备是否存在，存在则更新设备状态
                var model = await _commandLog.FirstOrDefaultAsync(c => c.DeviceNo == deviceNo && c.Tstamp == tstamp);
                if (model != null)
                {
                    //获取消息部分内容
                    string rMsg = GetMsgContent(reviceMsg);
                    model.ResultMessage = rMsg; //执行结果:00:成功，01:错误
                    model.CmdStatus = 2;
                    await _commandLog.UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        #endregion 后台下发设备回复处理
    }
}
