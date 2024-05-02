#pragma once
#ifndef DOTNET_RUNTIME_H
#define DOTNET_RUNTIME_H

#include <mq/Plugin.h>

enum EventSubtype : int {
	Event_CleanUI = 0,
	Event_ReloadUI,
	Event_DrawHUD,
	Event_SetGameState,
	Event_Pulse,
	Event_WriteChatColor,
	Event_IncomingChat,
	Event_AddSpawn,
	Event_RemoveSpawn,
	Event_AddGroundItem,
	Event_RemoveGroundItem,
	Event_BeginZone,
	Event_EndZone,
	Event_Zoned,
	Event_UpdateImGui,
	Event_MacroStart,
	Event_MacroStop,
	Event_LoadPlugin,
	Event_UnloadPlugin
};

struct mq_event {
	EventSubtype subtype;
};

struct gamestate_event : mq_event {
	int event_type;
};

struct chat_color_event : mq_event {
	const char* line;
	int color;
	int filter;
};

struct incoming_chat_event : mq_event {
	const char* line;
	unsigned long color;
};

struct spawn_event : mq_event {
	eqlib::PlayerClient* spawn;
};

struct ground_item_event : mq_event {
	eqlib::EQGroundItem* item;
};

struct string_event : mq_event {
	const char* msg;
};

void mq_event_fn(mq_event evt);
void game_state_event_fn(gamestate_event evt);
void write_chat_color_event_fn(chat_color_event evt);
bool incoming_chat_event_fn(incoming_chat_event evt);
void spawn_event_fn(spawn_event evt);
void ground_item_event_fn(ground_item_event evt);
void string_event_fn(string_event evt);

void initialize();
void shutdown();

#endif