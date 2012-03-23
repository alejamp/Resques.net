@echo off

rem See http://support.microsoft.com/kb/556009 for details
 
set regqry=HKLM\Hardware\Description\System\CentralProcessor\0
 
reg.exe query %regqry% > checkos.txt
 
find /i "x86" < checkos.txt > stringcheck.txt
 
if %errorlevel% == 0 (
    pushd 32bit
) else (
    pushd 64bit
)
 
del /q ..\checkos.txt
del /q ..\stringcheck.txt
 
redis-server ..\redis.conf
