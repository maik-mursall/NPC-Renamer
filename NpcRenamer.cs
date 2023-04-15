using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NPCRenamer;

// ReSharper disable once ClassNeverInstantiated.Global
public class NpcRenamer : Mod
{
    internal static NpcRenamer Instance;
    
    internal ModKeybind ToggleNpcRenamerHotKey;

    public override void Load()
    {
        base.Load();
        
        Instance = this;
        
        ToggleNpcRenamerHotKey = KeybindLoader.RegisterKeybind(this, "ToggleNpcRenamer", "F3");
    }

    public override void Unload()
    {
        base.Unload();

        Instance = null;
        ToggleNpcRenamerHotKey = null;
    }

    internal void ChangeNpcName(NPC npc, string newName)
    {
        npc.GivenName = newName;
        
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            SendNpcNameChangePacket(npc);
        }
    }

    private void SendNpcNameChangePacket(NPC npc)
    {
        var packet = GetPacket();
        packet.Write(npc.netID);
        packet.Write(npc.GivenName);
        packet.Send();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        var netId = reader.ReadInt32();
        var newName = reader.ReadString();
        
        var npcs = Main.npc;
        
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i].netID == netId)
            {
                npcs[i].GivenName = newName;

                if (Main.netMode == NetmodeID.Server)
                {
                    SendNpcNameChangePacket(npcs[i]);
                }

                break;
            }
        }
    }
}
