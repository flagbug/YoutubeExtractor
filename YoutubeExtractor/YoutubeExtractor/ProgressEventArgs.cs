using System;

namespace YoutubeExtractor
{
    public class ProgressEventArgs : EventArgs
    {
        public int ProgressPercentage { get; private set; }

        public ProgressEventArgs(int progressPercentage)
        {
            this.ProgressPercentage = progressPercentage;
        }
    }
}