# YoutubeExtractor

<a href="http://flattr.com/thing/1093085/" target="_blank">
<img src="http://api.flattr.com/button/flattr-badge-large.png" alt="Flattr this" title="Flattr this" border="0" />
</a>

## Overview
YoutubeExtractor is a library for .NET, written in C#, that allows to download videos from YouTube and/or extract their audio track (audio extraction currently only for flash videos).

## Target platforms

- .NET Framework 3.5 and higher
- Windows Phone 8
- WinRT
- Xamarin.Android
- Xamarin.iOS

Note that Windows Phone 8, WinRT, Xamarin.Android and Xamarin.iOS only support the extraction of the download URLs

## NuGet

[YoutubeExtractor at NuGet](http://nuget.org/packages/YoutubeExtractor)

    Install-Package YoutubeExtractor

## License

YoutubeExtractor has two licenses;

The YouTube URL-extraction code is licensed under the [MIT License](http://opensource.org/licenses/MIT)

The audio extraction code that is originally from [FlvExtract](http://moitah.net/) is licenced under the [GNU General Public License version 2 (GPLv2)](http://opensource.org/licenses/gpl-2.0)

Files that are GPLv2 licensed are explicitely marked with the GPLv2 header at the top of the file. All other files are implicitely MIT licensed.

## Credits

- [FlvExtract](http://moitah.net/) for extracting MP3 and AAC audio tracks out of flash files.

## Example code

**Get the download URLs**

```c#

// Our test youtube link
string link = "insert youtube link";

/*
 * Get the available video formats.
 * We'll work with them in the video and audio download examples.
 */
IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

```

**Download the video**

```c#

/*
 * Select the first .mp4 video with 360p resolution
 */
VideoInfo video = videoInfos
    .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

/*
 * Create the video downloader.
 * The first argument is the video to download.
 * The second argument is the path to save the video file.
 */
var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));

// Register the ProgressChanged event and print the current progress
videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

/*
 * Execute the video downloader.
 * For GUI applications note, that this method runs synchronously.
 */
videoDownloader.Execute();

```

**Download the audio track**

```c#

/*
 * We want the first extractable video with the highest audio quality.
 */
VideoInfo video = videoInfos
    .Where(info => info.CanExtractAudio)
    .OrderByDescending(info => info.AudioBitrate)
    .First();

/*
 * Create the audio downloader.
 * The first argument is the video where the audio should be extracted from.
 * The second argument is the path to save the audio file.
 */
var audioDownloader = new AudioDownloader(video, Path.Combine("D:/Downloads", video.Title + video.AudioExtension));

// Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
// because the download will take much longer than the audio extraction.
audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

/*
 * Execute the audio downloader.
 * For GUI applications note, that this method runs synchronously.
 */
audioDownloader.Execute();

```
