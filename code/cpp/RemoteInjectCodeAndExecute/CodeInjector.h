/*
LICENSE INFORMATION

Copyright 2020 Kaizen Cyber Ops, LLC.

File Name: CodeInjector.h
Associated File(s): CodeInector.cpp 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

https://opensource.org/licenses/MIT
*/


/*
	This program will read a binary from disk and then inject it into another process using CreateRemoteProcess and CreateRemoteThread
	
	Syntax:
		CodeInjector.exe [-f|-p] [arg] [shellcode-file]

	Flags:

		-f		Spawn a new program and inject shellcode into the newly created executable.  Arg specifies the path of the target victim executable.
		-p		Specify that the shellcode will be injected into a currently running process.  Requires SeDebugPrivilege.  Arg specifies the pid of the target victim process.

	Usage:
		CodeInjector.exe [(-f [path-to-executable])|(-p [process pid])] [path-to-shellcode]
	
	Example:
		
		To spawn cmd.exe process and inject:
			CodeInjector.exe -f C:\Windows\System32\Cmd.exe .\shellcode.bin

		To inject into existing process #1867:
			CodeInjector.exe -p 1867 .\shellcode.bin
*/


/*WINDOWS x64 PLATFORMS ONLY CURRENTLY*/
#ifdef _WIN64		
#define ARGC 4
#include <windows.h>
#include <strsafe.h>
#include <Processthreadsapi.h>
#include <iostream>
using namespace std;

/*STRUCT DEFINITIONS*/
typedef struct InjectorInfo {
	HANDLE victimH;
	HANDLE threadH;
	char* shellcode_bytes;
	size_t shellcode_size;
	LPVOID injected_code_address;
	bool inject_using_pid;
};

/*DECLARE GLOBAL VARIABLES*/
char** args{};  //global holds a copy of the cli args
InjectorInfo* pInfo;	//global struct holds the necessary data that will be used by the procedures

/*FUNCTION DECLARATIONS*/
void Stage();
void ProcessArgs(char* argv[], int argc);
void CheckFile(int index);  //arg parameter is the index of the file path from argv
void ReadShellCode();
void PrepareVictim();
void SpawnProc();
void GetHandleOfRunningProc();
void InjectAndX();
void FailureAndExit(char* msg);
void CleanUp();
#endif






