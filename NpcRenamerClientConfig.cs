using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NPCRenamer;

// ReSharper disable once ClassNeverInstantiated.Global
class NpcRenamerClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Automatic Settings")]
    // non-player specific stuff:
    [DefaultValue(typeof(Vector2), "1100, 100")]
    [Range(0f, 1920f)]
    [Label("NPC Renamer Position")]
    [Tooltip("Position of the NPC Renamer UI. This will automatically save, no need to adjust")]
    public Vector2 NpcRenamerPosition { get; set; }
    
    [DefaultValue(typeof(Vector2), "400, 300")]
    [Range(200f, 1920f)]
    [Label("NPC Renamer Size")]
    [Tooltip("Size of the NPC Renamer UI. This will automatically save, no need to adjust")]
    public Vector2 NpcRenamerSize { get; set; }

    internal static void SaveConfig()
    {
        // in-game ModConfig saving from mod code is not supported yet in tmodloader, and subject to change, so we need to be extra careful.
        // This code only supports client configs, and doesn't call onchanged. It also doesn't support ReloadRequired or anything else.
        MethodInfo saveMethodInfo =
            typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
        if (saveMethodInfo != null)
            saveMethodInfo.Invoke(null, new object[] { ModContent.GetInstance<NpcRenamerClientConfig>() });
        else
            NpcRenamer.Instance.Logger.Warn("In-game SaveConfig failed, code update required");
    }
}