using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ICGNS;

/// <summary>Keep a stable scene on screen while the native new-day worker changes world data.</summary>
internal sealed class DawnPresentation : IDisposable
{
    private Texture2D? scene;
    private Texture2D? hud;

    internal bool IsCaptured => scene is not null && !scene.IsDisposed;

    internal void Capture(Game1 game)
    {
        Dispose();
        GraphicsDevice device = game.GraphicsDevice;
        RenderTarget2D? screen = game.screen;
        if (screen is null || screen.IsDisposed)
            return;

        RenderTargetBinding[] bindings = device.GetRenderTargets();
        try
        {
            // Read a completed frame, never the world while its overnight worker is changing it.
            device.SetRenderTarget(null);
            scene = Copy(screen, device);
            if (game.uiScreen is { IsDisposed: false } ui)
                hud = Copy(ui, device);
        }
        catch
        {
            Dispose();
            throw;
        }
        finally
        {
            device.SetRenderTargets(bindings);
        }
    }

    internal bool Draw(Game1 game, GameTime time, RenderTarget2D? target)
    {
        if (!IsCaptured)
            return false;
        Game1.SetRenderTarget(target);
        DrawScene(Game1.spriteBatch, scene!, target?.Bounds ?? game.GraphicsDevice.Viewport.Bounds);
        if (hud is not null && !hud.IsDisposed && game.uiScreen is { IsDisposed: false } ui)
        {
            Game1.SetRenderTarget(ui);
            DrawTexture(hud, ui.Bounds);
            Game1.SetRenderTarget(target);
        }

        // SaveGameMenu waits until it has been drawn before starting the actual save.
        // Our replacement display must satisfy that gate without replacing its update/sync logic.
        EnableNativeSave(Game1.activeClickableMenu);
        if (Game1.activeClickableMenu is not null and not SaveGameMenu and not ReadyCheckDialog)
            game.DrawMenu(time, target); // Preserve level-up/profession choices and other required menus.

        Game1.PushUIMode();
        try
        {
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawSaveIcon(Game1.spriteBatch, time);
            Game1.spriteBatch.End();
        }
        finally
        {
            Game1.PopUIMode();
        }
        return true;
    }

    internal static void EnableNativeSave(IClickableMenu? menu)
    {
        if (menu is SaveGameMenu save)
            save.hasDrawn = true;
    }

    private static Texture2D Copy(RenderTarget2D source, GraphicsDevice device)
    {
        Color[] pixels = new Color[source.Width * source.Height];
        source.GetData(pixels);
        Texture2D copy = new(device, source.Width, source.Height, false, SurfaceFormat.Color);
        try
        {
            copy.SetData(pixels);
            return copy;
        }
        catch
        {
            copy.Dispose();
            throw;
        }
    }

    private static void DrawTexture(Texture2D texture, Rectangle bounds)
    {
        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        Game1.spriteBatch.Draw(texture, bounds, Color.White);
        Game1.spriteBatch.End();
    }

    internal static void DrawScene(SpriteBatch batch, Texture2D texture, Rectangle bounds)
    {
        // Native renderScreenBuffer draws the world opaquely. Its subtractive
        // lighting can leave zero alpha even though its RGB pixels are visible.
        // Alpha blending the captured world would turn a bright dawn into black.
        batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
        batch.Draw(texture, bounds, Color.White);
        batch.End();
    }

    private static void DrawSaveIcon(SpriteBatch batch, GameTime time)
    {
        const int size = 48;
        Vector2 position = Utility.makeSafe(new Vector2(24f, Game1.uiViewport.Height - size - 24f), new Vector2(size));
        int x = (int)position.X;
        int y = (int)position.Y;
        float pulse = 0.85f + 0.15f * (float)Math.Sin(time.TotalGameTime.TotalSeconds * 4d);
        void Block(int dx, int dy, int width, int height, Color color)
            => batch.Draw(Game1.staminaRect, new Rectangle(x + dx * 4, y + dy * 4, width * 4, height * 4), color * pulse);
        // A small pixel-art floppy disk; no fullscreen panel or saving text.
        Block(0, 0, 12, 12, new Color(65, 39, 28));
        Block(1, 1, 10, 10, new Color(239, 180, 79));
        Block(3, 1, 6, 4, new Color(239, 235, 202));
        Block(7, 1, 1, 3, new Color(77, 105, 116));
        Block(3, 7, 6, 4, new Color(255, 243, 207));
        Block(4, 8, 4, 1, new Color(142, 102, 65));
        Block(4, 10, 4, 1, new Color(142, 102, 65));
    }

    public void Dispose()
    {
        scene?.Dispose();
        hud?.Dispose();
        scene = null;
        hud = null;
    }
}
