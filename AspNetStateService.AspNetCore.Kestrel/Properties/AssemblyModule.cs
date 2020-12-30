using Autofac;

using Cogito.Autofac;

using HarmonyLib;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AspNetStateService.AspNetCore.Kestrel
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.AspNetCore.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterCallback(i => Patch());
        }

        /// <summary>
        /// Invokes the Harmony patches.
        /// </summary>
        void Patch()
        {
            var h = new Harmony(typeof(AssemblyModule).Assembly.GetName().Name);
            var a = typeof(KestrelServer).Assembly;
            var p = a.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler");
            var t = a.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpParser`1").MakeGenericType(p);
            var m = t.GetMethod("ParseRequestLine", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { p, typeof(ReadOnlySpan<byte>) }, null);
            var pre = AccessTools.Method(typeof(AssemblyModule), nameof(Prefix));
            var pfx = AccessTools.Method(typeof(AssemblyModule), nameof(Postfix));
            var n = h.Patch(m, new HarmonyMethod(pre), new HarmonyMethod(pfx));
        }

        /// <summary>
        /// Invoked before the original ParseRequestLine method.
        /// </summary>
        /// <param name="__state"></param>
        /// <param name="requestLine"></param>
        public unsafe static void Prefix(
            ref GCHandle? __state,
            ref ReadOnlySpan<byte> requestLine)
        {
            __state = null;

            var t = Encoding.ASCII.GetString(requestLine);
            var i = t.IndexOf(' ', 0, Math.Min(20, t.Length));
            if (i > 2 && t.Length > i + 2)
            {
                // is the following character not a '/'?
                var c = t[i + 1];
                if (c != '/')
                {
                    // insert missing '/'
                    t = t.Insert(i + 1, "/");

                    // create a copy of the rewritten request string
                    var buf = Encoding.ASCII.GetBytes(t);
                    var pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    var ptr = pin.AddrOfPinnedObject();

                    // will need to free this in the postfix
                    __state = pin;

                    // replace argument going into original method
                    requestLine = new ReadOnlySpan<byte>(ptr.ToPointer(), buf.Length);
                }
            }
        }

        /// <summary>
        /// Invoked after the original ParseRequestLine method.
        /// </summary>
        /// <param name="__state"></param>
        /// <param name="requestLine"></param>
        public unsafe static void Postfix(
            ref GCHandle? __state,
            ref ReadOnlySpan<byte> requestLine)
        {
            if (__state.HasValue)
                __state.Value.Free();
        }

    }

}
