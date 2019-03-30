using System;
using System.Runtime.InteropServices;
using System.Text;

using Harmony;

namespace AspNetStateService.AspNetCore.Kestrel
{

    /// <summary>
    /// Patches the Kestrel web server to accept invalid HTTP requests that begin with a '%'.
    /// </summary>
    [HarmonyPatch(
        typeof(Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpParser<Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler>),
        "ParseRequestLine",
        new[] { typeof(Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler), typeof(byte*), typeof(int) })]
    public class KestrelPatch
    {

        /// <summary>
        /// Invoked before the original ParseRequestLine method.
        /// </summary>
        /// <param name="__state"></param>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public unsafe static void Prefix(
            ref GCHandle? __state,
            ref Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler,
            ref byte* data,
            ref int length)
        {
            __state = null;

            var d = data;
            var t = Encoding.ASCII.GetString(d, length);

            var i = t.IndexOf(' ', 0, 20);
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
                    data = (byte*)ptr.ToPointer();
                    length = buf.Length;
                }
            }
        }

        /// <summary>
        /// Invoked after the original ParseRequestLine method.
        /// </summary>
        /// <param name="__state"></param>
        /// <param name="handler"></param>
        public unsafe static void Postfix(
            GCHandle? __state,
            ref Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler,
            ref byte* data,
            ref int length)
        {
            if (__state.HasValue)
                __state.Value.Free();
        }

    }

}
