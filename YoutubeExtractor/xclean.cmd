@echo off
delete /y /d:q /m:d /s bin obj Debug Release Retail Binaries _UpgradeReport_Files Logs ipch nt32 lh32 lh64 x64 _ReSharper.*
delete /y /d:q /m:f /s *.pidb *.userprefs *.ncb *.suo *.user *.scc *.cache *.bak UpgradeLog.XML *.InstallState *.InstallLog *.sdf *.vssscc *.vspscc *_h.h *_i.c *_p.c dlldata.c
