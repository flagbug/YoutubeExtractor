using System;

namespace YoutubeExtractor
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int progressBytes, double progressPercentage)
        {
            this.ProgressBytes = progressBytes;
            this.ProgressPercentage = progressPercentage;
        }

        /// <summary>
        /// Gets or sets a token whether the operation that reports the progress should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the progress in bytes downloaded.
        /// </summary>
        public int ProgressBytes { get; private set; }

        /// <summary>
        /// Gets the progress percentage in a range from 0.0 to 100.0.
        /// </summary>
        public double ProgressPercentage { get; private set; }
    }
}