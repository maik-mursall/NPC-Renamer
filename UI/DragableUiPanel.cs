using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace NPCRenamer.UI;

// This DragableUIPanel class inherits from UIPanel. 
// Inheriting is a great tool for UI design. By inheriting, we get the background drawing for free from UIPanel
// We've added some code to allow the panel to be dragged around. 
// We've also added some code to ensure that the panel will bounce back into bounds if it is dragged outside or the screen resizes.
// UIPanel does not prevent the player from using items when the mouse is clicked, so we've added that as well.
internal class DragableUiPanel : UIPanel
{
    // Stores the offset from the top left of the UIPanel while dragging.
    private Vector2 _dragOffset;
    private bool _dragging;
    
    private Vector2 _resizeOffset;
    private bool _resizing;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (evt.Target != this) return;
        
        DragStart(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
        
        if (evt.Target != this) return;

        DragEnd(evt);
    }

    private void DragStart(UIMouseEvent evt)
    {
        _dragOffset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
        _dragging = true;
    }

    private void DragEnd(UIMouseEvent evt)
    {
        Vector2 end = evt.MousePosition;
        _dragging = false;

        Left.Set(end.X - _dragOffset.X, 0f);
        Top.Set(end.Y - _dragOffset.Y, 0f);

        Recalculate();

        ModContent.GetInstance<NpcRenamerClientConfig>().NpcRenamerPosition = new Vector2(Left.Pixels, Top.Pixels);
        NpcRenamerClientConfig.SaveConfig();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime); // don't remove.

        // Checking ContainsPoint and then setting mouseInterface to true is very common. This causes clicks on this UIElement to not cause the player to use current items. 
        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        if (_dragging)
        {
            Left.Set(Main.mouseX - _dragOffset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same.
            Top.Set(Main.mouseY - _dragOffset.Y, 0f);
            Recalculate();
        }
        
        if (_resizing)
        {
            Width.Set(Math.Max(Main.mouseX - Left.Pixels + 18, MinWidth.Pixels), 0f);
            Height.Set(Math.Max(Main.mouseY - Top.Pixels + 18, MinHeight.Pixels), 0f);
            Recalculate();
        }

        // Here we check if the DragableUIPanel is outside the Parent UIElement rectangle. 
        // (In our example, the parent would be ExampleUI, a UIState. This means that we are checking that the DragableUIPanel is outside the whole screen)
        // By doing this and some simple math, we can snap the panel back on screen if the user resizes his window or otherwise changes resolution.
        var parentSpace = Parent.GetDimensions().ToRectangle();
        if (!GetDimensions().ToRectangle().Intersects(parentSpace))
        {
            Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
            Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
            // Recalculate forces the UI system to do the positioning math again.
            Recalculate();
        }
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        MinHeight.Set(200, 0f);
        MinWidth.Set(200, 0f);
        PaddingBottom = 36f;
        
        var resizeButton = new UIImageButton(NpcRenamer.Instance.Assets.Request<Texture2D>("Images/ResizeButton", AssetRequestMode.ImmediateLoad));
        resizeButton.Width.Set(12, 0f);
        resizeButton.Height.Set(12, 0f);
        resizeButton.VAlign = 1f;
        resizeButton.HAlign = 1f;
        resizeButton.MarginBottom = -PaddingBottom + 12;
        resizeButton.OnLeftMouseDown += ResizeButtonOnOnMouseDown;
        resizeButton.OnLeftMouseUp += ResizeButtonOnOnMouseUp;
        Append(resizeButton);
    }

    private void ResizeButtonOnOnMouseUp(UIMouseEvent evt, UIElement listeningelement)
    {
        _resizing = false;
        Width.Set(Math.Max(evt.MousePosition.X - Left.Pixels + 18, MinWidth.Pixels), 0f);
        Height.Set(Math.Max(evt.MousePosition.Y - Top.Pixels + 18, MinHeight.Pixels), 0f);
        Recalculate();
        
        ModContent.GetInstance<NpcRenamerClientConfig>().NpcRenamerSize = new Vector2(Width.Pixels, Height.Pixels);
        NpcRenamerClientConfig.SaveConfig();
    }

    private void ResizeButtonOnOnMouseDown(UIMouseEvent evt, UIElement listeningelement)
    {
        _resizing = true;
    }
}