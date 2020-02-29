using System;
using System.IO;
using DataServices.Repositories;
using DataServices.Repositories.Interfaces;
//using QBODataCollect.Repositories;
//using QBODataCollect.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.SqlServer;
using LoggerService;
using NLog;
using QBODataCollect.Extensions;
using Microsoft.Extensions.Hosting;

namespace QBODataCollect
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //In development and running frm VS the current folder is Program Files\IIS Express
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddTransient<IQBOAccessRepository, QBOAccessRepository>();
            services.AddTransient<IInvoiceRepository, InvoiceRepository>();
            services.AddTransient<ISubscriberRepository, SubscriberRepository>();
            services.AddTransient<IErrorLogRepository, ErrorLogRepository>();
            services.AddSingleton<ILoggerManager, LoggerManager>();
            services.AddControllers(); // replaces Add.Mvc in 2.2
            services.AddDataProtection();
            services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnectionString"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));
            //services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureCustomExceptionMiddleware();
            app.UseRouting(); // replaces app.UseMvc in 2.2
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => // Added with 3.1
            {
                endpoints.MapDefaultControllerRoute();
            });
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            //BackgroundJob.Enqueue<MasterController>(x => x.Client());
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});

        }
    }
}
