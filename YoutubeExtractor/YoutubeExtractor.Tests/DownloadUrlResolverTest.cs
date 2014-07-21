using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using YoutubeExtractor;

namespace YoutubeExtractor.Tests
{
    /// <summary>
    /// Small series of unit tests for DownloadUrlResolver. Run these with NUnit.
    /// </summary>
    [TestFixture]
    public class DownloadUrlResolverTest
    {        
        [Test]
        public void TryNormalizedUrlForStandardYouTubeUrlShouldReturnSame()
        {
            string url = "http://youtube.com/watch?v=12345";            
            
            string normalizedUrl = String.Empty;

            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual(url, normalizedUrl);
        }
        
        [Test]
        public void TryNormalizedrlForYouTuDotBeUrlShouldReturnNormalizedUrl()
        {
            string url = "http://youtu.be/12345";
            
            string normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }
        
        [Test]
        public void TryNormalizedUrlForMobileLinkShouldReturnNormalizedUrl()
        {
            string url = "http://m.youtube.com/?v=12345";
            
            string normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));

            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }
        
        [Test]
        public void GetNormalizedYouTubeUrlForBadLinkShouldReturnNull()
        {
            string url = "http://notAYouTubeUrl.com";
           
            string normalizedUrl = String.Empty;
            Assert.IsFalse(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.IsNull(normalizedUrl);
        }
    }
}
