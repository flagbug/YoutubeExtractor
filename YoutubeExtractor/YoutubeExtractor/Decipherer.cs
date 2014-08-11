using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static string DecipherOperations(string cipherVersion)
        {
            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/html5player-{0}.js", cipherVersion);
            string js = HttpHelper.DownloadString(jsUrl);

            //Find "C" in this: var A = B.sig||C (B.s)
            string functNamePattern = @"\.sig\s*\|\|(\w+)\(";
            var funcName = Regex.Match(js, functNamePattern).Groups[1].Value;
            string funcBodyPattern = @"(?<brace>{([^{}]| ?(brace))*})";  //Match nested angle braces
            string funcPattern = string.Format(@"{0}\(\w+\){1}", funcName, funcBodyPattern);
            var funcBody = Regex.Match(js, funcPattern).Groups["brace"].Value; //Entire sig function
            var lines = funcBody.Split(';'); //Each line in sig function

            string id_Reverse = "", id_Slice = "", id_CharSwap = ""; //Hold name for each method
            string functionIdentifier = "";
            string operations = "";

            //Match the code with each function. Only runs till all three are defined.
            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                if (!string.IsNullOrEmpty(id_Reverse) && !string.IsNullOrEmpty(id_Slice) &&
                 !string.IsNullOrEmpty(id_CharSwap)) { break; } //Break out if all defined.

                functionIdentifier = getFunctionFromLine(line);
                string re_Reverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string re_Slice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string re_Swap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                Match me;
                if ((me = Regex.Match(js, re_Reverse)).Success)
                { id_Reverse = functionIdentifier; } //If def matched the regex for reverse then the current function is a defined as the reverse cipher

                if ((me = Regex.Match(js, re_Slice)).Success)
                { id_Slice = functionIdentifier; } //If def matched the regex for slice then the current function is defined as the slice cipher.

                if ((me = Regex.Match(js, re_Swap)).Success)
                { id_CharSwap = functionIdentifier; } //If def matched the regex for charSwap then the current function is defined as swap cipher.

            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = getFunctionFromLine(line);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == id_CharSwap)
                { operations += "w" + m.Groups["index"].Value + " "; } //Character swap regex appears to be the same as before

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == id_Slice)
                { operations += "s" + m.Groups["index"].Value + " "; } //Slice appears to have changed the index location???
                //Could be wrong and the regex needs improving, seems to work on the latest algorithm though.

                if (functionIdentifier == id_Reverse)
                { operations += "r "; } //Reverse operation, no regex required

            }

            operations = operations.Trim();
            Console.WriteLine(operations);
            return operations;

        }

        public static string DecipherWithOperations(string cipher, string operations)
        {
            if (string.IsNullOrEmpty(operations))
            { throw new NotImplementedException("No valid cipher operations found."); }

            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(cipher, ApplyOperation);
        }


        private static string getFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want ac.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
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