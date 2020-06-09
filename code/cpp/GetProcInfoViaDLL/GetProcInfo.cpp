/*
LICENSE INFORMATION

Copyright 2020 Kaizen Cyber Ops, LLC.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

https://opensource.org/licenses/MIT
*/

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