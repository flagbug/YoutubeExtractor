# v0.7.0

## Notes
This version does NOT fix the various problems that occur because of the latest YouTube changes.

## Features
- The portable version of YoutubeExtractor now supports Xamarin.Android and 
  Xamarin.iOS
- Added an optional constructor argument to specify the exact number of bytes 
  to download.

## Improvements
- Better YouTube URL regognition.

## Changes
- DownloadUrlResolver now throws the `VideoNotAvailableException` with the
  reason for the request failure.
- Removed the obsolete `ProgressChanged` event and `OnProgressChanged` method
  from the `Downloader` class.

## Bugfixes
- Fixed the resource disposal in the `VideoDownloader` class.

# v0.6.2

## Bugfixes
- Fixed the build again...

# v0.6.1

## Bugfixes
- Fixed a bug in the download url extraction code that was caused by a faulty 
  build.

# v0.6.0

## Features
- WinRT and Windows Phone 8 support

# v0.5.0

## Features
- Added a cancel option to the ProgressEventArgs class for canceling the 
  download.
  
## Bugfixes
- Fixed disposal of some resources.

# v0.4.2

## Bugfixes
- Fixed disposal of resources.

# v0.4.1

## Bugfixes
- Fixed the parsing of the download urls, as YouTube changed their backend.

# v0.4.0

## Changes
- The VideoDownloader and the AudioDownloader classes now implement their own 
  progress events. This means that the old ProgressChanged event is now
  obsolete.
  This change was made, because the ProgressChanged event reported the first 
  50% as download progress and the second 50% as audio extraction progress.

  This was really inaccurate, as the download progress would be very slow in 
  relation to the audio extraction progress, so the AudioDownloader has now 
  two separate events: 
  DownloadProgressChanged and AudioExctractionProgressChanged.
  
  The VideoDownloader has the ProgressChanged event replaced with the 
  DownloadProgressChanged event, but they are basically the same, 
  so the real change is made to the AudioDownloader class.

# v0.3.4

## Bugfixes
- Fixed a bug, that caused a 403 forbidden exception.
- Fixed a bug, that caused a NullReferenceExcepion when the download has 
  finished.

# v0.3.3

## Changes
- The AudioDownloader.Execute method can now throw the 
  "AudioExtractionException", that is thrown when an error occurs during audio 
  extraction.

## Improvements
- Added more documentation for exception handling.

## Bugfixes
- The VideoDownloader.Execute method doesn't swallow exceptions anymore.

# v0.3.2

## Bugfixes
- Fixed a bug, that caused a YoutubeParseException for some videos.

# v0.3.1

## Changes
- Removed dependency to Newtonsoft.Json.

## Bugfixes
- Fixed the parsing of the download urls, as YouTube changed their layout.

# v0.3.0

## NOTES
- **Warning:** This release contains breaking changes!
- The complete VideoInfo class has been overhauled, to support new formats and 
  features.

## Features
- New video formats, especially 3D formats.
- New audio type: Ogg Vorbis
- The new property "Resolution" returns the video resolution, for example 1080 
  for a video with a resolution of 1080p.
- The new property "Is3D" indicates, if a video is 3D
- The new property "AudioType" returnsthe audio encoding (Mp3, Aac, Vorbis)
- The new property "AudioBitrate" returns approximate the audio bitrate in 
  kbit/s.

## Changes
- The "VideoFormat" property has been completely removed.

# v0.2.1

## Changes
- The DownloadUrlResolver.GetDownloadUrls method now throws the 
  "YoutubeParseException", if the YouTube page could not be parsed.

## Bugfixes
- Fixed a bug, that caused the DownloadUrlResolver.GetDownloadUrls method to 
  randomly throw ArgumentOutOfRangeException.

# v0.2.0

## Features
- Added support for full HD WebM (format code 46).
- Added support for some 3GP format (format code 36).

## Changes
- DownloadUrlResolver.GetDownloadUrls now throws ArgumentException, instead of
  InvalidOperationException, if the YouTube url is not valid.
- Updated JSON.NET to version 4.5.7

# v0.1.0

## Features
- Added the "Title" property in the Videoinfo class, that returns the title 
  of the video.
- The DownloadUrlResolver.GetDownloadUrls method is less strict with the 
  videoUrl argument and also accepts short-URLs (youtu.be)

## Improvements
- Added more documentation (especially method XML-comments)
