#pragma once
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <stdio.h>
#include <stdlib.h>

class BinaryExecutor
{
public:
	bool isSuccess() { return success; }
	BinaryExecutor(const char* packedExe);

	~BinaryExecutor();
private:
	bool success;
	int RunPortableExecutable(const char* Image);


};

