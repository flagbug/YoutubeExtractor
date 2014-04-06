using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            // NB: We intentionally don't cache the operations as it seems that the same operation
            //     doesn't work if the URL below isn't called

            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/html5player-{0}.js", cipherVersion);
            string js = HttpHelper.DownloadString(jsUrl);

            //Find "C" in this: var A = B.sig||C (B.s)
            string functNamePattern = @"\.sig\s*\|\|(\w+)\(";
            var funcName = Regex.Match(js, functNamePattern).Groups[1].Value;

            //Match nested angle braces
            string funcBodyPattern = @"(?<brace>{([^{}]| ?(brace))*})";
            //Match the function function_name (that has one argument)
            string funcPattern = string.Format(@"{0}\(\w+\){1}", funcName, funcBodyPattern);
            var funcBody = Regex.Match(js, funcPattern).Groups["brace"].Value;

            var lines = funcBody.Split(';');
            string operations = "";
            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success)
                    //calling a two argument function (swap)
                    operations += "w" + m.Groups["index"].Value + " ";
                else if ((m = Regex.Match(line, @"slice\((?<index>\d+)\)")).Success)
                    //calling slice
                    operations += "s" + m.Groups["index"].Value + " ";
                else if ((m = Regex.Match(line, @"reverse\(\)")).Success)
                    //calling reverse
                    operations += "r ";
            }
            operations = operations.Trim();

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