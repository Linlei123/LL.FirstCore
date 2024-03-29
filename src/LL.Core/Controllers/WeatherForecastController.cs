﻿using LL.Core.Common.Output;
using LL.Core.IServices;
using LL.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.Core.Controllers
{
    [ApiVersion("2")]
    [ApiController]
    //[EnableCors("any")]
    [Route("v{version:apiVersion}/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPostServices _services;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPostServices services)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// 获取天气数据信息yao
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "id": 1,
        ///        "name": "Item1",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = false)]
        //此特性用于标识接口将被弃用，但是目前又可以访问使用
        [Obsolete]
        [HttpGet]
        public IResponseOutput Get()
        {
            _services.Insert(new Post { Title = "http://blogs.msdn.com/adonet", Content = "测试文本" }, true);

            var rng = new Random();
            var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return ResponseOutput.Ok(data, "获取数据成功！！！");
        }

        /// <summary>
        /// 提交实体数据信息
        /// </summary>
        /// <returns></returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>       
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post(WeatherForecast item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
