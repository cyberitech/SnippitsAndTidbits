#pragma once
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

class CommandShell
{
public:
	CommandShell();
	~CommandShell();
	char* exec(char* command, char* outBuf);
private:
	bool initSuccess;
	HANDLE readH;
	HANDLE writeH;
	HANDLE stdH;
	DWORD bytesRead;
	DWORD retCode;
	SECURITY_ATTRIBUTES secAtt;
	LPPROCESS_INFORMATION procInfo;
	STARTUPINFO startupInfo;
public:
	char* exec(char* command);
	bool spawnProcess();
};

