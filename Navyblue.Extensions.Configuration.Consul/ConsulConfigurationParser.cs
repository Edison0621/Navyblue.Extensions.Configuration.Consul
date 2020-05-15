// *****************************************************************************************************************
// Project          : Navyblue
// File             : ConsulConfigurationParser.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:43
// *****************************************************************************************************************
// <copyright file="ConsulConfigurationParser.cs" company="Shanghai Future Mdt InfoTech Ltd.">
//     Copyright ©  2012-2019 Mdt InfoTech Ltd. All rights reserved.
// </copyright>
// *****************************************************************************************************************

using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Navyblue.Extension.Configuration.Consul
{
    public sealed class ConsulConfigurationParser
    {
        private readonly IConsulConfigurationSource consulConfigurationSource;
        private readonly object lastIndexLock = new object();
        private ulong lastIndex;
        private ConfigurationReloadToken reloadToken = new ConfigurationReloadToken();

        public ConsulConfigurationParser(IConsulConfigurationSource consulConfigurationSource)
        {
            this.consulConfigurationSource = consulConfigurationSource;
        }

        /// <summary>
        /// 获取并转换Consul配置信息
        /// </summary>
        /// <param name="reloading"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetConfig(bool reloading, IConsulConfigurationSource source)
        {
            QueryResult<KVPair> kvPair = await this.GetKvPairs(source.ServiceKey, source.QueryOptions, source.CancellationToken).ConfigureAwait(false);
            switch (kvPair?.Response)
            {
                case null when !source.Optional:
                {
                    if (!reloading)
                    {
                        throw new FormatException(Resources.Error_InvalidService(source.ServiceKey));
                    }

                    return new Dictionary<string, string>();
                }
                case null:
                    throw new FormatException(Resources.Error_ValueNotExist(source.ServiceKey));
                default:
                    this.UpdateLastIndex(kvPair);

                    return JsonConfigurationFileParser.Parse(source.ServiceKey, new MemoryStream(kvPair.Response.Value));
            }
        }

        /// <summary>
        /// Consul配置信息监控
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IChangeToken Watch(string key, CancellationToken cancellationToken)
        {
            Task.Run(() => this.RefreshForChanges(key, cancellationToken), cancellationToken);

            return this.reloadToken;
        }

        private async Task<QueryResult<KVPair>> GetKvPairs(string key, QueryOptions queryOptions, CancellationToken cancellationToken)
        {
            using (IConsulClient consulClient = new ConsulClient(
                this.consulConfigurationSource.ConsulClientConfiguration,
                this.consulConfigurationSource.ConsulHttpClient,
                this.consulConfigurationSource.ConsulHttpClientHandler))
            {
                QueryResult<KVPair> result = await consulClient.KV.Get(key, queryOptions, cancellationToken).ConfigureAwait(false);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.NotFound:
                        return result;

                    default:
                        throw new FormatException(Resources.Error_Request(key));
                }
            }
        }

        private async Task<bool> IsValueChanged(string key, CancellationToken cancellationToken)
        {
            QueryOptions queryOptions;
            lock (this.lastIndexLock)
            {
                queryOptions = new QueryOptions
                {
                    WaitIndex = this.lastIndex
                };
            }

            QueryResult<KVPair> result = await this.GetKvPairs(key, queryOptions, cancellationToken).ConfigureAwait(false);

            return result != null && this.UpdateLastIndex(result);
        }

        private async Task RefreshForChanges(string key, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await this.IsValueChanged(key, cancellationToken).ConfigureAwait(false))
                {
                    ConfigurationReloadToken previousToken = Interlocked.Exchange(ref this.reloadToken, new ConfigurationReloadToken());
                    previousToken.OnReload();

                    return;
                }
            }
        }

        private bool UpdateLastIndex(QueryResult queryResult)
        {
            lock (this.lastIndexLock)
            {
                if (queryResult.LastIndex > this.lastIndex)
                {
                    this.lastIndex = queryResult.LastIndex;
                    return true;
                }
            }

            return false;
        }
    }
}