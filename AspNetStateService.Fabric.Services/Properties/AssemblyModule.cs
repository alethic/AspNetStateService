﻿using System.Fabric;
using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Service.Properties
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(ctx => new FabricClient()).SingleInstance();
        }

    }

}
