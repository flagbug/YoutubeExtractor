using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static IDictionary<string, string> Ciphers = new Dictionary<string, string>
        {
            {"vflNzKG7n", "s3 r s2 r s1 r w67"},
            {"vfllMCQWM", "s2 w46 r w27 s2 w43 s2 r"},
            {"vflJv8FA8", "s1 w51 w52 r"},
            {"vflR_cX32", "s2 w64 s3"},
            {"vflveGye9", "w21 w3 s1 r w44 w36 r w41 s1"},
            {"vflj7Fxxt", "r s3 w3 r w17 r w41 r s2"},
            {"vfltM3odl", "w60 s1 w49 r s1 w7 r s2 r"},
            {"vflDG7-a-", "w52 r s3 w21 r s3 r"},
            {"vfl39KBj1", "w52 r s3 w21 r s3 r"},
            {"vflmOfVEX", "w52 r s3 w21 r s3 r"},
            {"vflJwJuHJ", "r s3 w19 r s2"},
            {"vfl_ymO4Z", "r s3 w19 r s2"},
            {"vfl26ng3K", "r s2 r"},
            {"vflcaqGO8", "w24 w53 s2 w31 w4"},
            {"vflQw-fB4", "s2 r s3 w9 s3 w43 s3 r w23"},
            {"vflSAFCP9", "r s2 w17 w61 r s1 w7 s1"},
            {"vflART1Nf", "s3 r w63 s2 r s1"},
            {"vflLC8JvQ", "w34 w29 w9 r w39 w24"},
            {"vflm_D8eE", "s2 r w39 w55 w49 s3 w56 w2"},
            {"vflTWC9KW", "r s2 w65 r"},
            {"vflRFcHMl", "s3 w24 r"},
            {"vflM2EmfJ", "w10 r s1 w45 s2 r s3 w50 r"},
            {"vflz8giW0", "s2 w18 s3"},
            {"vfl_wGgYV", "w60 s1 r s1 w9 s3 r s3 r"},
            {"vfl1HXdPb", "w52 r w18 r s1 w44 w51 r s1"},
            {"vflkn6DAl", "w39 s2 w57 s2 w23 w35 s2"},
            {"vfl2LOvBh", "w34 w19 r s1 r s3 w24 r"},
            {"vfl-bxy_m", "w48 s3 w37 s2"},
            {"vflZK4ZYR", "w19 w68 s1"},
            {"vflh9ybst", "w48 s3 w37 s2"},
            {"vflapUV9V", "s2 w53 r w59 r s2 w41 s3"},
            {"vflg0g8PQ", "w36 s3 r s2"},
            {"vflHOr_nV", "w58 r w50 s1 r s1 r w11 s3"},
            {"vfluy6kdb", "r w12 w32 r w34 s3 w35 w42 s2"},
            {"vflkuzxcs", "w22 w43 s3 r s1 w43"},
            {"vflGNjMhJ", "w43 w2 w54 r w8 s1"},
            {"vfldJ8xgI", "w11 r w29 s1 r s3"},
            {"vfl79wBKW", "s3 r s1 r s3 r s3 w59 s2"},
            {"vflg3FZfr", "r s3 w66 w10 w43 s2"},
            {"vflUKrNpT", "r s2 r w63 r"},
            {"vfldWnjUz", "r s1 w68"},
            {"vflP7iCEe", "w7 w37 r s1"},
            {"vflzVne63", "w59 s2 r"},
            {"vflO-N-9M", "w9 s1 w67 r s3"},
            {"vflZ4JlpT", "s3 r s1 r w28 s1"},
            {"vflDgXSDS", "s3 r s1 r w28 s1"},
            {"vflW444Sr", "r w9 r s1 w51 w27 r s1 r"},
            {"vflK7RoTQ", "w44 r w36 r w45"},
            {"vflKOCFq2", "s1 r w41 r w41 s1 w15"},
            {"vflcLL31E", "s1 r w41 r w41 s1 w15"},
            {"vflz9bT3N", "s1 r w41 r w41 s1 w15"},
            {"vfliZsE79", "r s3 w49 s3 r w58 s2 r s2"},
            {"vfljOFtAt", "r s3 r s1 r w69 r"},
            {"vflqSl9GX", "w32 r s2 w65 w26 w45 w24 w40 s2"},
            {"vflFrKymJ", "w32 r s2 w65 w26 w45 w24 w40 s2"},
            {"vflKz4WoM", "w50 w17 r w7 w65"},
            {"vflhdWW8S", "s2 w55 w10 s3 w57 r w25 w41"},
            {"vfl66X2C5", "r s2 w34 s2 w39"},
            {"vflCXG8Sm", "r s2 w34 s2 w39"},
            {"vfl_3Uag6", "w3 w7 r s2 w27 s2 w42 r"},
            {"vflQdXVwM", "s1 r w66 s2 r w12"},
            {"vflCtc3aO", "s2 r w11 r s3 w28"},
            {"vflCt6YZX", "s2 r w11 r s3 w28"},
            {"vflG49soT", "w32 r s3 r s1 r w19 w24 s3"},
            {"vfl4cHApe", "w25 s1 r s1 w27 w21 s1 w39"},
            {"vflwMrwdI", "w3 r w39 r w51 s1 w36 w14"},
            {"vfl4AMHqP", "r s1 w1 r w43 r s1 r"},
            {"vfln8xPyM", "w36 w14 s1 r s1 w54"},
            {"vflVSLmnY", "s3 w56 w10 r s2 r w28 w35"},
            {"vflkLvpg7", "w4 s3 w53 s2"},
            {"vflbxes4n", "w4 s3 w53 s2"},
            {"vflmXMtFI", "w57 s3 w62 w41 s3 r w60 r"},
            {"vflYDqEW1", "w24 s1 r s2 w31 w4 w11 r"},
            {"vflapGX6Q", "s3 w2 w59 s2 w68 r s3 r s1"},
            {"vflLCYwkM", "s3 w2 w59 s2 w68 r s3 r s1"},
            {"vflcY_8N0", "s2 w36 s1 r w18 r w19 r"},
            {"vfl9qWoOL", "w68 w64 w28 r"},
            {"vfle-mVwz", "s3 w7 r s3 r w14 w59 s3 r"},
            {"vfltdb6U3", "w61 w5 r s2 w69 s2 r"},
            {"vflLjFx3B", "w40 w62 r s2 w21 s3 r w7 s3"},
            {"vfliqjKfF", "w40 w62 r s2 w21 s3 r w7 s3"},
            {"ima-vflxBu-5R", "w40 w62 r s2 w21 s3 r w7 s3"},
            {"ima-vflrGwWV9", "w36 w45 r s2 r"},
            {"ima-vflCME3y0", "w8 s2 r w52"},
            {"ima-vfl1LZyZ5", "w8 s2 r w52"},
            {"ima-vfl4_saJa", "r s1 w19 w9 w57 w38 s3 r s2"},
            {"ima-en_US-vflP9269H", "r w63 w37 s3 r w14 r"},
            {"ima-en_US-vflkClbFb", "s1 w12 w24 s1 w52 w70 s2"},
            {"ima-en_US-vflYhChiG", "w27 r s3"},
            {"ima-en_US-vflWnCYSF", "r s1 r s3 w19 r w35 w61 s2"},
            {"en_US-vflbT9-GA", "w51 w15 s1 w22 s1 w41 r w43 r"},
            {"en_US-vflAYBrl7", "s2 r w39 w43"},
            {"en_US-vflS1POwl", "w48 s2 r s1 w4 w35"},
            {"en_US-vflLMtkhg", "w30 r w30 w39"},
            {"en_US-vflgd5txb", "w26 s1 w15 w3 w62 w54 w22"},
            {"en_US-vflbJnZqE", "w26 s1 w15 w3 w62 w54 w22"},
            {"en_US-vflTm330y", "w26 s1 w15 w3 w62 w54 w22"},
            {"en_US-vflnwMARr", "s3 r w24 s2"},
            {"en_US-vflA-1YdP", "w26 s1 w14 r s3 w8"}
        };

        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/html5player-{0}.js",
                cipherVersion);
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