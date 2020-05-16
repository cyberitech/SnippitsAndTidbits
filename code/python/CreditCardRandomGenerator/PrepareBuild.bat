@echo on
md ccgen-venv

virtualenv ccgen-venv
ccgen-venv\Scripts\activate.bat && pip install pywin32 pypiwin32 rstr pyinstaller > nul  2> nul
