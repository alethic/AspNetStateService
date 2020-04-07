using System;
using System.Security.Cryptography;
using System.Text;

using Cogito.Autofac;

namespace AspNetStateService.Amazon.S3
{

    /// <summary>
    /// Converts session IDs into key usable by the Amazon S3 service.
    /// </summary>
    [RegisterAs(typeof(IStateKeyProvider))]
    public class DefaultStateKeyProvider : IStateKeyProvider
    {

        /// <summary>
        /// Returns <c>true</c> if the given value is an allowed character.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsAllowedChar(char c)
        {
            switch (c)
            {
                case '/':
                case '\\':
                case '^':
                case '`':
                case '>':
                case '<':
                case '{':
                case '}':
                case ']':
                case '[':
                case '#':
                case '%':
                case '"':
                case '~':
                case '|':
                    return false;
                default:
                    return char.IsControl(c) == false;
            }
        }

        /// <summary>
        /// Converts a byte set to a char set in hex.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static char[] ToHex(ReadOnlySpan<byte> bytes)
        {
            var c = new char[bytes.Length * 2];

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                var a = (byte)(bytes[bx] >> 4);
                c[cx] = (char)(a > 9 ? a - 10 + 'A' : a + '0');

                var b = (byte)(bytes[bx] & 0x0F);
                c[++cx] = (char)(b > 9 ? b - 10 + 'A' : b + '0');
            }

            return c;
        }

        readonly SHA1 sha1 = SHA1.Create();

        /// <summary>
        /// Gets a partition key for the given session ID based off of the first 4 bytes of the SHA1 hash.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDirectoryPath(string id)
        {
            return new string(ToHex(new ReadOnlySpan<byte>(sha1.ComputeHash(Encoding.UTF8.GetBytes(id))).Slice(0, 4)));
        }

        /// <summary>
        /// Gets a row key for the given session ID by eliminating prohibited characters.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetFileName(string id)
        {
            var rk = new char[id.Length];

            // replace disallowed characters
            for (int i = 0; i < id.Length; i++)
                rk[i] = IsAllowedChar(id[i]) ? id[i] : '_';

            return new string(rk);
        }

        /// <summary>
        /// Gets the expected key for the given session ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetKey(string id)
        {
            var d = GetDirectoryPath(id);
            var n = GetFileName(id);
            return d != null ? d + "/" + n : n;
        }

    }

}
