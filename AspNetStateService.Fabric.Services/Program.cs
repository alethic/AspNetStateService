using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Autofac.Integration.ServiceFabric;

using FileAndServe.AspNetCore;
using FileAndServe.Autofac;
using FileAndServe.Components.AspNetCore;
using FileAndServe.Components.ServiceFabric.AspNetCore;
using FileAndServe.ServiceFabric;

using Harmony;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetStateService.Fabric.Services
{

    public static class Program
    {

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task Main(string[] args)
        {
            var h = HarmonyInstance.Create("AspNetStateService");
            h.PatchAll(Assembly.GetExecutingAssembly());

            return FabricEnvironment.IsFabric ? RunFabric(args) : RunConsole(args);
        }

        [HarmonyPatch(typeof(Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpParser<Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler>))]
        [HarmonyPatch("ParseRequestLine")]
        [HarmonyPatch(new[] { typeof(Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler), typeof(byte*), typeof(int) })]
        class Patch
        {

            /// <summary>
            /// Invoked before the original ParseRequestLine method.
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="handler"></param>
            /// <param name="data"></param>
            /// <param name="length"></param>
            public unsafe static bool Prefix(
                ref IntPtr __state,
                Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler,
                ref byte* data,
                ref int length)
            {
                __state = IntPtr.Zero;

                var d = data;
                var t = Marshal.PtrToStringAnsi((IntPtr)d, length);

                var i = t.IndexOf(' ');
                if (i > 2)
                {
                    var ch = t[i + 1];
                    if (ch == '%')
                    {
                        // insert missing /
                        t = t.Insert(i + 1, "/");

                        // create a copy of the rewritten request string
                        var buf = Encoding.ASCII.GetBytes(t);
                        var ptr = Marshal.AllocHGlobal(buf.Length);
                        Marshal.Copy(buf, 0, ptr, buf.Length);

                        // will need to free this in the postfix
                        __state = ptr;

                        // replace argument going into original method
                        data = (byte*)ptr.ToPointer();
                        length = buf.Length;
                    }
                }

                return true;
            }

            public unsafe static void Postfix(
                IntPtr __state,
                Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler)
            {
                if (__state != IntPtr.Zero)
                    Marshal.FreeHGlobal(__state);
            }

        }

        /// <summary>
        /// Runs the application in Console mode.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task RunConsole(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterFromAttributes(typeof(Program).Assembly);

            using (var container = builder.Build())
            using (var hostScope = container.BeginLifetimeScope())
                await WebHost.CreateDefaultBuilder(args)
                    .ConfigureComponents<StateWebService>(hostScope)
                    .UseKestrel()
                    .BuildAndRunAsync();
        }

        /// <summary>
        /// Runs the application in Service Fabric mode.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task RunFabric(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterServiceFabricSupport();
            builder.RegisterActor<StateActor>();
            builder.RegisterStatelessKestrelWebService<StateWebService>("StateWebService", "HttpServiceEndpoint");

            using (builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
