﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Web.Models
{
    public class GenerateOption:CodeGenerateOption
    {
        [Required]
        public override string OutputPath { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string ModelsNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string ViewModelsNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string ControllersNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string IRepositoriesNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string RepositoriesNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string IServicesNamespace { get; set; }
        [Required,RegularExpression(@"^[^0-9]\w+$")]
        public override string ServicesNamespace { get; set; }
        [Required]
        public List<DbTable> TableData { get; set; }
    }
}