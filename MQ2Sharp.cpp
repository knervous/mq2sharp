#include <mq/Plugin.h>
#include "dotnet_runtime.h"

PreSetup("MQ2Sharp");
PLUGIN_VERSION(0.1);


PLUGIN_API void InitializePlugin()
{
	DebugSpewAlways("MQ2Sharp::Initializing version 123 %f", MQ2Version);
	initialize();
}

PLUGIN_API void ShutdownPlugin()
{
	DebugSpewAlways("MQ2Sharp::Shutting down"); 
	shutdown();
}

PLUGIN_API void OnCleanUI()
{
	mq_event_fn(mq_event{ EventSubtype::Event_CleanUI });
}

PLUGIN_API void OnReloadUI()
{
	mq_event_fn(mq_event{ EventSubtype::Event_ReloadUI });
}

PLUGIN_API void OnDrawHUD()
{
	mq_event_fn(mq_event{ EventSubtype::Event_DrawHUD });
}

PLUGIN_API void SetGameState(int GameState)
{
	game_state_event_fn(gamestate_event{ EventSubtype::Event_SetGameState, GameState });
}

PLUGIN_API void OnPulse()
{
	mq_event_fn(mq_event{ EventSubtype::Event_Pulse });
}

PLUGIN_API void OnWriteChatColor(const char* Line, int Color, int Filter)
{
	write_chat_color_event_fn(chat_color_event{ EventSubtype::Event_WriteChatColor, Line, Color, Filter });
}

PLUGIN_API bool OnIncomingChat(const char* Line, DWORD Color)
{
	return incoming_chat_event_fn(incoming_chat_event{ EventSubtype::Event_IncomingChat, Line, Color });
}

PLUGIN_API void OnAddSpawn(PSPAWNINFO pNewSpawn)
{
	spawn_event_fn(spawn_event{ EventSubtype::Event_AddSpawn, pNewSpawn });
}

PLUGIN_API void OnRemoveSpawn(PSPAWNINFO pSpawn)
{
	spawn_event_fn(spawn_event{ EventSubtype::Event_RemoveSpawn, pSpawn });
}

PLUGIN_API void OnAddGroundItem(PGROUNDITEM pNewGroundItem)
{
	ground_item_event_fn(ground_item_event{ EventSubtype::Event_AddGroundItem, pNewGroundItem });
}

PLUGIN_API void OnRemoveGroundItem(PGROUNDITEM pGroundItem)
{
	ground_item_event_fn(ground_item_event{ EventSubtype::Event_AddGroundItem, pGroundItem });
}

PLUGIN_API void OnBeginZone()
{
	mq_event_fn(mq_event{ EventSubtype::Event_BeginZone });
}

PLUGIN_API void OnEndZone()
{
	mq_event_fn(mq_event{ EventSubtype::Event_EndZone });
}

PLUGIN_API void OnZoned()
{
	mq_event_fn(mq_event{ EventSubtype::Event_Zoned });
}

PLUGIN_API void OnUpdateImGui()
{
	mq_event_fn(mq_event{ EventSubtype::Event_UpdateImGui });
}


PLUGIN_API void OnMacroStart(const char* Name)
{
	string_event_fn(string_event{ EventSubtype::Event_MacroStart, Name });
}

PLUGIN_API void OnMacroStop(const char* Name)
{
	string_event_fn(string_event{ EventSubtype::Event_MacroStop, Name });
}


PLUGIN_API void OnLoadPlugin(const char* Name)
{
	string_event_fn(string_event{ EventSubtype::Event_LoadPlugin, Name });
}

PLUGIN_API void OnUnloadPlugin(const char* Name)
{
	string_event_fn(string_event{ EventSubtype::Event_UnloadPlugin, Name });
}