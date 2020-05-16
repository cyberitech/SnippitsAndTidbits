#ifndef CMDEXECPIPED_H
#define CMDEXECPIPED_H
std::string exec(const char* cmd) {
	char buffer[128];
	std::string result = "";
	FILE* pipe = _popen(cmd, "r");
	if (!pipe) throw std::runtime_error("popen() failed!");
	try {
		while (fgets(buffer, sizeof buffer, pipe) != NULL) {
			result += buffer;
		}
	}
	catch (...) {
		_pclose(pipe);
		throw;
	}
	_pclose(pipe);
	return result;
}

std::string exec(const std::string cmd) {
	char buffer[128];
	std::string result = "";
	FILE* pipe = _popen(cmd.c_str(), "r");
	if (!pipe) throw std::runtime_error("popen() failed!");
	try {
		while (fgets(buffer, sizeof buffer, pipe) != NULL) {
			result += buffer;
		}
	}
	catch (...) {
		_pclose(pipe);
		throw;
	}
	_pclose(pipe);
	return result;
}

bool exec(const std::string cmd,char* (&inout), size_t inoutSize) {
	char buffer[128];
	std::string result = "";
	FILE* pipe = _popen(cmd.c_str(), "r");
	if (!pipe) return false;
	try {
		while (fgets(buffer, sizeof buffer, pipe) != NULL) {
			result += buffer;
		}
	}
	catch (...) {
		_pclose(pipe);
		return false;
	}
	_pclose(pipe);
	if (inoutSize < result.size()+1)
		return false;
	strncat_s(inout,inoutSize,result.c_str(),result.size());
	return true;
}
#endif