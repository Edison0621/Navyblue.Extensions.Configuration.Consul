// *****************************************************************************************************************
// Project          : Navyblue
// File             : Program.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:43
// *****************************************************************************************************************
// <copyright file="Program.cs" company="Shanghai Future Mdt InfoTech Ltd.">
//     Copyright ©  2012-2019 Mdt InfoTech Ltd. All rights reserved.
// </copyright>
// *****************************************************************************************************************

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Navyblue.Extension.Configuration.Consul;
using System;
using System.Threading;
using Consul;

namespace WebTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration(
                (hostingContext, builder) =>
                {
                    builder.AddConsul("userservice", cancellationTokenSource.Token, source =>
                    {
                        source.ConsulClientConfiguration = cco => cco.Address = new Uri("http://localhost:8500");
                        source.Optional = true;
                        source.ReloadOnChange = true;
                        source.ReloadDelay = 300;
                        source.QueryOptions = new QueryOptions
                        {
                            WaitIndex = 0
                        };
                    });

                    builder.AddConsul("commonservice", cancellationTokenSource.Token, source =>
                    {
                        source.ConsulClientConfiguration = cco => cco.Address = new Uri("http://localhost:8500");
                        source.Optional = true;
                        source.ReloadOnChange = true;
                        source.ReloadDelay = 300;
                        source.QueryOptions = new QueryOptions
                        {
                            WaitIndex = 0
                        };
                    });
                }).UseStartup<Startup>().Build().Run();
        }
    }
}