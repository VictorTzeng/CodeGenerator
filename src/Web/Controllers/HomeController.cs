using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Extensions;
using Web.Models;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.CodeGenerator.DbFirst;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
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
                var option = new DbContextOption()
                {
                    ConnectionString = input.ConnectionString,
                    IsOutputSql = true
                };
                var dbContext = GetDbContext(input.DbType, option);
                var tables = dbContext.GetCurrentDatabaseTableList().Where(m=>m.Columns.Any(n=>n.IsPrimaryKey)).ToList();

                tables?.ForEach(x =>
                {
                    if (!input.KeepPrefix && !input.Prefixes.IsNullOrWhiteSpace())
                    {
                        var prefixes = input.Prefixes.Split(',');
                        foreach (var prefix in prefixes)
                        {
                            if (x.TableName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                x.Alias = x.TableName.Replace(prefix, "", StringComparison.OrdinalIgnoreCase);
                                break;
                            }
                        }
                    }

                    if (input.IsPascalCase)
                    {
                        x.Alias = (x.Alias.IsNullOrEmpty() ? x.TableName : x.Alias).ToPascalCase();
                    }

                    foreach (var column in x.Columns)
                    {
                        column.Alias = input.IsPascalCase ? column.ColName.ToPascalCase() : column.ColName;
                    }
                });
                return Json(ExcutedResult.SuccessResult(tables));
            }
            catch (Exception e)
            {
                Log4NetHelper.WriteError(GetType(), e);
                return Json(ExcutedResult.FailedResult($"数据库连接失败，具体原因如下：{e.Message}"));
            }
        }

        private IDbContextCore GetDbContext(DatabaseType dbType, DbContextOption option)
        {
            IDbContextCore dbContext;
            switch (dbType)
            {
                case DatabaseType.MySQL:
                    dbContext = new MySqlDbContext(option);
                    break;
                case DatabaseType.PostgreSQL:
                    dbContext = new PostgreSQLDbContext(option);
                    break;
                case DatabaseType.Oracle:
                    dbContext = new OracleDbContext(option);
                    break;
                default:
                    dbContext = new SqlServerDbContext(option);
                    break;
            }

            return dbContext;
        }

        [HttpPost]
        public IActionResult Generate([FromBody]GenerateOption input)
        {
            if (ModelState.IsValid)
            {
                var generator = new CodeGenerator(input);

                var tables = input.TableData;
                generator.Generate(tables, true);

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
            var host = GetService<IWebHostEnvironment>();
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

        [HttpPost]
        public IActionResult PascalRename([FromBody]GenerateOption input)
        {
            if (input.TableData != null)
            {
                foreach(var item in input.TableData)
                {
                    if (item.Alias.IsNullOrEmpty())
                    {
                        item.Alias = input.IsPascalCase?item.TableName.ToPascalCase():item.TableName;
                    }
                    else
                    {
                        item.Alias = input.IsPascalCase?item.Alias.ToPascalCase():item.TableName;
                    }

                    foreach(var x in item.Columns)
                    {
                        if (x.Alias.IsNullOrEmpty())
                        {
                            x.Alias = input.IsPascalCase?x.ColName.ToPascalCase():x.ColName;
                        }
                        else
                        {
                            x.Alias = input.IsPascalCase?x.Alias.ToPascalCase():x.ColName;
                        }
                    }
                }
            }
            var tables = input.TableData;
            return Json(ExcutedResult.SuccessResult(tables));
        }

        [HttpPost]
        public IActionResult Preview(int index, [FromBody] GenerateOption option)
        {
            var generator = new CodeGenerator(option);

            var table = option.TableData[index];

            generator.Generate(new List<DbTable>() {table}, true);
            var tableName = table.TableName;
            var className = table.Alias.IsNullOrEmpty() ? tableName : table.Alias;

            return Json(ExcutedResult.SuccessResult(new
            {
                table = tableName,
                model = new
                {
                    content = generator.ReadFile("Models", tableName + ".cs"),
                    name = tableName + ".cs"
                },
                irepository = new
                {
                    content = generator.ReadFile("IRepositories", $"I{className}Repository.cs"),
                    name = $"I{className}Repository.cs"
                },
                repository = new
                {
                    content = generator.ReadFile("Repositories", $"{className}Repository.cs"),
                    name = $"{className}Repository.cs"
                },
                iservice = new
                {
                    content = generator.ReadFile("IServices", $"I{className}Service.cs"),
                    name = $"I{className}Service.cs"
                },
                service = new
                {
                    content = generator.ReadFile("Services", $"{className}Service.cs"),
                    name = $"{className}Service.cs"
                },
                controller = new
                {
                    content = generator.ReadFile("Controllers", $"{className}Controller.cs"),
                    name = $"{className}Controller.cs"
                }
            }));
        }
    }
}
