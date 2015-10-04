// ****************************************************************************
//
// FLV Extract
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
using System.IO;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides a method to download a video and extract its audio track.
    /// </summary>
    public class AudioDownloader : Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to convert.</param>
        /// <param name="savePath">The path to save the audio.</param>
        /// /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public AudioDownloader(VideoInfo video, string savePath, int? bytesToDownload = null)
            : base(video, savePath, bytesToDownload)
        { }

        /// <summary>
        /// Occurs when the progress of the audio extraction has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> AudioExtractionProgressChanged;

        /// <summary>
        /// Occurs when the download progress of the video file has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Downloads the video from YouTube and then extracts the audio track out of it.
        /// </summary>
        /// <exception cref="IOException">
        /// The temporary video file could not be created.
        /// - or -
        /// The audio file could not be created.
        /// </exception>
        /// <exception cref="AudioExtractionException">An error occured during audio extraction.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override void Execute()
        {
            string tempPath = Path.GetTempFileName();
            try
            {
                this.DownloadVideo(tempPath);
                this.ExtractAudio(tempPath);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Downloads the video from YouTube (without extracting audio).
        /// </summary>
        /// <exception cref="IOException">
        /// The temporary video file could not be created.
        /// - or -
        /// The audio file could not be created.
        /// </exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        /// <param name="path"></param>
        public void DownloadVideo(string path)
        {
            var videoDownloader = new VideoDownloader(this.Video, path, this.BytesToDownload);

            videoDownloader.DownloadProgressChanged += DownloadProgressChanged;

            videoDownloader.Execute();
        }

        /// <summary>
        /// Extracts the audio track out of the downloaded video.
        /// </summary>
        /// <exception cref="IOException">
        /// The temporary video file could not be created.
        /// - or -
        /// The audio file could not be created.
        /// </exception>
        /// <exception cref="AudioExtractionException">An error occured during audio extraction.</exception>
        /// <param name="path"></param>
        public void ExtractAudio(string path)
        {
            using (var flvFile = new FlvFile(path, this.SavePath))
            {
                flvFile.ConversionProgressChanged += AudioExtractionProgressChanged;

                flvFile.ExtractStreams();
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}