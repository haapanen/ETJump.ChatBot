using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Builder;
using ETJump.ChatBot.Autofac;
using ETJump.ChatBot.Core;
using ETJump.ChatBot.Core.Settings;
using Microsoft.Extensions.Configuration;
using NLog;

namespace ETJump.ChatBot
{
    public class Program
    {
        public static IContainer Container { get; set; }

        public static void Main(string[] args)
        {
            Container = BuildContainer();

            using (var scope = Container.BeginLifetimeScope())
            {
                var application = scope.Resolve<Application>();

                application.RunAsync(args).Wait();
            }
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Application>();
            builder.RegisterModule<NLogModule>();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("chatbot.config.json", optional: false)
                .Build();
            builder.Register<Settings>(c =>
            {
                var settings = new Settings();

                configuration.Bind(settings);

                return settings;
            });

            NLog.LogManager.LoadConfiguration("nlog.config");
            
            return builder.Build();
        }
    }
}
