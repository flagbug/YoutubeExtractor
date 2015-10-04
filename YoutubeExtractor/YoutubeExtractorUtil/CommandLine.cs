﻿// ****************************************************************************
//
// YoutubeExtractorUtil
// Copyright (C) 2013-2015 Dennis Daume (daume.dennis@gmail.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YoutubeExtractor;

namespace YoutubeExtractorUtil
{
    internal class CommandLine
    {
        public List<string> Links { get; set; }
        public string LinksFile { get; set; }
        public bool IsHelp { get; set; }
        public string Destination { get; set; }
        public int MinResolution { get; set; }
        public int MaxResolution { get; set; }
        public int IdealResolution { get; set; }
        public bool ExtractAudio { get; set; }
        public VideoType[] VideoTypes { get; set; }

        public void ProcessArgs(IEnumerable<string> args)
        {
            MaxResolution = int.MaxValue;
            Links = new List<string>();

            foreach (string arg in args)
            {
                if (IsSwitch(arg))
                {
                    ProcessSwitch(arg.Substring(1));
                }
                else
                {
                    ProcessArg(arg);
                }
            }

            if (VideoTypes == null)
            {
                // Default accepted video types in order of preference
                VideoTypes = new[] { VideoType.Mp4, VideoType.Flash, VideoType.WebM, VideoType.Mobile, VideoType.Unknown };
            }
        }

        private void ProcessSwitch(string nakedArg)
        {
            string stringValue;

            if (nakedArg.Equals("Help", StringComparison.OrdinalIgnoreCase) || nakedArg.Equals("?", StringComparison.OrdinalIgnoreCase))
            {
                IsHelp = true;
            }
            else if (TryGetStringSwitchValue(nakedArg, "LinksFile", out stringValue))
            {
                LinksFile = Path.GetFullPath(string.IsNullOrEmpty(stringValue) ? "LinksFile.txt" : stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "Destination", out stringValue))
            {
                Destination = Path.GetFullPath(stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "MinResolution", out stringValue))
            {
                MinResolution = Convert.ToInt32(stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "MaxResolution", out stringValue))
            {
                MaxResolution = Convert.ToInt32(stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "IdealResolution", out stringValue))
            {
                IdealResolution = Convert.ToInt32(stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "ExtractAudio", out stringValue))
            {
                ExtractAudio = stringValue == "" || Convert.ToBoolean(stringValue);
            }
            else if (TryGetStringSwitchValue(nakedArg, "VideoTypes", out stringValue))
            {
                VideoTypes = stringValue
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => (VideoType)Enum.Parse(typeof(VideoType), t))
                    .ToArray();
            }
        }

        private void ProcessArg(string arg)
        {
            Links.Add(arg);
        }

        private static bool IsSwitch(string arg)
        {
            #region Validation
            if (arg == null) { throw new ArgumentNullException("arg"); }
            #endregion
            return arg.StartsWith("-", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetStringSwitchValue(string nakedArgWithValue, string switchName, out string stringValue, string defaultValue = null)
        {
            #region Validation
            if (nakedArgWithValue == null) { throw new ArgumentNullException("nakedArgWithValue"); }
            if (switchName == null) { throw new ArgumentNullException("switchName"); }
            #endregion

            string nakedArg;

            int equalIndex = nakedArgWithValue.IndexOf('=');

            if (equalIndex == -1)
            {
                equalIndex = nakedArgWithValue.Length;
            }

            nakedArg = nakedArgWithValue.Substring(0, equalIndex);

            if (nakedArg.Length > switchName.Length)
            {
                stringValue = defaultValue;
                return false;
            }

            if (0 != string.Compare(nakedArg, 0, switchName, 0, switchName.Length, StringComparison.OrdinalIgnoreCase))
            {
                stringValue = defaultValue;
                return false;
            }

            stringValue = nakedArgWithValue.Substring(Math.Min(equalIndex + 1, nakedArgWithValue.Length));
            return true;
        }

        public string ToString(string separator)
        {
            StringBuilder sb = new StringBuilder();

            if (Links.Count > 0)
            {
                sb.Append("Links: { ");
                sb.Append(string.Join(", ", Links.Select(l => '"' + l + '"').ToArray()));
                sb.Append(" }");
                sb.Append(separator);
            }

            if (!string.IsNullOrEmpty(LinksFile))
            {
                sb.Append("LinksFile: \"");
                sb.Append(LinksFile);
                sb.Append('"');
                sb.Append(separator);
            }

            if (!string.IsNullOrEmpty(Destination))
            {
                sb.Append("Destination: \"");
                sb.Append(Destination);
                sb.Append('"');
                sb.Append(separator);
            }

            if (MinResolution > 0)
            {
                sb.Append("MinResolution: ");
                sb.Append(MinResolution);
                sb.Append(separator);
            }

            if (MaxResolution < int.MaxValue)
            {
                sb.Append("MaxResolution: ");
                sb.Append(MaxResolution);
                sb.Append(separator);
            }

            if (IdealResolution != 0)
            {
                sb.Append("IdealResolution: ");
                sb.Append(IdealResolution);
                sb.Append(separator);
            }

            sb.Append("ExtractAudio: ");
            sb.Append(ExtractAudio);
            sb.Append(separator);

            sb.Append("VideoTypes: ");
            sb.Append("Links: { ");
            sb.Append(string.Join(", ", VideoTypes.Select(t => t.ToString()).ToArray()));
            sb.Append(" }");
            sb.Append(separator);

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }
    }
}
