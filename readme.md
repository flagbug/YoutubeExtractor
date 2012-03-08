# YouTubeExtractor

## Overview
YoutubeExtractor is a reusable library for .NET, written in C#, that allows to download videos from YouTube and/or extract their audio track (currently only for flash videos).

The original project, youtubeFisher, is a pure GUI application, but there was the need for a reusable library.

## Authors
All credits go to the [youtubeFisher](http://youtubefisher.codeplex.com/) project.

I, flagbug, just extracted the code out of this project, cleaned it up and packaged it into reusable classes.

## License

YouTubeExtractor is licenced under the [GNU General Public License version 2 (GPLv2)](http://opensource.org/licenses/gpl-2.0)

## Dependencies

- .NET Framework 3.5
- [JSON.NET](http://json.codeplex.com/)

## NuGet

YoutubeExtractor is available on [NuGet!](http://nuget.org/packages/YoutubeExtractor)

## Example code

**Get the download URLs**

```c#

	// Our test youtube link
	const string link = "http://www.youtube.com/watch?v=6bMmhKz6KXg";
	
	/*
	 * Get the available video formats.
	 * We'll work with them in the video and audio download examples.
	 */
	IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
```

**Download the video**

```c#

	/*
	 * Select the standard youtube quality
	 * See the VideoFormat enum for more info about the quality.
	 */
	VideoInfo video = videoInfos
	    .First(info => info.VideoFormat == VideoFormat.Standard360);
	
	/*
	 * Create the video downloader.
	 * The first argument is the video to download.
	 * The second argument is the path to save the video file.
	 * Automatic video title infering will be supported later.
	 */
	var videoDownloader = new VideoDownloader(video, "D:/Downloads/test" + video.VideoExtension);
	
	// Register the ProgressChanged event and print the current progress
	videoDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
	
	/*
	 * Execute the video downloader.
	 * For GUI applications note, that this method runs synchronously.
	 */
	videoDownloader.Execute();
```

**Download the audio track**

```c#

	/*
	 * We want the first flash (only flash audio extraction is currently supported)
	 * video with the highest audio quality.
	 * See the VideoFormat enum for more info about the quality.
	 */
	VideoInfo video = videoInfos
	    .Where(info => info.CanExtractAudio)
	    .First(info =>
	           info.VideoFormat == VideoFormat.FlashAacHighQuality ||
	           info.VideoFormat == VideoFormat.FlashAacLowQuality ||
	           info.VideoFormat == VideoFormat.FlashMp3HighQuality ||
	           info.VideoFormat == VideoFormat.FlashMp3LowQuality);
	
	/*
	 * Create the audio downloader.
	 * The first argument is the video where the audio should be extracted from.
	 * The second argument is the path to save the audio file.
	 * Automatic video title infering will be supported later.
	 */
	var audioDownloader = new AudioDownloader(video, "D:/Downloads/test" + video.AudioExtension);
	
	// Register the ProgressChanged event and print the current progress
	audioDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
	
	/*
	 * Execute the audio downloader.
	 * For GUI applications note, that this method runs synchronously.
	 */
	audioDownloader.Execute();
```