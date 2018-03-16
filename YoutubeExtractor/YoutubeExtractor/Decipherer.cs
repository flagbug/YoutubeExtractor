using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player-{0}.js", cipherVersion);
            string js = HttpHelper.DownloadString(jsUrl);

            var decodeArray = FindSignatureCode(js);

            return DecryptSignature(cipher, decodeArray);
        }

        private static List<int> FindSignatureCode(string sourceCode)
        {
            var signatureFunctionName = FindMatch(sourceCode, @"\.set\s*\(""signature""\s*,\s*([a-zA-Z0-9_$][\w$]*)\(");

            //Optimization of Gantt's technique - functionName not needed
            var regExp = @"\s*\([\w$]*\)\s*{[\w$]*=[\w$]*\.split\(""""\);\n*(.+);return [\w$]*\.join";

            var reverseFunctionName = FindMatch(sourceCode, @"([\w$]*)\s*:\s*function\s*\(\s*[\w$]*\s*\)\s*{\s*(?:return\s*)?[\w$]*\.reverse\s*\(\s*\)\s*}");
            var sliceFunctionName = FindMatch(sourceCode, @"([\w$]*)\s*:\s*function\s*\(\s*[\w$]*\s*,\s*[\w$]*\s*\)\s*{\s*(?:return\s*)?[\w$]*\.(?:slice|splice)\(.+\)\s*}");

            var functionCode = FindMatch(sourceCode, regExp);
            functionCode = functionCode.Replace(reverseFunctionName, "reverse");
            functionCode = functionCode.Replace(sliceFunctionName, "slice");
            var functionCodePieces = functionCode.Split(';');

            List<int> decodeArray = new List<int>();

            var regSlice = new Regex("slice\\s*\\(\\s*.+([0-9]+)\\s*\\)");
            string regSwap = "\\w+\\s*\\(\\s*\\w+\\s*,\\s*([0-9]+)\\s*\\)";
            string regInline = "\\w+\\[0\\]\\s*=\\s*\\w+\\[([0-9]+)\\s*%\\s*\\w+\\.length\\]";

            for (var i = 0; i < functionCodePieces.Length; i++)
            {

                functionCodePieces[i] = functionCodePieces[i].Trim();

                var codeLine = functionCodePieces[i];

                if (codeLine.Length > 0)
                {

                    var arrSlice = regSlice.Match(codeLine);

                    if (arrSlice.Success && arrSlice.Length >= 2)
                    {
                        var slice = int.Parse(arrSlice.Groups[1].Value);
                        decodeArray.Add(-slice);
                    }
                    else if (functionCodePieces[i].IndexOf("reverse") >= 0)
                    {
                        decodeArray.Add(0);
                    }
                    else if (codeLine.IndexOf("[0]") >= 0)
                    { // inline swap

                        if (i + 2 < functionCodePieces.Length && functionCodePieces[i + 1].IndexOf(".length") >= 0 && functionCodePieces[i + 1].IndexOf("[0]") >= 0)
                        {

                            var inline = FindMatch(functionCodePieces[i + 1], regInline);
                            decodeArray.Add(int.Parse(inline));

                            i += 2;

                        }
                    }
                    else if (codeLine.IndexOf(',') >= 0)
                    { // swap
                        var swap = FindMatch(codeLine, regSwap);
                        int swapVal = int.Parse(swap);

                        if (swapVal > 0)
                        {
                            decodeArray.Add(swapVal);
                        }

                    }
                }
            }
            return decodeArray;
        }

        private static string DecryptSignature(string sig, List<int> arr)
        {
            var sigA = sig;

            for (var i = 0; i < arr.Count; i++)
            {
                var act = arr[i];
                sigA = (act > 0) ? Swap(sigA.ToCharArray(), act) : ((act == 0) ? Reverse(sigA) : sigA.Substring(-act));
            }

            return sigA;
        }

        private static string Swap(char[] a, int b)
        {
            var c = a[0];
            a[0] = a[b % a.Length];
            a[b] = c;

            return new string(a);
        }

        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static string FindMatch(string text, string regexp)
        {
            Regex rgx = new Regex(regexp);
            var matches = rgx.Matches(text);

            if (matches.Count > 0)
                return matches[0].Groups[1].Value;
            else
                return string.Empty;
        }
    }
}