﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.Core.Common.Cache
{
    /// <summary>
    /// 简单的缓存接口,只有查询和添加
    /// </summary>
    public interface ICaching
    {
        #region 删
        /// <summary>
        /// 用于在key存在时删除key
        /// </summary>
        /// <param name="key">键</param>
        long Delete(params string[] key);

        /// <summary>
        /// 用于在key存在时删除 key
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<long> DeleteAsync(params string[] key);

        /// <summary>
        /// 用于在key模板存在时删除
        /// </summary>
        /// <param name="pattern">key模板</param>
        /// <returns></returns>
        Task<long> DeleteByPatternAsync(string pattern);
        #endregion

        #region 是否存在
        /// <summary>
        /// 检查给定key是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// 检查给定key是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string key);
        #endregion

        #region 查
        /// <summary>
        /// 获取指定 key 的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);
        #endregion

        #region 增
        /// <summary>
        /// 设置指定key的值,所有写入参数object都支持string | byte[] | 数值 | 对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        bool Set(string key, object value);

        /// <summary>
        /// 设置指定key的值,所有写入参数object都支持string | byte[] | 数值 | 对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">有效期</param>
        bool Set(string key, object value, TimeSpan expire);

        /// <summary>
        /// 设置指定key的值,所有写入参数object都支持string | byte[] | 数值 | 对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, object value);

        /// <summary>
        /// 设置指定key的值,所有写入参数object都支持string | byte[] | 数值 | 对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">有效期</param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, object value, TimeSpan expire);
        #endregion
    }
}
