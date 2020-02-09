using System;
using System.Security.Cryptography;
using System.Text;

using Cogito.Autofac;

namespace AspNetStateService.Azure.Cosmos.Table
{

    /// <summary>
    /// Converts session IDs into partition and row keys usable by the Azure Storage Table service.
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
                case '#':
                case '?':
                    return false;
                default:
                    return char.IsControl(c) == false;
            }
        }

        readonly SHA1 sha1 = SHA1.Create();

        /// <summary>
        /// Gets a partition key for the given session ID based off of the first 4 bytes of the SHA1 hash.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetPartitionKey(string id)
        {
            return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(id)), 0, 4);
        }

        /// <summary>
        /// Gets a row key for the given session ID by eliminating prohibited characters.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetRowKey(string id)
        {
            var rk = new char[id.Length];

            // replace disallowed characters
            for (int i = 0; i < id.Length; i++)
                rk[i] = IsAllowedChar(id[i]) ? id[i] : '_';

            return new string(rk);
        }

    }

}
