using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LL.Core.Common.Output
{
    /// <summary>
    /// 通用数据输出接口
    /// </summary>
    public interface IResponseOutput
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        bool Success { get; }

        /// <summary>
        /// 消息
        /// </summary>
        string Msg { get; }
    }

    /// <summary>
    /// 响应数据泛型接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResponseOutput<T> : IResponseOutput
    {
        /// <summary>
        /// 返回数据
        /// </summary>
        T Data { get; }
    }
}
