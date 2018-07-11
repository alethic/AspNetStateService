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
        /// <param name="__instance"></param>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public unsafe static void Prefix(
            ref IntPtr __state,
            ref Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler,
            ref byte* data,
            ref int length)
        {
            __state = IntPtr.Zero;

            var d = data;
            var t = Marshal.PtrToStringAnsi((IntPtr)d, length);

            var i = t.IndexOf(' ');
            if (i > 2)
            {
                // is the following character not a '/'?
                var c = t[i + 1];
                if (c != '/')
                {
                    // insert missing '/'
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
        }

        /// <summary>
        /// Invoked after the original ParseRequestLine method.
        /// </summary>
        /// <param name="__state"></param>
        /// <param name="handler"></param>
        public unsafe static void Postfix(
            IntPtr __state,
            ref Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler handler,
            ref byte* data,
            ref int length)
        {
            // deallocate our temporary buffer
            if (__state != IntPtr.Zero)
                Marshal.FreeHGlobal(__state);
        }

    }

}
