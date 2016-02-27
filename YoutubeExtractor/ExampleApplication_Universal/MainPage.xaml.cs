using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YoutubeExtractor;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExampleApplication_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IEnumerator<VideoInfo> videoEnumerator;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetVideoInfos();
        }

        private async void GetVideoInfos()
        {
            var videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("http://www.youtube.com/watch?v=fRh_vgS2dFE");
            videoEnumerator = videoInfos.GetEnumerator();
            videoEnumerator.MoveNext();
            PlayVideo(); 
        }

        private void PlayVideo()
        {
            var currentVideo = videoEnumerator.Current;
           
            if (currentVideo != null)
            {
                this.info.Text = currentVideo.ToString();
                System.Diagnostics.Debug.WriteLine(currentVideo);
                this.player.Source = new Uri(currentVideo.DownloadUrl);
            }
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Media Ended");
            videoEnumerator.MoveNext();
            PlayVideo();
        }

        private void player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Media failed");
            videoEnumerator.MoveNext();
            PlayVideo();
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Media Opened");
        }
    }
}
