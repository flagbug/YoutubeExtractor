using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YoutubeExtractor.Tests
{
    /// <summary>
    /// Small series of unit tests for DownloadUrlResolver. Run these with NUnit.
    /// </summary>
	[TestClass]
    public class DownloadUrlResolverTest
    {
        [TestMethod]
        public void TryNormalizedUrlForStandardYouTubeUrlShouldReturnSame()
        {
            string url = "http://youtube.com/watch?v=12345";

            string normalizedUrl = String.Empty;

            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual(url, normalizedUrl);
        }

        [TestMethod]
        public void TryNormalizedrlForYouTuDotBeUrlShouldReturnNormalizedUrl()
        {
            string url = "http://youtu.be/12345";

            string normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }

        [TestMethod]
        public void TryNormalizedUrlForMobileLinkShouldReturnNormalizedUrl()
        {
            string url = "http://m.youtube.com/?v=12345";

            string normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));

            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }

        [TestMethod]
        public void GetNormalizedYouTubeUrlForBadLinkShouldReturnNull()
        {
            string url = "http://notAYouTubeUrl.com";

            string normalizedUrl = String.Empty;
            Assert.IsFalse(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.IsNull(normalizedUrl);
        }
    }
}
