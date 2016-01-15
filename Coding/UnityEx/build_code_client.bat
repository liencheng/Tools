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

rem 1. build client.sln
rem 2. update output(dll/pdb) to dinary directory
rem 3. generate mdb at binary directory according to pdb files
rem 4. copy resource files

echo building Client.sln ...
%xbuild% /nologo /noconsolelogger /property:Configuration=%cfg% ^
         /flp:LogFile=%logdir%\Client.sln.log;Encoding=UTF-8 ^
		 /t:clean;rebuild ^
         %workdir%\Client\Client.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\Client.sln.log.
  goto error_end
) else (
  echo done.
)

echo [client]: generate *mdb debug files for mono

pushd %workdir%\Client\Src\bin\%cfg%
for /r %%i in (*.pdb) do (
  %pdb2mdb% %%~dpni.dll
)
popd
echo done. & echo.

rem copy dll to unity3d's plugin directory
echo "update binaries"
xcopy %workdir%\Client\Src\bin\%cfg%\*.dll %plugindir% /y /q
xcopy %workdir%\Client\Src\bin\%cfg%\*.mdb %plugindir% /y /q
del /a /f %plugindir%\Library.dll
del /a /f %plugindir%\UnityEngine.dll
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)


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
