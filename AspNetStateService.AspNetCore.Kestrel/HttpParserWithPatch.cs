using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace AspNetStateService.AspNetCore.Kestrel
{

    /// <summary>
    /// Wrapper of the <see cref="HttpParser{TRequestHandler}" /> implementation from Kestrel that rewrites the first
    /// request line with a leading '/' if it is missing.
    /// </summary>
    /// <typeparam name="TRequestHandler"></typeparam>
    class HttpParserWithPatch<TRequestHandler> :
        HttpParser<TRequestHandler>,
        IHttpParser<TRequestHandler>
        where TRequestHandler : IHttpHeadersHandler, IHttpRequestLineHandler
    {

        const byte ByteLF = (byte)'\n';

        static readonly MethodInfo innerParseRequestLineMethod;
        static readonly MethodInfo innerTryGetNewLineMethod;

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static HttpParserWithPatch()
        {
            innerParseRequestLineMethod = typeof(HttpParser<TRequestHandler>).GetMethod(
                "ParseRequestLine",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(TRequestHandler), typeof(byte*), typeof(int) },
                null);

            innerTryGetNewLineMethod = typeof(HttpParser<TRequestHandler>).GetMethod(
                "TryGetNewLine",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[] { typeof(ReadOnlySequence<byte>).MakeByRefType(), typeof(SequencePosition).MakeByRefType() },
                null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="showErrorDetails"></param>
        public HttpParserWithPatch(bool showErrorDetails = true) :
            base(showErrorDetails)
        {

        }

        /// <summary>
        /// Invokes the inner ParseRequestLine method.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        unsafe void ParseRequestLine(TRequestHandler handler, byte* data, int length)
        {
            var h = new GCHandle();
            var t = Encoding.ASCII.GetString(data, length);
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
                    h = pin;

                    // replace argument going into original method
                    data = (byte*)ptr.ToPointer();
                    length = buf.Length;
                }
            }

            try
            {
                innerParseRequestLineMethod.Invoke(
                    this,
                    new[] { handler, Pointer.Box(data, typeof(byte*)), length });
            }

            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }
        }

        /// <summary>
        /// Attempts to parse the first request line from the buffer.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="buffer"></param>
        /// <param name="consumed"></param>
        /// <param name="examined"></param>
        /// <returns></returns>
        unsafe bool IHttpParser<TRequestHandler>.ParseRequestLine(TRequestHandler handler, in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var span = buffer.First.Span;
            var lineIndex = span.IndexOf(ByteLF);
            if (lineIndex >= 0)
            {
                consumed = buffer.GetPosition(lineIndex + 1, consumed);
                span = span.Slice(0, lineIndex + 1);
            }
            else if (buffer.IsSingleSegment)
            {
                return false;
            }
            else if (TryGetNewLine(buffer, out var found))
            {
                span = buffer.Slice(consumed, found).ToSpan();
                consumed = found;
            }
            else
            {
                return false;
            }

            fixed (byte* data = span)
                ParseRequestLine(handler, data, span.Length);

            examined = consumed;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool TryGetNewLine(in ReadOnlySequence<byte> buffer, out SequencePosition found)
        {
            var o = new object[] { buffer, null };
            var r = innerTryGetNewLineMethod.Invoke(null, o);
            found = (SequencePosition)o[1];
            return (bool)r;
        }

    }

}
