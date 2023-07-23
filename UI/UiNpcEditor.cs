using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace NPCRenamer.UI;

public delegate void NpcNameChange(NPC npc, string newName);

public class UiNpcEditor : UIElement
{
    private readonly NPC _npc;

    private UISearchBar _nameTextInput;
    private UIImage _headImage;
    private string _temporaryName;

    public event NpcNameChange OnNpcNameChange;

    public UiNpcEditor(NPC npc)
    {
        _npc = npc;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);


        if (!_nameTextInput?.IsWritingText == true)
        {
            _nameTextInput.SetContents(_npc?.GivenName ?? "");
        }
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        _nameTextInput = new UISearchBar(LocalizedText.Empty, 1f);
        _nameTextInput.Width.Set(0, 1f);
        _nameTextInput.Height.Set(34, 0f);
        _nameTextInput.MarginLeft = 40f;
        _nameTextInput.PaddingRight = 40f;
        _nameTextInput.VAlign = 0.5f;
        _nameTextInput.OnEndTakingInput += NameTextInputOnOnEndTakingInput;
        _nameTextInput.OnContentsChanged += NameTextInputOnOnContentsChanged;
        _nameTextInput.OnClick += OnOnClick;
        _nameTextInput.SetContents(_npc.GivenName);
        Append(_nameTextInput);

        _headImage = GetNpcHeadImage();
        _headImage.Width.Set(34, 0f);
        _headImage.Height.Set(34, 0f);
        _headImage.VAlign = 0.5f;
        Append(_headImage);
    }

    private void OnOnClick(UIMouseEvent evt, UIElement listeningelement)
    {
        if (_npc == null) return;
        
        if (PlayerInput.WritingText) return;

        _temporaryName = _npc.GivenName;
        _nameTextInput.ToggleTakingText();
    }

    private void NameTextInputOnOnContentsChanged(string obj)
    {
        _temporaryName = obj;
    }

    private void NameTextInputOnOnEndTakingInput()
    {
        if (_npc == null) return;

        OnNpcNameChange?.Invoke(_npc, _temporaryName);
        _temporaryName = "";
    }

    private UIImage GetNpcHeadImage()
    {
        if (_npc.ModNPC != null)
        {
            // This NPC is an ModNPC thus we have access to the asset path directly
            var headTexturePath = _npc.ModNPC.HeadTexture;
            // The asset path starts with the name of the mod. This needs to be cut off.
            var prunedHeadTexturePath = headTexturePath.Substring(headTexturePath.IndexOf('/') + 1);
            return new UIImage(_npc.ModNPC.Mod.Assets.Request<Texture2D>(prunedHeadTexturePath));
        }

        // This is an vanilla NPC thus we need to fetch the head texture ID first
        var headId = NPCHeadID.HousingQuery;
        if (_npc != null && TownNPCProfiles.Instance.GetProfile(_npc, out var profile))
        {
            headId = profile.GetHeadTextureIndex(_npc);
        }

        return new UIImage(Main.Assets.Request<Texture2D>($"Images/NPC_Head_{headId}"));
    }
}