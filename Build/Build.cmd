@echo on
call "%VS110COMNTOOLS%vsvars32.bat"

msbuild.exe /ToolsVersion:4.0 "..\YoutubeExtractor\YoutubeExtractor\YoutubeExtractor.csproj" /p:configuration=Release

mkdir ..\Release

copy ..\YoutubeExtractor\YoutubeExtractor\bin\Release\YoutubeExtractor.dll ..\Release\YoutubeExtractor.dll
copy ..\YoutubeExtractor\YoutubeExtractor\bin\Release\YoutubeExtractor.xml ..\Release\YoutubeExtractor.xml

pause