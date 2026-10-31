using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ICGNS.Tests;

internal static class FatigueHudGraphicsChecks
{
    internal static void Run(Game game, string gamePath, SpriteBatch batch, Texture2D pixel, Action<bool, string> check)
    {
        using ContentManager content = new(game.Services, Path.Combine(gamePath, "Content"));
        Texture2D cursors = content.Load<Texture2D>("LooseSprites/Cursors");
        SpriteFont font = content.Load<SpriteFont>("Fonts/SmallFont");
        GraphicsDevice device = game.GraphicsDevice;
        using RenderTarget2D output = new(device, 720, 320);
        Rectangle[] gauges = new Rectangle[3];
        Color background = new(50, 65, 58);
        device.SetRenderTarget(output);
        device.Clear(background);
        batch.Begin(samplerState: SamplerState.PointClamp);
        for (int i = 0; i < 3; i++)
        {
            int level = new[] { 1, 5, 10 }[i];
            Rectangle safe = new(i * 240, 0, 240, 320);
            Rectangle bounds = FatigueBar.Bounds(safe, safe.Right - 56, true);
            gauges[i] = bounds;
            batch.DrawString(font, "Palier " + level, new Vector2(safe.Left + 42, 12), Color.White);
            DrawNativeGauge(new Rectangle(bounds.X + 56, bounds.Y, 48, bounds.Height), true, 0.8f);
            DrawNativeGauge(new Rectangle(bounds.X + 112, bounds.Y, 48, bounds.Height), false, 0.6f);
            FatigueBar.DrawGauge(batch, cursors, pixel, bounds, level / 10f);
        }
        batch.End();
        device.SetRenderTarget(null);
        Color[] pixels = new Color[output.Width * output.Height];
        output.GetData(pixels);
        for (int i = 0; i < gauges.Length; i++)
        {
            Rectangle bounds = gauges[i];
            Color filled = pixels[(bounds.Bottom - 12) * output.Width + bounds.X + 24];
            check(filled.B > filled.G && filled.R > filled.G, "The real fatigue gauge must render purple fill using native HUD assets.");
            Color gap = pixels[(bounds.Y + 100) * output.Width + bounds.Right + 3];
            check(gap == background, "The fatigue gauge must not paint over the gap before the health gauge.");
        }
        Color emptySpace = pixels[(gauges[0].Y + 100) * output.Width + gauges[0].X + 24];
        Color fullSpace = pixels[(gauges[2].Y + 100) * output.Width + gauges[2].X + 24];
        check(emptySpace != fullSpace && fullSpace.B > fullSpace.G,
            "The first and maximum fatigue levels must have visibly different fill heights.");
        // Optional local preview for visual inspection; no game artwork is committed.
        string? preview = Environment.GetEnvironmentVariable("ICGNS_HUD_PREVIEW_PATH");
        if (!string.IsNullOrWhiteSpace(preview))
        {
            using FileStream stream = File.Create(preview);
            output.SaveAsPng(stream, output.Width, output.Height);
        }

        void DrawNativeGauge(Rectangle bounds, bool health, float fill)
        {
            int sourceX = health ? 268 : 256;
            batch.Draw(cursors, new Vector2(bounds.X, bounds.Y), new Rectangle(sourceX, 408, 12, 16), Color.White,
                0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            batch.Draw(cursors, new Rectangle(bounds.X, bounds.Y + 64, 48, bounds.Height - 120),
                new Rectangle(sourceX, 424, 12, 16), Color.White);
            batch.Draw(cursors, new Vector2(bounds.X, bounds.Bottom - 64), new Rectangle(sourceX, 448, 12, 16), Color.White,
                0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            batch.Draw(pixel, FatigueBar.FillBounds(bounds, fill), health ? new Color(130, 194, 34) : new Color(161, 201, 18));
        }
    }
}
