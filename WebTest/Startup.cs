// *****************************************************************************************************************
// Project          : Navyblue
// File             : Startup.cs
// Created          : 2019-05-23  19:30
//
// Last Modified By : (jstsmaxx@163.com)
// Last Modified On : 2019-05-24  11:44
// *****************************************************************************************************************
// <copyright file="Startup.cs" company="Shanghai Future Mdt InfoTech Ltd.">
//     Copyright ©  2012-2019 Mdt InfoTech Ltd. All rights reserved.
// </copyright>
// *****************************************************************************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Navyblue.Extension.Configuration.Consul;
using System.Linq;

namespace WebTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();

            builder.SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            this.Configuration = configuration;
            this.ConfigurationRoot = (IConfigurationRoot)configuration;
        }

        public IConfiguration Configuration { get; }

        public IConfigurationRoot ConfigurationRoot { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddMvcOptions(p=>p.EnableEndpointRouting=false);
            var consulProvider = this.ConfigurationRoot.Providers.Where(p => p.GetType() == typeof(ConsulConfigurationProvider)).ToList();
            var configurationSection = this.ConfigurationRoot.GetChildren();
        }
    }
}