using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NPCRenamer.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace NPCRenamer;

// ReSharper disable once UnusedType.Global
public class NpcRenamerUiSystem : ModSystem
{
    private UserInterface _userInterface;
    private NpcRenamerUiState _npcRenamerUiState;

    private GameTime _lastUpdateUiGameTime;

    public override void Load()
    {
        if (Main.dedServ) return;

        _userInterface = new UserInterface();
        _npcRenamerUiState = new NpcRenamerUiState();
    }

    public override void OnWorldLoad()
    {
        base.OnWorldLoad();

        if (Main.dedServ) return;

        _npcRenamerUiState.OnNpcNameChange += ((NpcRenamer)Mod).ChangeNpcName;
        _npcRenamerUiState.OnCloseRequest += HideUi;
        _npcRenamerUiState.Activate();
    }

    public override void OnWorldUnload()
    {
        base.OnWorldUnload();

        if (Main.dedServ) return;

        _npcRenamerUiState.OnNpcNameChange -= ((NpcRenamer)Mod).ChangeNpcName;
        _npcRenamerUiState.OnCloseRequest -= HideUi;
        _npcRenamerUiState.Deactivate();
        HideUi();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;

        if (_userInterface?.CurrentState != null)
        {
            if (NpcRenamer.Instance.ToggleNpcRenamerHotKey.JustPressed)
            {
                HideUi();
            }

            _userInterface.Update(gameTime);
        }
        else if (NpcRenamer.Instance.ToggleNpcRenamerHotKey.JustPressed)
        {
            ShowUi();
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "NpcRenamer: Interface",
                delegate
                {
                    if (_lastUpdateUiGameTime != null && _userInterface?.CurrentState != null)
                    {
                        _userInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                    }

                    return true;
                },
                InterfaceScaleType.UI));
        }
    }

    private void ShowUi()
    {
        _userInterface?.SetState(_npcRenamerUiState);
    }

    private void HideUi()
    {
        _userInterface?.SetState(null);
    }

    private class NpcRenamerUiState : UIState
    {
        private DragableUiPanel _uiPanel;
        private UIList _uiList;
        private UIScrollbar _uiScrollBar;
        private bool _scrollBarVisible = true;

        private UIText _emptyText;
        private bool _emptyTextVisible = true;

        private readonly Dictionary<NPC, UiNpcEditor> _uiNpcEditors = new();

        public event NpcNameChange OnNpcNameChange;
        public event Action OnCloseRequest;

        public override void Update(GameTime gameTime)
        {
            foreach (var npc in Main.npc.Where(npc => npc.active && npc.townNPC && npc.type != NPCID.OldMan))
            {
                if (!_uiNpcEditors.ContainsKey(npc))
                {
                    var uiNpcEditor = new UiNpcEditor(npc);
                    uiNpcEditor.Width.Set(0, 1f);
                    uiNpcEditor.Height.Set(34, 0f);
                    uiNpcEditor.Top.Set(40 * _uiNpcEditors.Count, 0f);
                    uiNpcEditor.OnNpcNameChange += OnNpcNameChange;
                    _uiList.Add(uiNpcEditor);

                    uiNpcEditor.Activate();

                    _uiNpcEditors.Add(npc, uiNpcEditor);
                }
            }

            foreach (var uiNpcEditor in _uiNpcEditors)
            {
                if (!uiNpcEditor.Key.active)
                {
                    uiNpcEditor.Value.OnNpcNameChange -= OnNpcNameChange;
                    uiNpcEditor.Value.Deactivate();
                    _uiList.Remove(uiNpcEditor.Value);

                    _uiNpcEditors.Remove(uiNpcEditor.Key);
                }
            }
            
            switch (_uiNpcEditors.Count == 0)
            {
                case false when _emptyTextVisible:
                    _emptyText.Deactivate();
                    _emptyText.Remove();
                    _emptyTextVisible = false;
                    break;
                case true when !_emptyTextVisible:
                    _emptyText.Activate();
                    _uiPanel.Append(_emptyText);
                    _emptyTextVisible = true;
                    break;
            }
            
            switch (_uiScrollBar.CanScroll)
            {
                case false when _scrollBarVisible:
                    _uiScrollBar.Deactivate();
                    _uiScrollBar.Remove();
                    _uiList.PaddingRight = 0f;
                    _scrollBarVisible = false;
                    break;
                case true when !_scrollBarVisible:
                    _uiScrollBar.Activate();
                    _uiPanel.Append(_uiScrollBar);
                    _uiList.PaddingRight = 26f;
                    _scrollBarVisible = true;
                    break;
            }

            base.Update(gameTime);
        }

        public override void OnInitialize()
        {
            var uiPanelPosition = ModContent.GetInstance<NpcRenamerClientConfig>().NpcRenamerPosition;
            var uiPanelSize = ModContent.GetInstance<NpcRenamerClientConfig>().NpcRenamerSize;

            _uiPanel = new DragableUiPanel();
            _uiPanel.Width.Set(uiPanelSize.X, 0);
            _uiPanel.Height.Set(uiPanelSize.Y, 0);
            _uiPanel.Left.Set(uiPanelPosition.X, 0f);
            _uiPanel.Top.Set(uiPanelPosition.Y, 0f);
            _uiPanel.PaddingTop = 36f;
            Append(_uiPanel);

            _uiScrollBar = new UIScrollbar();
            _uiScrollBar.Height.Set(0, 1f);
            _uiScrollBar.HAlign = 1f;

            _uiList = new UIList();
            _uiList.Width.Set(0, 1f);
            _uiList.Height.Set(0, 1f);
            _uiList.PaddingRight = 26f;
            _uiList.SetScrollbar(_uiScrollBar);
            _uiList.Append(_uiScrollBar);
            _uiPanel.Append(_uiList);

            var closeButton = new UIText("X", 1f);
            closeButton.SetPadding(12);
            closeButton.Width.Set(12, 0);
            closeButton.Height.Set(12, 0);
            closeButton.VAlign = 0f;
            closeButton.HAlign = 1f;
            closeButton.MarginTop = -_uiPanel.PaddingTop;
            closeButton.MarginRight = -_uiPanel.PaddingRight;
            closeButton.OnLeftMouseDown += CloseButtonOnOnMouseDown;
            _uiPanel.Append(closeButton);

            var titleText = new UIText("NPC Renamer")
            {
                VAlign = 0f,
                HAlign = 0.5f,
                MarginTop = -_uiPanel.PaddingTop + 12,
                IgnoresMouseInteraction = true
            };
            _uiPanel.Append(titleText);

            _emptyText = new UIText("No NPCs alive")
            {
                VAlign = 0.5f,
                HAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
            _uiPanel.Append(_emptyText);
        }

        private void CloseButtonOnOnMouseDown(UIMouseEvent evt, UIElement listeningelement)
        {
            OnCloseRequest?.Invoke();
        }

        public override void OnDeactivate()
        {
            foreach (var uinpcEditor in _uiNpcEditors)
            {
                uinpcEditor.Value.OnNpcNameChange -= OnNpcNameChange;
                uinpcEditor.Value.Deactivate();
            }

            _uiNpcEditors.Clear();
            _uiList.Clear();
        }
    }
}