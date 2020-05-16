@echo on



chdir > cdir.txt
set /p CDIR=<cdir.txt
del /F cdir.txt

rmdir /Q /S exe
md exe > nul 2> nul
md exe\spec > nul 2> nul
pyinstaller --onefile --distpath %CDIR%\exe --noconfirm --log-level INFO -n CC_GenRandom --specpath %CDIR%\exe\spec  -i %CDIR%\icons\hackerimage_Stk_icon.ico %CDIR%\main.py && ^
rmdir /Q /S build
rmdir /Q /S exe\spec

