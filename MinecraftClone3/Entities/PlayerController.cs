using System;
using MinecraftClone3.Blocks;
using MinecraftClone3.Graphics;
using MinecraftClone3.Utils;
using OpenTK;
using OpenTK.Input;

namespace MinecraftClone3.Entities
{
    internal static class PlayerController
    {
        public static Camera Camera = new Camera();

        private static MouseState? _oldMouseState;
        private static BlockRaytraceResult _blockRaytrace;

        public static void Update(GameWindow window, World world)
        {
            _blockRaytrace = world.BlockRaytrace(Camera.Position, Camera.Forward, 8);

            var ks = Keyboard.GetState();
            var a = Vector3.Zero;
            if (ks.IsKeyDown(Key.A))
                a.X -= 1;
            if (ks.IsKeyDown(Key.D))
                a.X += 1;
            if (ks.IsKeyDown(Key.ShiftLeft))
                a.Y -= 1;
            if (ks.IsKeyDown(Key.Space))
                a.Y += 1;
            if (ks.IsKeyDown(Key.S))
                a.Z -= 1;
            if (ks.IsKeyDown(Key.W))
                a.Z += 1;
            if (Math.Abs(a.LengthSquared) > 0.0001f)
                Camera.Move(a.Normalized() * 0.08f);


            if (window.Focused)
            {
                var ms = Mouse.GetState();
                if (_oldMouseState != null)
                {
                    var pitch = _oldMouseState.Value.Y - ms.Y;
                    var yaw = _oldMouseState.Value.X - ms.X;
                    Camera.Rotate(pitch * 0.008f, yaw * 0.008f);

                    if (_oldMouseState.Value.LeftButton == ButtonState.Released && ms.LeftButton == ButtonState.Pressed)
                        BreakBlock(world);
                    if (_oldMouseState.Value.RightButton == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                        PlaceBlock(world);
                }
                _oldMouseState = ms;
                Mouse.SetPosition(window.X + window.Width / 2, window.Y + window.Height / 2);
            }

            Camera.Update();
        }

        private static void BreakBlock(World world)
        {
            if (_blockRaytrace == null) return;
            world.SetBlock(_blockRaytrace.BlockPos, 0);
        }

        private static void PlaceBlock(World world)
        {
            if (_blockRaytrace == null) return;
            world.SetBlock(_blockRaytrace.BlockPos + _blockRaytrace.Face.GetNormali(), 1);
        }

        public static void ResetMouse()
        {
            _oldMouseState = Mouse.GetState();
        }
    }
}