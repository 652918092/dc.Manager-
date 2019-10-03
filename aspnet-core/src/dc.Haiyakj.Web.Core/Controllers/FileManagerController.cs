using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Web.Models;
using dc.Haiyakj.Communication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Controllers
{
    [Route("api/[controller]/[action]")]
    public class FileManagerController: AbpProjectNameControllerBase
    {
        private AppFolders _appFolders;
        private readonly IRepository<Device_Info> _deviceInfo;
        public FileManagerController(IRepository<Device_Info> deviceInfo)
        {
            _appFolders = IocManager.Instance.Resolve<AppFolders>();
            this._deviceInfo = deviceInfo;
        }
        /// <summary>
        /// 上传Excel文件并返回数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> UploadPlanFiles()
        {
            string filePath = "";
            try
            {
                bool flag = false;
                var files = Request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    WriteLog.WriteInfo(string.Format("上传的表格文件:{0}", file.FileName));

                    filePath = string.Format("{0}/{1}{2}", this._appFolders.ExcelFolder, DateTime.Now.ToString("yyyyMMddHHmmss"),file.FileName);

                    WriteLog.WriteInfo(string.Format("服务器存放的表格文件路径:{0}", filePath));
                    string dir = Path.GetDirectoryName(filePath);                   //获取文件目录
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    using (var mStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        using (var mfs = file.OpenReadStream())
                        {
                            var buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = mfs.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                mStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                    string suffix = Path.GetExtension(filePath);
                    string[] szSuffix = { ".xlsx", ".csv", ".xls" };
                    if (!szSuffix.Contains(suffix))
                    {
                        return Json(new AjaxResponse(new { flag = false }));
                    }
                    flag = await GetExcelData(filePath, suffix);
                }
                else
                {
                    return Json(new AjaxResponse(new { flag = false }));
                }
                return Json(new AjaxResponse(new { flag = flag }));
            }
            catch (System.Exception ex)
            {
                WriteLog.WriteError("[UploadPlanFiles] errInfo:" + ex.Message);
                return Json(new AjaxResponse(new { flag = false }));
            }
        }
        /// <summary>
        /// 下载设备列表报表文件(Excel）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DownloadDevicesExcelFile()
        {
            try
            {
                /////Excel文件存放的路径，与TaskUnitAppService类TaskBuildExcel方法的路径要一致
                string filePath = this._appFolders.ExcelFolder + "/导出报表/设备列表.xlsx";

                if (!System.IO.File.Exists(filePath))
                {
                    throw new UserFriendlyException(L("RequestedFileDoesNotExists"));
                }
                var mfs = System.IO.File.OpenRead(filePath);
                var extName = Path.GetExtension(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var index = fileName.LastIndexOf("_");
                if (index > 0)
                {
                    fileName = fileName.Substring(0, index);
                }

                return File(mfs, "application/octet-stream;charset=UTF-8", fileName + extName);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        /// <summary>
        /// 通过NPOI将Excel文件中的设备导入到数据库
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="suffix">文件后缀名</param>
        /// <returns>true：有设备导入， false:无设备导入</returns>
        private async Task<bool> GetExcelData(string filePath, string suffix)
        {
            var task = new Task<bool>(() =>
            {
                bool flag = false;
                using (FileStream fs = System.IO.File.OpenRead(filePath))
                {
                    IWorkbook wk;
                    if (suffix == ".xlsx")
                    {
                        wk = new XSSFWorkbook(fs);
                    }
                    else
                    {
                        wk = new HSSFWorkbook(fs);
                    }
                    int insertCount = 0;
                    if (wk.NumberOfSheets > 0)
                    {
                        ISheet sheet = wk.GetSheetAt(0);    //读取excel第一张表格
                        int rows = sheet.LastRowNum;
                        if (rows < 1)
                        {
                            return false;
                        }
                        for (int i = 1; i <= rows; i++)
                        {
                            //提取表格数据
                            IRow row = sheet.GetRow(i);
                            string deviceNo = row.GetCell(0).ToString();
                            if (string.IsNullOrWhiteSpace(deviceNo) || deviceNo.Length > 9)
                                continue;
                            string deviceDescribe = row.GetCell(1).ToString();
                            //插入数据库操作
                            var model = this._deviceInfo.GetAll().Where(d => d.DeviceNo == deviceNo).FirstOrDefault();
                            if (model == null)
                            {
                                //没有该设备号时才导入
                                Device_Info d = new Device_Info
                                {
                                    DeviceNo = deviceNo,
                                    DeviceDescribe = deviceDescribe,
                                    DeviceStatus = 0,
                                };
                                this._deviceInfo.InsertAsync(d);
                                insertCount++;
                            }
                        };
                    }
                    if (insertCount <= 0)
                        flag = false;
                    else
                        flag = true;
                }
                return flag;
            });
            task.Start();
            return await task;
        }


    }
}