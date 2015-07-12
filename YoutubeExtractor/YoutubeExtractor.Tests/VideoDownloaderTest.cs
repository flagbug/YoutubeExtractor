using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using YoutubeExtractor;
using Moq;

namespace YoutubeExtractor.Tests
{
    /// <summary>
    /// Test suite for VideoDownloader
    /// </summary>
    [TestFixture]
    public class VideoDownloaderTest
    {

        private Mock<VideoDownloader> _videoDownloader;
        private Mock<EventHandler<ProgressEventArgs>> _videoDownloadProgressChanged;
        private Mock<EventHandler> _videoDownloadStarted, _videoDownloadFinished;
        private Mock<VideoInfo> _videoInfo;
        private Mock<String> _savePath;
        #region [Setup / TearDown]
       

        /// <summary>
        /// Initialization for mock objects.
        /// </summary>
        [SetUp]
        public void InitializeVideoDownloader()
        {
            IEnumerable<VideoInfo> vids = DownloadUrlResolver.GetDownloadUrls("https://www.youtube.com/watch?v=LY_rMXXuJp8");
            _videoDownloader = new Mock<VideoDownloader>(vids.FirstOrDefault(), @"D:\somepath", null);
            _videoDownloadProgressChanged = new Mock<EventHandler<ProgressEventArgs>>();
            _videoDownloadStarted = new Mock<EventHandler>();
            _videoDownloadFinished = new Mock<EventHandler>();
            _videoDownloader.Object.DownloadStarted += _videoDownloadStarted.Object;
            _videoDownloader.Object.DownloadFinished += _videoDownloadFinished.Object;
            _videoDownloader.Object.DownloadProgressChanged += _videoDownloadProgressChanged.Object;
        }

        /// <summary>
        /// Clean up used resource.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            _videoDownloader = null;
            _videoDownloadStarted = null;
            _videoDownloadProgressChanged = null;
            _videoDownloadFinished = null;
        }
        #endregion

        #region "Actual Test"
        
        [Test]
        public void CheckIfVideoDownloadStartEventIsCalled()
        {
            _videoDownloader.Object.Execute();
            _videoDownloadStarted.Verify();
        }

        [Test]
        public void CheckIfVideoDownloadProgressEventIsCalled()
        {
            _videoDownloader.Object.Execute();
            _videoDownloadProgressChanged.Verify();
        }


        [Test]
        public void CheckIfVideoDownloadEndEventIsCalled()
        {
            _videoDownloader.Object.Execute();
            _videoDownloadFinished.Verify();
        }
        #endregion
    }
}
