using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static IDictionary<string, string> Ciphers;

        static Decipherer()
        {
            // Extract the ciphers from the embedded YAML file
            string cipherFile = Encoding.UTF8.GetString(Properties.Resources.Ciphers);

            var cipherRegex = new Regex("(.*): (.*)");

            var ciphers = cipherRegex.Matches(cipherFile).Cast<Match>()
                .ToDictionary(x => x.Result("$1"), x => x.Result("$2"));

            Ciphers = ciphers;
        }

        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            string operations;

            bool hasCipher = Ciphers.TryGetValue(cipherVersion, out operations);

            if (!hasCipher)
            {
                return String.Empty;
            }

            return DecipherWithOperations(cipher, operations);
        }

        private static string ApplyOperation(string cipher, string op)
        {
            switch (op[0])
            {
                case 'r':
                    return new string(cipher.ToCharArray().Reverse().ToArray());

                case 'w':
                    {
                        int index = GetOpIndex(op);
                        return SwapFirstChar(cipher, index);
                    }

                case 's':
                    {
                        int index = GetOpIndex(op);
                        return cipher.Substring(index);
                    }

                default:
                    throw new NotImplementedException("Couldn't find cipher operation.");
            }
        }

        private static string DecipherWithOperations(string cipher, string operations)
        {
            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(cipher, ApplyOperation);
        }

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        private static string SwapFirstChar(string cipher, int index)
        {
            var builder = new StringBuilder(cipher);
            builder[0] = cipher[index];
            builder[index] = cipher[0];

            return builder.ToString();
        }
    }
}