using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;
using NLog;

namespace ETJump.ChatBot.Autofac
{
    public class NLogModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += RegistrationOnPreparing;
        }

        private void RegistrationOnPreparing(object sender, PreparingEventArgs e)
        {
            var t = e.Component.Activator.LimitType;

            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof (ILogger),
                        (p, i) => LogManager.GetLogger(t.FullName)
                    )
                });
        }
    }
}
