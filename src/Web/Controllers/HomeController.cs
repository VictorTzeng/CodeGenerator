using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Zxw.Framework.NetCore.CodeGenerator;
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

        [HttpGet]
        public IActionResult GetDataTables(string connectionString)
        {
            try
            {
                var dbContext = new SqlServerDbContext(new DbContextOption()
                {
                    ConnectionString = connectionString
                });
                var dts = dbContext.GetCurrentDatabaseTableList();
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
                return Json(ExcutedResult.SuccessResult("生成成功"));
            }
            return Json(ExcutedResult.SuccessResult("数据验证失败，请检查后重试"));
        }
    }
}
