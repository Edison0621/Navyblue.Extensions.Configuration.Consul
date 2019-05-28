// *****************************************************************************************************************
// Project          : Navyblue
// File             : IConsulConfigurationSource.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:43
// *****************************************************************************************************************
// <copyright file="IConsulConfigurationSource.cs" company="Shanghai Future Mdt InfoTech Ltd.">
//     Copyright ©  2012-2019 Mdt InfoTech Ltd. All rights reserved.
// </copyright>
// *****************************************************************************************************************

using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading;

namespace Navyblue.Extension.Configuration.Consul
{
    /// <summary>
    /// ConsulConfigurationSource
    /// </summary>
    public interface IConsulConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// CancellationToken
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Consul构造函数实例，可自定义传入
        /// </summary>
        Action<ConsulClientConfiguration> ConsulClientConfiguration { get; set; }

        /// <summary>
        ///  Consul构造函数实例，可自定义传入
        /// </summary>
        Action<HttpClient> ConsulHttpClient { get; set; }

        /// <summary>
        ///  Consul构造函数实例，可自定义传入
        /// </summary>
        Action<HttpClientHandler> ConsulHttpClientHandler { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        string ServiceKey { get; }

        /// <summary>
        /// 可选项
        /// </summary>
        bool Optional { get; set; }

        /// <summary>
        /// Consul查询选项
        /// </summary>
        QueryOptions QueryOptions { get; set; }

        /// <summary>
        /// 重新加载延迟时间，单位是毫秒
        /// </summary>
        int ReloadDelay { get; set; }

        /// <summary>
        /// 是否在配置改变的时候重新加载
        /// </summary>
        bool ReloadOnChange { get; set; }
    }
}