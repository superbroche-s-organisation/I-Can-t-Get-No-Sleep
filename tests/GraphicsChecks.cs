using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ICGNS.Tests;

internal static class GraphicsChecks
{
    internal static void Run(string gamePath, Action<bool, string> check)
    {
        // No game loop, visible window or save is loaded. Use the installed MonoGame
        // renderer in a hidden SDL window to exercise both native lightmap passes.
        IntPtr sdl = NativeLibrary.Load(Path.Combine(gamePath, "SDL2.dll"));
        try
        {
            using Game game = new();
            using GraphicsDeviceManager manager = new(game)
            {
                PreferredBackBufferWidth = 32,
                PreferredBackBufferHeight = 32,
                PreferredDepthStencilFormat = DepthFormat.None,
                SynchronizeWithVerticalRetrace = false
            };
            manager.ApplyChanges();
            GraphicsDevice device = manager.GraphicsDevice;
            using SpriteBatch batch = new(device);
            using Texture2D pixel = new(device, 1, 1);
            pixel.SetData(new[] { Color.White });
            using RenderTarget2D lightmap = new(device, 32, 32, false, SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            using RenderTarget2D output = new(device, 32, 32, false, SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            using BlendState nativeBlend = ReadNativeLightingBlend();
            check(nativeBlend.ColorBlendFunction == BlendFunction.ReverseSubtract,
                "The installed game's lighting must use the actual subtractive blend under test.");
            check(PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(Game1), nameof(Game1.DrawLighting)))
                .Any(instruction => instruction.LoadsField(AccessTools.Field(typeof(BlendState), nameof(BlendState.NonPremultiplied)))),
                "The installed game's lightmap must still use non-premultiplied source colors.");

            Color scenery = new(180, 170, 140);
            Color Render(Color mask)
            {
                device.SetRenderTarget(lightmap);
                device.Clear(Color.Transparent);
                batch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
                batch.Draw(pixel, lightmap.Bounds, mask);
                batch.End();
                device.SetRenderTarget(output);
                device.Clear(scenery);
                batch.Begin(blendState: nativeBlend, samplerState: SamplerState.PointClamp);
                batch.Draw(lightmap, output.Bounds, Color.White);
                batch.End();
                device.SetRenderTarget(null);
                Color[] pixels = new Color[32 * 32];
                output.GetData(pixels);
                return pixels[16 * 32 + 16];
            }

            Color oldBug = Render(Color.White);
            check(oldBug.R == 0 && oldBug.G == 0 && oldBug.B == 0,
                "The regression fixture must reproduce the reported black scene with the old white endpoint.");
            foreach (bool raining in new[] { false, true })
            {
                Color ambient = raining ? new Color(255, 200, 80) : Color.White;
                Color night = (raining ? ambient : new Color(255, 255, 0)) * 0.93f;
                Color morning = Sunrise.MorningLight(raining, ambient);
                Color previous = Render(night);
                for (int minute = 1; minute <= 120; minute++)
                {
                    int total = 28 * 60 + minute;
                    Color current = Render(Sunrise.Blend(night, morning, total / 60 * 100 + total % 60, 0f));
                    check(current.R >= previous.R && current.G >= previous.G && current.B >= previous.B,
                        $"Actual rendered scenery must brighten throughout sunrise (rain={raining}, minute={minute}).");
                    previous = current;
                }
                Color nativeMorning = Render(raining ? ambient * 0.3f : Color.Transparent);
                check(previous == nativeMorning, "Rendered sunrise must reach native morning brightness.");
                if (!raining)
                    // The game's final world-buffer pass is opaque; lightmap alpha
                    // is not used when composing the visible scene with the HUD.
                    check(previous.R == scenery.R && previous.G == scenery.G && previous.B == scenery.B,
                        $"Clear-weather sunrise must preserve full daylight RGB (expected {scenery}, rendered {previous}).");

                // Exercise the actual mod scene-copy pass with the native output,
                // including its alpha. A clear dawn has visible RGB but zero alpha.
                Color[] savedPixels = new Color[32 * 32];
                output.GetData(savedPixels);
                using Texture2D snapshot = new(device, 32, 32);
                snapshot.SetData(savedPixels);
                device.SetRenderTarget(output);
                device.Clear(Color.Black);
                DawnPresentation.DrawScene(batch, snapshot, output.Bounds);
                device.SetRenderTarget(null);
                output.GetData(savedPixels);
                Color savedScene = savedPixels[16 * 32 + 16];
                check(savedScene.R == previous.R && savedScene.G == previous.G && savedScene.B == previous.B,
                    "The dawn save display must retain the rendered world's visible RGB regardless of its alpha.");
            }
            FatigueHudGraphicsChecks.Run(game, gamePath, batch, pixel, check);
        }
        finally
        {
            NativeLibrary.Free(sdl);
        }
    }

    private static BlendState ReadNativeLightingBlend()
    {
        // Read the three blend settings from the installed Game1 constructor rather
        // than encoding another copy of the game's lighting formula in the test.
        List<CodeInstruction> code = PatchProcessor.GetOriginalInstructions(AccessTools.Constructor(typeof(Game1))).ToList();
        int end = code.FindIndex(instruction => instruction.StoresField(AccessTools.Field(typeof(Game1), "lightingBlend")));
        if (end < 0)
            throw new InvalidOperationException("Native lighting blend initializer not found.");
        int start = code.FindLastIndex(end, instruction => instruction.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(BlendState));
        if (start < 0)
            throw new InvalidOperationException("Native lighting blend initializer not found.");
        BlendState result = new();
        for (int i = start + 1; i < end; i++)
        {
            if (code[i].operand is not MethodInfo setter || setter.DeclaringType != typeof(BlendState) || !setter.Name.StartsWith("set_"))
                continue;
            PropertyInfo property = typeof(BlendState).GetProperty(setter.Name[4..])!;
            CodeInstruction constant = code[i - 1];
            int value = constant.opcode.Value switch
            {
                0x15 => -1,
                >= 0x16 and <= 0x1e => constant.opcode.Value - 0x16,
                _ => Convert.ToInt32(constant.operand)
            };
            property.SetValue(result, Enum.ToObject(property.PropertyType, value));
        }
        return result;
    }
}
