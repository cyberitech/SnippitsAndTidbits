#include <windows.h>
#include <stdio.h>
#include <winternl.h>

typedef NTSTATUS(WINAPI* SysInfoProc)(SYSTEM_INFORMATION_CLASS, PVOID, ULONG, PULONG);

int main()
{

	HMODULE hmdl = NULL;
	hmdl = LoadLibrary("Ntdll.dll");
	if (NULL != hmdl)
	{
		SysInfoProc myproc = NULL;
		myproc = (SysInfoProc)GetProcAddress(hmdl, ("ZwQuerySystemInformation"));
		if (NULL != myproc)
		{
			NTSTATUS ntResult = -1;
			ULONG cbNeeded = 0;
			ntResult = (myproc)(SystemProcessInformation, NULL, cbNeeded, &cbNeeded);
			BYTE* pBuff = NULL;
			pBuff = new BYTE[cbNeeded];
			ntResult = (myproc)(SystemProcessInformation, pBuff, cbNeeded, &cbNeeded);
			if (ntResult == 0)
			{
				SYSTEM_PROCESS_INFORMATION* psys;
				HANDLE ProcID = 0;
				ULONG HndlCount = 0;
				ULONG offset = 0;
				while (cbNeeded > 0)
				{
					psys = (SYSTEM_PROCESS_INFORMATION*)pBuff;
					ProcID = psys->UniqueProcessId;
					HndlCount = psys->HandleCount;
					offset = psys->NextEntryOffset;
					pBuff += offset;
					cbNeeded -= offset;
				}
			}
			delete []pBuff;
		}
	}
	FreeLibrary(hmdl);
}