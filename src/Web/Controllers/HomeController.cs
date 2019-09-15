﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Extensions;
using Web.Models;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.CodeGenerator.DbFirst;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult GetDataTables([FromBody]GenerateOption input)
        {
            try
            {
                var dbContext = new SqlServerDbContext(new DbContextOption()
                {
                    ConnectionString = input.ConnectionString
                });
                var dts = dbContext.GetCurrentDatabaseTableList();

                var instance = new CodeGenerator(input);
                dts?.ForEach(x =>
                {
                    var tmp = instance.DealTablePrefix(x);
                    x.Alias = tmp.Alias;
                });
                return Json(ExcutedResult.SuccessResult(dts));
            }
            catch (Exception e)
            {
                return Json(ExcutedResult.FailedResult($"数据库连接失败，具体原因如下：{e.Message}"));
            }
        }

        [HttpPost]
        public IActionResult Generate([FromBody]GenerateOption input)
        {
            if (ModelState.IsValid)
            {
                var instance = new CodeGenerator(input);
                instance.Generate(input.TableData, true);

                var zipFileName = GetZipFile();
                //var file =new FileInfo(zipFileName);
                //var bytes = new byte[file.Length];
                //var stream = file.OpenRead();
                //stream.Read(bytes, 0, Convert.ToInt32(file.Length));
                //stream.Close();
                //return File(bytes, "application/x-zip-compressed");
                return Json(ExcutedResult.SuccessResult(rows:"/zips/" + Path.GetFileName(zipFileName)));
            }
            return Json(ExcutedResult.SuccessResult("数据验证失败，请检查后重试"));
        }

        private string GetZipFile()
        {
            var host = GetService<IHostingEnvironment>();
            var zipPath = Path.Combine(host.WebRootPath, "zips");
            var zipFileName = Path.Combine(zipPath, Guid.NewGuid().ToString("N") + ".zip");
            foreach (var file in Directory.GetFiles(zipPath))
            {
                System.IO.File.Delete(file);
            }

            ZipFile.CreateFromDirectory(@"D:\CodeGenerates", zipFileName);

            foreach (var file in Directory.GetFiles(@"D:\CodeGenerates", "*.*", SearchOption.AllDirectories))
            {
                System.IO.File.Delete(file);
            }

            return zipFileName;
        }

        private T GetService<T>() => (T) HttpContext.RequestServices.GetService(typeof(T));
    }
}
