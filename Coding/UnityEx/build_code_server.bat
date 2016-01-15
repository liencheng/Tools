@echo off
rem mdb files will be generated after all the copy is done.
rem the build configuration, i google all over the web and find out to add
rem '/property:Configuration=XXX' to commandline will cause the build process to
rem use XXX as the build configuration, and i tried, nothing happened, it's 
rem always been 'Debug', totally have no idea.
if NOT "%1" EQU "" (
  set cfg=%1
) else (
  set cfg=Debug
)
if NOT "%2" EQU "" (
  set is_pause=%2
) else (
  set is_pause=True
)

rem commandlien parameters
rem %0 --rebuild --configuration Debug|Release --copy-only

rem lang: chs cht kr
set lang=chs

rem working directory
set workdir=%~dp0

set clires=%workdir%\Client\Publish\Assets\StreamingAssets
set plugindir=%workdir%\Client\Publish\Assets\Plugins
set resdir=%workdir%\Public\Resource

set svrbin=%workdir%\DcoreEnv\bin
set logdir=%workdir%\BuildLog
set libdir=%workdir%\Public\CSharpLibs

rem xbuild is copy from mono-3.0.3/lib/mono/4.5
rem this xbuild will probably not work in a clean machine
set xbuild=%workdir%\Tools\xbuild\xbuild.exe

rem mdb generator
set pdb2mdb=%workdir%\Client\Tools\mono.exe %workdir%\Client\lib\mono\4.0\pdb2mdb.exe

rem resource copy and convert *.txt from ansi to utf-8
if "%cfg%" EQU "Release" (
  set rescopy=%workdir%\Tools\Ascii2Utf8\bin\Debug\Ascii2Utf8.exe
) else (
  set rescopy=%workdir%\Tools\Ascii2Utf8\bin\Release\Ascii2Utf8.exe
)

rem extract res cache
set anlysisres=%workdir%\Tools\ResCache\bin\Debug\ResCache.exe
set i18n_tool=%workdir%\Tools\I18NDictTableGenerator\I18NDictTableGenerator\bin\Debug\I18NDictTableGenerator.exe
set i18ndir=%workdir%\Public\I18N

rem show xbuild version
%xbuild% /version
echo.

rem make build log dir
mkdir %logdir%

rem 1.Lobby.sln, DashFireServer.sln
rem 2. update output(dll/pdb) to dinary directory
rem 3. generate mdb at binary directory according to pdb files
rem 4. copy resource files


echo building Lobby.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\Lobby.sln.log;Encoding=UTF-8 ^
         %workdir%\Lobby\Lobby.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\Lobby.sln.log.
  goto error_end
) else (
  echo done.
)

echo "update binaries"
xcopy %workdir%\Lobby\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo building BigworldLobby.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\BigworldLobby.sln.log;Encoding=UTF-8 ^
         %workdir%\BigworldLobby\BigworldLobby.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\BigworldLobby.sln.log.
  goto error_end
) else (
  echo done.
)

echo "update binaries"
xcopy %workdir%\BigworldLobby\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo building DashFireServer.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\DashFireServer.sln.log;Encoding=UTF-8 ^
         %workdir%\Server\src\DashFireServer.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\DashFireServer.sln.log.
  goto error_end
) else (
  echo done.
)

echo "update binaries"
xcopy %workdir%\Server\src\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo [server]: generate *mdb debug files for mono
pushd %svrbin%
for /r %%i in (*.pdb) do (
echo generate mdb for %%~dpni.dll
%pdb2mdb% %%~dpni.dll
)
popd
echo done. & echo.

rem echo anlysisres resource files
rem %anlysisres%


rem echo d | xcopy DcoreEnv\bin\data DcoreEnv\binBigWorld\data /S/Q/Y
echo d | xcopy DcoreEnv\bin\nodejs DcoreEnv\binBigWorld\nodejs /S/Q/Y
echo f | xcopy DcoreEnv\bin\*.dll DcoreEnv\binBigWorld\ /Q/Y
echo f | xcopy DcoreEnv\bin\*.exe DcoreEnv\binBigWorld\ /Q/Y
echo f | xcopy DcoreEnv\bin\*.pdb DcoreEnv\binBigWorld\ /Q/Y

goto good_end

:error_end
set ec=1
goto end
:good_end
set ec=0
echo All Done, Good to Go.
:end
if %is_pause% EQU True (
  pause
  exit /b %ec%
)
