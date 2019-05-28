// *****************************************************************************************************************
// Project          : Navyblue
// File             : ConsulConfigurationSource.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:43
// *****************************************************************************************************************
// <copyright file="ConsulConfigurationSource.cs" company="Shanghai Future Mdt InfoTech Ltd.">
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
    internal sealed class ConsulConfigurationSource : IConsulConfigurationSource
    {
        public ConsulConfigurationSource(string serviceKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                throw new ArgumentNullException(nameof(serviceKey));
            }

            this.ServiceKey = serviceKey;
            this.CancellationToken = cancellationToken;
        }

        #region IConsulConfigurationSource Members

        public CancellationToken CancellationToken { get; }

        public Action<ConsulClientConfiguration> ConsulClientConfiguration { get; set; }

        public Action<HttpClient> ConsulHttpClient { get; set; }

        public Action<HttpClientHandler> ConsulHttpClientHandler { get; set; }

        public string ServiceKey { get; }

        public bool Optional { get; set; } = false;

        public QueryOptions QueryOptions { get; set; }

        public int ReloadDelay { get; set; } = 250;

        public bool ReloadOnChange { get; set; } = false;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            ConsulConfigurationParser consulParser = new ConsulConfigurationParser(this);

            return new ConsulConfigurationProvider(this, consulParser);
        }

        #endregion IConsulConfigurationSource Members
    }
}