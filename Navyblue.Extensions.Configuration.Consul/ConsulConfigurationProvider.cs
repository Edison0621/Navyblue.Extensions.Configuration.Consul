// *****************************************************************************************************************
// Project          : Navyblue
// File             : ConsulConfigurationProvider.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:43
// *****************************************************************************************************************
// <copyright file="ConsulConfigurationProvider.cs" company="Shanghai Future Mdt InfoTech Ltd.">
//     Copyright ©  2012-2019 Mdt InfoTech Ltd. All rights reserved.
// </copyright>
// *****************************************************************************************************************

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace Navyblue.Extension.Configuration.Consul
{
    public sealed class ConsulConfigurationProvider : ConfigurationProvider
    {
        private readonly ConsulConfigurationParser configurationParser;
        private readonly IConsulConfigurationSource source;

        public ConsulConfigurationProvider(IConsulConfigurationSource source, ConsulConfigurationParser configurationParser)
        {
            this.configurationParser = configurationParser;
            this.source = source;

            if (source.ReloadOnChange)
            {
                ChangeToken.OnChange(
                    () => this.configurationParser.Watch(this.source.ServiceKey, this.source.CancellationToken),
                    async () =>
                    {
                        await this.configurationParser.GetConfig(true, source).ConfigureAwait(false);

                        Thread.Sleep(source.ReloadDelay);

                        this.OnReload();
                    });
            }
        }

        public override void Load()
        {
            try
            {
                this.Data = this.configurationParser.GetConfig(false, this.source).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (AggregateException aggregateException)
            {
                if (aggregateException.InnerException != null)
                {
                    throw aggregateException.InnerException;
                }

                throw;
            }
        }
    }
}