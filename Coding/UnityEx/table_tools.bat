@echo on
set workdir = %~dp0
rem dir
cd Tools
rem dir
.\genlist.sh
.\genlistall.sh
set TableReaderGen = %workdir%\Tools\TableReaderGenerator\bin\Debug\TableReaderGenerator.exe
echo %TableReaderGen% /gendsl
cd TableReaderGenerator\bin\Debug\
dir
TableReaderGenerator.exe gendsl
rem done.
pause