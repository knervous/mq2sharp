public class Sharp : MQEventHandler
{
    public override void Main()
    {
        Logger.Log("Called main function in Sharp - CHANGED");
    }

    public override void SetGameState(int eventType)
    {
        base.SetGameState(eventType);
    }

    public override void AddSpawn(PlayerClient spawn)  
    {
        Logger.Log($"Spawn added {spawn.Name}"); 
    }

    public override void RemoveSpawn(PlayerClient spawn) 
    {
        Logger.Log($"Spawn removed {spawn.Name}");
    }

    public override void WriteChatColor(string msg, int color, int filter)
    {
        base.WriteChatColor(msg, color, filter);
    }

    public override bool IncomingChat(string msg, ulong color)
    {
        Logger.Log($"Got incoming chat: {msg}");
        return base.IncomingChat(msg, color);
    }

    public override void UpdateImGui()
    {

    }

    public override void LoadPlugin(string plugin)
    {
    }
    public override void UnloadPlugin(string plugin)
    {
    }
    public override void Pulse()
    {
    }
    public override void Zoned()
    {
    }
    public override void MacroStart(string macro)
    {
    }
    public override void MacroStop(string macro)
    {
    }

    public override void AddGroundItem()
    {
    }
    public override void RemoveGroundItem()
    {
    }
    public override void BeginZone()
    {
    }
    public override void CleanUI()
    {
    }
    public override void DrawHUD()
    {
    }
    public override void ReloadUI()
    {
    }
    public override void EndZone()
    {
    }
}