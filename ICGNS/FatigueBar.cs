using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace ICGNS;

internal static class FatigueBar
{
    internal const int FullLevel = 10; // Both fatigue penalties have reached their caps.
    internal const int Width = 48;
    internal const int Spacing = 56;
    private static readonly Color Header = new(105, 58, 106);
    private static readonly Color Letter = new(244, 225, 255);

    internal static bool IsVisible(bool enabled, FatigueProgress? progress)
        => enabled && progress is { Level: > 0 };

    internal static float Fill(FatigueProgress progress)
        => Math.Clamp(progress.AwakeMinutes / (float)(FatigueProgress.MinutesPerLevel * FullLevel), 0f, 1f);

    internal static Rectangle Bounds(Rectangle safeArea, int energyX, bool healthVisible)
    {
        int height = Math.Min(224, safeArea.Height - 32);
        int x = energyX - Spacing * (healthVisible ? 2 : 1);
        // Hide rather than cover a native gauge on an exceptionally small viewport.
        if (height < 128 || x < safeArea.Left + 8 || x + Width > safeArea.Right - 8)
            return Rectangle.Empty;
        return new Rectangle(x, safeArea.Bottom - 16 - height, Width, height);
    }

    internal static Rectangle FillBounds(Rectangle bounds, float amount)
    {
        int height = (int)Math.Round((bounds.Height - 56) * Math.Clamp(amount, 0f, 1f));
        return new Rectangle(bounds.X + 12, bounds.Bottom - 8 - height, 24, height);
    }

    internal static void AfterDrawHud(Game1 __instance, bool __runOriginal)
    {
        if (!__runOriginal || !Context.IsWorldReady || !GameplaySettings.Enabled || Game1.eventUp
            || Game1.farmEvent is not null || Game1.newDay || Game1.showingEndOfNightStuff || __instance.takingMapScreenshot)
            return;
        ModEntry mod = ModEntry.Instance;
        FatigueProgress? saved = mod.Fatigue.Value.Progress;
        if (!IsVisible(mod.Config.ShowFatigueBar, saved))
            return;
        FatigueProgress progress = saved!.Value;
        Rectangle safeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
        int energyX = safeArea.Right - 56;
        if (Game1.isOutdoorMapSmallerThanViewport())
            energyX = Math.Min(energyX, -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 48);
        Rectangle bounds = Bounds(safeArea, energyX, Game1.showingHealth);
        if (bounds.IsEmpty)
            return;

        DrawGauge(Game1.spriteBatch, Game1.mouseCursors, Game1.staminaRect, bounds, Fill(progress));
        if (bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
        {
            string label = mod.Helper.Translation.Get("fatigue.name", new { level = progress.Level });
            string effects = mod.Helper.Translation.Get("fatigue.description",
                new { energyPercent = progress.EnergyReductionPercent, speed = progress.SpeedPenalty });
            IClickableMenu.drawHoverText(Game1.spriteBatch, label + "\n" + effects, Game1.smallFont);
        }
    }

    /// <summary>Draw in the existing HUD batch, using the same frame and scale as the native gauges.</summary>
    internal static void DrawGauge(SpriteBatch batch, Texture2D cursors, Texture2D pixel, Rectangle bounds, float amount)
    {
        batch.Draw(cursors, new Vector2(bounds.X, bounds.Y), new Rectangle(256, 408, 12, 16), Color.White,
            0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        batch.Draw(cursors, new Rectangle(bounds.X, bounds.Y + 64, Width, bounds.Height - 120),
            new Rectangle(256, 424, 12, 16), Color.White);
        batch.Draw(cursors, new Vector2(bounds.X, bounds.Bottom - 64), new Rectangle(256, 448, 12, 16), Color.White,
            0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

        // Replace the energy frame's E with a small pixel F (Fatigue in EN/FR).
        batch.Draw(pixel, new Rectangle(bounds.X + 12, bounds.Y + 8, 24, 28), Header);
        batch.Draw(pixel, new Rectangle(bounds.X + 16, bounds.Y + 12, 4, 20), Letter);
        batch.Draw(pixel, new Rectangle(bounds.X + 20, bounds.Y + 12, 8, 4), Letter);
        batch.Draw(pixel, new Rectangle(bounds.X + 20, bounds.Y + 20, 4, 4), Letter);

        Rectangle fill = FillBounds(bounds, amount);
        if (fill.Height <= 0)
            return;
        Color color = Color.Lerp(new Color(164, 128, 226), new Color(126, 66, 175), amount);
        batch.Draw(pixel, fill, color);
        batch.Draw(pixel, new Rectangle(fill.X, fill.Y, 4, fill.Height), color * 0.75f);
        batch.Draw(pixel, new Rectangle(fill.X, fill.Y, fill.Width, Math.Min(4, fill.Height)), color * 0.75f);
    }
}
