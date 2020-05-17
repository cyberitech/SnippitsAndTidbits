// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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