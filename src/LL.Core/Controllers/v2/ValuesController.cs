﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.Core.Controllers.v2
{
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// v2版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get() => Ok(new string[] { "version2.0" });

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("exceptiontest")]
        public string ExceptionTest()
        {
            try
            {
                _logger.LogInformation("测试exceptionless");
                throw new Exception("发生了未知的异常");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{HttpContext.Connection.RemoteIpAddress}调用了api/v2/Values/exceptiontest接口返回了失败");
            }
            return "调用失败";
        }
    }
}
