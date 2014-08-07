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


            //CYNAO - 07/08/2014, Test Fix
            /* The previous algoritms used a.splice(), where the decipher used the method (.splice()), however it seems the new algoritm 
             * renames the method to random but unique characters (example ab.dc() = splice). This code determines what each method code is,
             * as it is defined once using the original name.
             */
            string id_Reverse = ""  , id_Slice = "" , id_CharSwap = ""; //Holds the new method name for each.
            string functionIdentifier = "";

            string functIDPattern = @"\w+:\bfunction\b";  //Define as "NB:function(a,b)" where nb can be the three ciphers
            var funcID = Regex.Match(js, functIDPattern).Groups[1].Value;

            ///CODE ADDITION: Get the three ciphers by finding the definition
            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                string newVarName; //Previous algoritms used to be just "a." - now stores temp var name as its uneccessary
                int locOfDot, locOfBracket, functionIDLength; 
                locOfDot = line.IndexOf("."); // NB.AC( - gets location of the dot.
                locOfBracket = line.IndexOf("("); //NB.AC( - gets location of the bracet
                functionIDLength = locOfBracket - (locOfDot + 1);
                newVarName = line.Substring(0, locOfDot);
                functionIdentifier = line.Substring(locOfDot + 1, functionIDLength); //leaving us with the function AC

                //This is what the definitions currently look like, could be changed so the regex needs improving. Messy fix.
                string tempReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Reverse only one that doesnt have two parameters
                string tempSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string tempCharSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.
                                
                Match me;
                if ((me = Regex.Match(js, tempReverse)).Success)
                { id_Reverse = functionIdentifier; } //If def matched the regex for reverse then the current function is a defined as the reverse cipher

                if ((me = Regex.Match(js, tempSlice)).Success)
                { id_Slice = functionIdentifier; } //If def matched the regex for slice then the current function is defined as the slice cipher.

                if ((me = Regex.Match(js, tempCharSwap)).Success)
                { id_CharSwap = functionIdentifier; } //If def matched the regex for charSwap then the current function is defined as swap cipher.

            }


            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                ///DUPLICATE CODE! Improve. 
                int locOfDot; int locOfBracket; int functionIDLength;
                locOfDot = line.IndexOf(".");
                locOfBracket = line.IndexOf("(");
                functionIDLength = locOfBracket - (locOfDot + 1);
                functionIdentifier = line.Substring(locOfDot + 1, functionIDLength); //Just needed this (define it as a member?)

                string newSliceIDRegex = string.Format(@"(?<index>\d+)\)+", functionIdentifier);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == id_CharSwap)
                { operations += "w" + m.Groups["index"].Value + " "; } //Character swap regex appears to be the same as before


                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == id_Slice)
                { operations += "s" + m.Groups["index"].Value + " "; } //Slice appears to have changed the index location???
                //Could be wrong and the regex needs improving, seems to work on the latest algorithm though.

                if (functionIdentifier == id_Reverse)
                { operations += "r "; } //Reverse operation, no regex required

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