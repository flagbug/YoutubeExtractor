using System;

namespace YoutubeExtractor
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(double progressPercentage)
        {
            this.ProgressPercentage = progressPercentage;
        }

        /// <summary>
        /// Gets the progress percentage in a range from 0.0 to 100.0.
        /// </summary>
        public double ProgressPercentage { get; private set; }
    }
}