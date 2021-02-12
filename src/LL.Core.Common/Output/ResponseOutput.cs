using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LL.Core.Common.Output
{
    /// <summary>
    /// 通用数据输出实现类
    /// </summary>
    public class ResponseOutput<T> : IResponseOutput<T>
    {
        /// <summary>
        /// 是否成功标记
        /// </summary>
        [JsonIgnore]
        public bool Success { get; private set; }

        /// <summary>
        /// 状态码(1:成功;0:失败)
        /// </summary>
        public int Code => Success ? 1 : 0;

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; private set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="msg">消息</param>
        public ResponseOutput<T> Ok(T data, string msg = null)
        {
            Success = true;
            Data = data;
            Msg = msg;

            return this;
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public ResponseOutput<T> NotOk(string msg = null, T data = default)
        {
            Success = false;
            Msg = msg;
            Data = data;

            return this;
        }

        /// <summary>
        /// 成功(带标题)
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="title">标题</param>
        /// <param name="msg">消息</param>
        public ResponseOutput<T> OkTitle(T data, string title = "", string msg = "")
        {
            Success = true;
            Data = data;
            Msg = msg;
            Title = title;

            return this;
        }
    }

    /// <summary>
    /// 输出数据静态类
    /// </summary>
    public static partial class ResponseOutput
    {
        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static IResponseOutput Ok<T>(T data = default, string msg = null)
        {
            return new ResponseOutput<T>().Ok(data, msg);
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static IResponseOutput Ok()
        {
            return Ok<string>();
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static IResponseOutput NotOk<T>(string msg = null, T data = default)
        {
            return new ResponseOutput<T>().NotOk(msg, data);
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static IResponseOutput NotOk(string msg = null)
        {
            return new ResponseOutput<string>().NotOk(msg);
        }

        /// <summary>
        /// 根据布尔值返回结果
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        public static IResponseOutput Result<T>(bool success)
        {
            return success ? Ok<T>() : NotOk<T>();
        }

        /// <summary>
        /// 根据布尔值返回结果
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        public static IResponseOutput Result(bool success)
        {
            return success ? Ok() : NotOk();
        }
    }
}
