// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Standard headers
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <chrono>
#include <iostream>
#include <thread>
#include <vector>
#include <filesystem>

#include "dotnet_runtime.h"

// Provided by the AppHost NuGet package and installed as an SDK pack
#include "deps/nethost.h"

// Header files copied from https://github.com/dotnet/core-setup
#include "deps/coreclr_delegates.h"
#include "deps/hostfxr.h"

#include <Windows.h>

// Determine which library version to link based on the target architecture
#if defined(_WIN32) && !defined(_WIN64)
#pragma comment(lib, "deps/x86/libnethost.lib")
#pragma comment(lib, "deps/x86/nethost.lib")
#elif defined(_WIN64)
#pragma comment(lib, "deps/x64/libnethost.lib")
#pragma comment(lib, "deps/x64/nethost.lib")
#endif

#define STR(s) L##s
#define CH(c) L##c
#define DIR_SEPARATOR L'\\'

#define string_compare wcscmp


using string_t = std::basic_string<char_t>;

namespace fs = std::filesystem;

namespace
{
	// Globals to hold hostfxr exports
	hostfxr_initialize_for_dotnet_command_line_fn init_for_cmd_line_fptr;
	hostfxr_initialize_for_runtime_config_fn init_for_config_fptr;
	hostfxr_get_runtime_delegate_fn get_delegate_fptr;
	hostfxr_run_app_fn run_app_fptr;
	hostfxr_close_fn close_fptr;

	// Forward declarations
	bool load_hostfxr(const char_t* app);
	load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* assembly);
}

struct lib_args
{
	const char_t* message;
	int number;
};

load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
get_function_pointer_fn runtime_function = nullptr;


typedef void(CORECLR_DELEGATE_CALLTYPE* initialize_fn_ptr)();
typedef void(CORECLR_DELEGATE_CALLTYPE* shutdown_fn_ptr)();
typedef void(CORECLR_DELEGATE_CALLTYPE* mq_event_fn_ptr)(mq_event args);
typedef void(CORECLR_DELEGATE_CALLTYPE* gamestate_event_fn_ptr)(gamestate_event args);
typedef void(CORECLR_DELEGATE_CALLTYPE* chat_color_event_fn_ptr)(chat_color_event args);
typedef bool(CORECLR_DELEGATE_CALLTYPE* incoming_chat_event_fn_ptr)(incoming_chat_event args);
typedef void(CORECLR_DELEGATE_CALLTYPE* spawn_event_fn_ptr)(spawn_event args);
typedef void(CORECLR_DELEGATE_CALLTYPE* ground_item_event_fn_ptr)(ground_item_event args);
typedef void(CORECLR_DELEGATE_CALLTYPE* string_event_fn_ptr)(string_event args);

// Initialize callback pointers
mq_event_fn_ptr mq_event_callback = nullptr;
shutdown_fn_ptr reload_callback = nullptr;
initialize_fn_ptr init_callback = nullptr;
gamestate_event_fn_ptr gamestate_event_callback = nullptr;
chat_color_event_fn_ptr chat_color_event_callback = nullptr;
incoming_chat_event_fn_ptr incoming_chat_event_callback = nullptr;
spawn_event_fn_ptr spawn_event_callback = nullptr;
ground_item_event_fn_ptr ground_item_event_callback = nullptr;
string_event_fn_ptr string_event_callback = nullptr;


const char_t* dotnet_type = STR("MQInterface, DotNetBridge");
bool initialized = false;

