using System;

namespace YoutubeExtractor
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(double progressPercentage, int progressBytes)
        {
            this.ProgressPercentage = progressPercentage;
            this.ProgressBytes = progressBytes;
        }

        /// <summary>
        /// Gets or sets a token whether the operation that reports the progress should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the progress percentage in a range from 0.0 to 100.0.
        /// </summary>
        public double ProgressPercentage { get; private set; }

        /// <summary>
        /// Gets the progress bytes from the downloaded video.
        /// </summary>
        public int ProgressBytes { get; private set; }
    }
}