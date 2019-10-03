using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj
{
    public class WriteLog
    {
        private static int _SaveDay = -5;
        /// <summary>
        /// 写日志(UPD数据接收类日志)
        /// </summary>
        /// <param name="value">日志内容</param>
        /// <param name="filePath">文件存放的路径，不包含文件名</param>
        public static void WriteReceiveLog(object value, string filePath = "")
        {
            try
            {
                string fullpath = "";
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fullpath = filePath;
                }
                else
                {
                    fullpath = AppDomain.CurrentDomain.BaseDirectory + "MyLog\\";
                }
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string logfile = fullpath + DateTime.Today.ToString("yyyy-MM-dd") + "_Receive.log";
                File.Delete(fullpath + DateTime.Today.AddDays(_SaveDay).ToString("yyyy-MM-dd") + "_Receive.log");

                using (FileStream file = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes("\r\n" + DateTime.Now + ": " + value.ToString());
                    file.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// 写日志(UPD通讯接入地址集合日志)
        /// </summary>
        /// <param name="value">日志内容</param>
        /// <param name="filePath">文件存放的路径，不包含文件名</param>
        public static void WriteIPConnect(object value, string filePath = "")
        {
            try
            {
                string fullpath = "";
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fullpath = filePath;
                }
                else
                {
                    fullpath = AppDomain.CurrentDomain.BaseDirectory + "MyLog\\";
                }
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string logfile = fullpath + DateTime.Today.ToString("yyyy-MM-dd") + "_IP.log";
                File.Delete(fullpath + DateTime.Today.AddDays(_SaveDay).ToString("yyyy-MM-dd") + "_IP.log");

                using (FileStream file = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes("\r\n" + DateTime.Now + ": " + value.ToString());
                    file.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// 写日志(UDP信息发送类日志)
        /// </summary>
        /// <param name="value">日志内容</param>
        /// <param name="filePath">文件存放的路径，不包含文件名</param>
        public static void WriteSendLog(object value, string filePath = "")
        {
            try
            {
                string fullpath = "";
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fullpath = filePath;
                }
                else
                {
                    fullpath = AppDomain.CurrentDomain.BaseDirectory + "MyLog\\";
                }
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string logfile = fullpath + DateTime.Today.ToString("yyyy-MM-dd") + "_Send.log";
                File.Delete(fullpath + DateTime.Today.AddDays(_SaveDay).ToString("yyyy-MM-dd") + "_Send.log");

                using (FileStream file = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes("\r\n" + DateTime.Now + ": " + value.ToString());
                    file.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// 写日志(信息类日志)
        /// </summary>
        /// <param name="value">日志内容</param>
        /// <param name="filePath">文件存放的路径，不包含文件名</param>
        public static void WriteInfo(object value, string filePath = "")
        {
            try
            {
                string fullpath = "";
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fullpath = filePath;
                }
                else
                {
                    fullpath = AppDomain.CurrentDomain.BaseDirectory + "MyLog\\";
                }
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string logfile = fullpath + DateTime.Today.ToString("yyyy-MM-dd") + "_Info.log";
                File.Delete(fullpath + DateTime.Today.AddDays(_SaveDay).ToString("yyyy-MM-dd") + "_Info.log");

                using (FileStream file = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes("\r\n" + DateTime.Now + ": " + value.ToString());
                    file.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// 写日志(错误类日志)
        /// </summary>
        /// <param name="value">日志内容</param>
        /// <param name="filePath">文件存放的路径，不包含文件名</param>
        public static void WriteError(object value, string filePath = "")
        {
            try
            {
                string fullpath = "";
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fullpath = filePath;
                }
                else
                {
                    fullpath = AppDomain.CurrentDomain.BaseDirectory + "MyLog\\";
                }
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                string logfile = fullpath + DateTime.Today.ToString("yyyy-MM-dd") + "_Error.log";
                File.Delete(fullpath + DateTime.Today.AddDays(_SaveDay).ToString("yyyy-MM-dd") + "_Error.log");

                using (FileStream file = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes("\r\n" + DateTime.Now + ": " + value.ToString());
                    file.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