void mq_event_fn(mq_event evt)
{
	if (!initialized)
	{
		return;
	}

	if (mq_event_callback != nullptr)
	{
		mq_event_callback(evt);
		return;
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("MQEventMethod") /*method_name*/,
		STR("MQInterface+MQEventDelegate, DotNetBridge") /*delegate_type_name*/,
		nullptr,
		nullptr,
		(void**)&mq_event_callback);

	if (rc != 0)
	{
		std::cerr << "Get delegate failed for mq event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	mq_event_callback(evt);

	return;
}

void game_state_event_fn(gamestate_event evt) {
	if (!initialized) {
		return;
	}

	if (gamestate_event_callback != nullptr) {
		gamestate_event_callback(evt);
		return;
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("GameStateEventMethod"),
		STR("MQInterface+GameStateEventDelegate, DotNetBridge"),
		nullptr,
		nullptr,
		(void**)&gamestate_event_callback);

	if (rc != 0) {
		std::cerr << "Get delegate failed for game state event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	else {
		gamestate_event_callback(evt);
	}
}


void write_chat_color_event_fn(chat_color_event evt) {
    if (!initialized) {
        return;
    }

    if (chat_color_event_callback != nullptr) {
        chat_color_event_callback(evt);
        return;
    }

    auto rc = runtime_function(
        dotnet_type,
        STR("ChatColorEventMethod"),
        STR("MQInterface+ChatColorEventDelegate, DotNetBridge"),
        nullptr,
        nullptr,
        (void**)&chat_color_event_callback);

    if (rc != 0) {
        std::cerr << "Get delegate failed for chat color event delegate: " << std::hex << std::showbase << rc << std::endl;
    } else {
        chat_color_event_callback(evt);
    }
}

bool incoming_chat_event_fn(incoming_chat_event evt) {
	if (!initialized) {
		return false;
	}

	if (incoming_chat_event_callback != nullptr) {
		return incoming_chat_event_callback(evt);
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("IncomingChatEventMethod"),
		STR("MQInterface+IncomingChatEventDelegate, DotNetBridge"),
		nullptr,
		nullptr,
		(void**)&incoming_chat_event_callback);

	if (rc != 0) {
		std::cerr << "Get delegate failed for incoming chat event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	else {
		return incoming_chat_event_callback(evt);
	}
	return false;
}

void spawn_event_fn(spawn_event evt) {
	if (!initialized) {
		return;
	}

	if (spawn_event_callback != nullptr) {
		spawn_event_callback(evt);
		return;
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("SpawnEventMethod"),
		STR("MQInterface+SpawnEventDelegate, DotNetBridge"),
		nullptr,
		nullptr,
		(void**)&spawn_event_callback);

	if (rc != 0) {
		std::cerr << "Get delegate failed for spawn event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	else {
		spawn_event_callback(evt);
	}
}

void ground_item_event_fn(ground_item_event evt) {
	if (!initialized) {
		return;
	}

	if (ground_item_event_callback != nullptr) {
		ground_item_event_callback(evt);
		return;
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("GroundItemEventMethod"),
		STR("MQInterface+GroundItemEventDelegate, DotNetBridge"),
		nullptr,
		nullptr,
		(void**)&ground_item_event_callback);

	if (rc != 0) {
		std::cerr << "Get delegate failed for ground item event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	else {
		ground_item_event_callback(evt);
	}
}

void string_event_fn(string_event evt) {
	if (!initialized) {
		return;
	}

	if (string_event_callback != nullptr) {
		string_event_callback(evt);
		return;
	}

	auto rc = runtime_function(
		dotnet_type,
		STR("StringEventMethod"),
		STR("MQInterface+StringEventDelegate, DotNetBridge"),
		nullptr,
		nullptr,
		(void**)&string_event_callback);

	if (rc != 0) {
		std::cerr << "Get delegate failed for string event delegate: " << std::hex << std::showbase << rc << std::endl;
	}
	else {
		string_event_callback(evt);
	}
}

// TBD what we're doing here
void shutdown() {
}

std::string GetModuleDirectory() {
	char path[MAX_PATH] = { 0 };
	HMODULE hModule = NULL;

	// Get the handle to the current DLL or executable
	BOOL getModHandle = GetModuleHandleEx(
		GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
		GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
		(LPCSTR)&GetModuleDirectory,
		&hModule
	);

	if (getModHandle && hModule != NULL) {
		// Retrieve the path of the executable
		if (GetModuleFileName(hModule, path, MAX_PATH)) {
			std::string fullPath(path);

			// Find the last backslash (path separator)
			size_t pos = fullPath.find_last_of("\\/");
			if (pos != std::string::npos) {
				// Return the directory part only
				return fullPath.substr(0, pos);
			}
		}
		else {
			std::cerr << "Error getting module file name: " << GetLastError() << std::endl;
		}
	}
	else {
		std::cerr << "Error getting module handle." << std::endl;
	}

	return "";  // Return an empty string in case of failure
}

void initialize()
{
	if (initialized)
	{
		return;
	}

	std::string path = GetModuleDirectory();
	std::filesystem::path currentPath = std::filesystem::path(path).parent_path();
	std::filesystem::path dotnetPath; 
	const std::string roslyn_dll("DotNetBridge.dll");
	for (const auto& entry : fs::recursive_directory_iterator(currentPath, fs::directory_options::follow_directory_symlink)) {
		if (entry.is_regular_file() && entry.path().filename() == roslyn_dll) {
			dotnetPath = entry.path();
			break;
		}
	}
	if (dotnetPath.empty()) {
		printf("Could not locate DotNetBridge.dll from working directory %s\n", currentPath.c_str());
		return;
	}
	else {
		printf("Loading .NET lib at %s\n", dotnetPath.c_str());
	}
	//
	// STEP 1: Load HostFxr and get exported hosting functions
	//
	if (!load_hostfxr(dotnetPath.c_str()))
	{
		assert(false && "Failure: load_hostfxr()");
		return;
	}

	load_assembly_and_get_function_pointer = get_dotnet_load_assembly(dotnetPath.c_str());
	assert(load_assembly_and_get_function_pointer != nullptr && "Failure: get_dotnet_load_assembly()");

	auto rc = runtime_function(
		dotnet_type,
		STR("Initialize") /*method_name*/,
		STR("MQInterface+InitializeDelegate, DotNetBridge") /*delegate_type_name*/, 
		nullptr,
		nullptr,
		(void**)&init_callback);
	if (rc != 0)
	{
		std::cerr << "Get delegate failed for init: " << std::hex << std::showbase << rc << std::endl;
	}
	init_callback();
	initialized = true;

	return;
}

/********************************************************************************************
 * Function used to load and activate .NET Core
 ********************************************************************************************/

namespace
{
	// Forward declarations
	void* load_library(const char_t*);
	void* get_export(void*, const char*);

	void* load_library(const char_t* path)
	{
		HMODULE h = ::LoadLibraryW(path);
		assert(h != nullptr);
		return (void*)h;
	}
	void* get_export(void* h, const char* name)
	{
		void* f = ::GetProcAddress((HMODULE)h, name);
		assert(f != nullptr);
		return f;
	}

	// <SnippetLoadHostFxr>
	// Using the nethost library, discover the location of hostfxr and get exports
	bool load_hostfxr(const char_t* assembly_path)
	{
		get_hostfxr_parameters params{ sizeof(get_hostfxr_parameters), assembly_path, nullptr };
		// Pre-allocate a large buffer for the path to hostfxr
		char_t buffer[MAX_PATH];
		size_t buffer_size = sizeof(buffer) / sizeof(char_t);
		int rc = get_hostfxr_path(buffer, &buffer_size, &params);
		if (rc != 0)
			return false;

		// Load hostfxr and get desired exports
		void* lib = load_library(buffer);
		init_for_cmd_line_fptr = (hostfxr_initialize_for_dotnet_command_line_fn)get_export(lib, "hostfxr_initialize_for_dotnet_command_line");
		init_for_config_fptr = (hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
		get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
		run_app_fptr = (hostfxr_run_app_fn)get_export(lib, "hostfxr_run_app");
		close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

		return (init_for_config_fptr && get_delegate_fptr && close_fptr);
	}
	// </SnippetLoadHostFxr>

	// <SnippetInitialize>
	// Load and initialize .NET Core and get desired function pointer for scenario
	load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* config_path)
	{
		// Load .NET Core
		void* load_assembly_and_get_function_pointer = nullptr;
		hostfxr_handle cxt = nullptr;
		std::vector<const char_t*> args{ config_path, STR("app_arg_1"), STR("app_arg_2") };
		int rc = init_for_cmd_line_fptr(args.size(), args.data(), nullptr, &cxt);
		if (rc != 0 || cxt == nullptr)
		{
			std::cerr << "Init failed: " << std::hex << std::showbase << rc << std::endl;
			close_fptr(cxt);
			return nullptr;
		}

		// Get the load assembly function pointer
		rc = get_delegate_fptr(
			cxt,
			hdt_load_assembly_and_get_function_pointer,
			&load_assembly_and_get_function_pointer);
		if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
			std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;
		void* fn_ptr = nullptr;
		get_delegate_fptr(
			cxt,
			hdt_get_function_pointer,
			&fn_ptr);
		runtime_function = (get_function_pointer_fn)(fn_ptr);
		close_fptr(cxt);
		return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
	}

	// </SnippetInitialize>
}