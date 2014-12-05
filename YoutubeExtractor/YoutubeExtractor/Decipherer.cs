using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
		static Dictionary<string,string> Cache = new Dictionary<string, string>();
		public static bool PreCache()
		{
			const string version = "en_US-vflkHheES/html5player";
			if (Cache.ContainsKey (version))
				return true;
			try{
				GetCipherString(version);
				return true;
			}
			catch(Exception ex) {
				Console.WriteLine ("Error caching cipher key: {0}", ex.Message);
			}
			return false;
		}
        public static string DecipherWithVersion(string cipher, string cipherVersion)
		{
			var operations = GetCipherString (cipherVersion);
            return DecipherWithOperations(cipher, operations);
        }

		private static string GetCipherString(string cipherVersion)
		{
			string operations = "";
			string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/html5player-{0}.js", cipherVersion);
			if (!Cache.ContainsKey (cipherVersion)) {
				string js = HttpHelper.DownloadString (jsUrl);

				//Find "C" in this: var A = B.sig||C (B.s)
				string functNamePattern = @"\.sig\s*\|\|(\w+)\(";
				var funcName = Regex.Match (js, functNamePattern).Groups [1].Value;
				string funcBodyPattern = @"(?<brace>{([^{}]| ?(brace))*})";  //Match nested angle braces
				string funcPattern = string.Format (@"{0}\(\w+\){1}", funcName, funcBodyPattern);
				var funcBody = Regex.Match (js, funcPattern).Groups ["brace"].Value; //Entire sig function
				var lines = funcBody.Split (';'); //Each line in sig function

				string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
				string functionIdentifier = "";

				foreach (var line in lines.Skip(1).Take(lines.Length - 2)) { //Matches the funcBody with each cipher method. Only runs till all three are defined.
					if (!string.IsNullOrEmpty (idReverse) && !string.IsNullOrEmpty (idSlice) &&
						!string.IsNullOrEmpty (idCharSwap)) {
						break; //Break loop if all three cipher methods are defined
					}

					functionIdentifier = GetFunctionFromLine (line);
					string reReverse = string.Format (@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
					string reSlice = string.Format (@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
					string reSwap = string.Format (@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

					if (Regex.Match (js, reReverse).Success) {
						idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
					}

					if (Regex.Match (js, reSlice).Success) {
						idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
					}

					if (Regex.Match (js, reSwap).Success) {
						idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
					}
				}

				foreach (var line in lines.Skip(1).Take(lines.Length - 2)) {
					Match m;
					functionIdentifier = GetFunctionFromLine (line);

					if ((m = Regex.Match (line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap) {
						operations += "w" + m.Groups ["index"].Value + " "; //operation is a swap (w)
					}

					if ((m = Regex.Match (line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice) {
						operations += "s" + m.Groups ["index"].Value + " "; //operation is a slice
					}

					if (functionIdentifier == idReverse) { //No regex required for reverse (reverse method has no parameters)
						operations += "r "; //operation is a reverse
					}
				}

				Cache[cipherVersion] = operations.Trim ();
			}
			return  Cache [cipherVersion];
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

        private static string GetFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
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