#pragma once
#include <stdio.h>
#include <stdlib.h>

#include "CommandShell.h"



CommandShell::CommandShell():bytesRead(0),retCode(0), initSuccess(false)
{
	ZeroMemory(&secAtt, sizeof(secAtt));
	ZeroMemory(&procInfo, sizeof(procInfo));
	ZeroMemory(&startupInfo, sizeof(startupInfo));
	secAtt.bInheritHandle = true;
	secAtt.lpSecurityDescriptor = NULL;
	secAtt.nLength = sizeof(STARTUPINFO);
	startupInfo.cb = sizeof(STARTUPINFO);
	startupInfo.dwFlags = STARTF_USESHOWWINDOW;
	startupInfo.wShowWindow = SW_HIDE;
}


CommandShell::~CommandShell()
{
}


char* CommandShell::exec(char* command, char* outBuf)
{
	// TODO: Add your implementation code here.
	return nullptr;
}


bool CommandShell::spawnProcess()
{
	bool success = CreatePipe(&readH, &writeH, &secAtt, NULL);
	if (!success)
	{
		initSuccess = false;
		puts("failed creating pipe");
		return false;
	}
	LPWSTR command =(LPWSTR) TEXT("cmd.exe");
	//success &= SetStdHandle(STD_OUTPUT_HANDLE, writeH) && CreateProcess(NULL,command,NULL,NULL,TRUE,0, &secAtt , &procInfo);

	return true;
}
