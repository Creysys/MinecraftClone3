using System;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Input;

namespace MinecraftClone3.Entities
{
    internal static class PlayerController
    {
        private static EntityPlayer _playerEntity;
        private static MouseState? _oldMouseState;
        private static BlockRaytraceResult _blockRaytrace;
        private static string _currentBlock = "Vanilla:Torch";

        public static void SetEntity(EntityPlayer playerEntity) => _playerEntity = playerEntity;

        public static void Update(GameWindow window, World world)
        {
            _blockRaytrace = world.BlockRaytrace(_playerEntity.Position, _playerEntity.Forward, 8);
            
            if (!window.Focused) return;

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
                _playerEntity.Move(a.Normalized() * 0.08f);

            if (ks.IsKeyDown(Key.Number1)) _currentBlock = "Vanilla:Torch";
            if (ks.IsKeyDown(Key.Number2)) _currentBlock = "Vanilla:Dirt";

            var ms = Mouse.GetState();
            if (_oldMouseState != null)
            {
                var pitch = _oldMouseState.Value.Y - ms.Y;
                var yaw = _oldMouseState.Value.X - ms.X;
                _playerEntity.Rotate(pitch * 0.008f, yaw * 0.008f);

                if (_oldMouseState.Value.LeftButton == ButtonState.Released && ms.LeftButton == ButtonState.Pressed)
                    BreakBlock(world);
                if (_oldMouseState.Value.RightButton == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    for(var i = 0; i < 100; i++) PlaceBlock(world);
            }
            _oldMouseState = ms;
            Mouse.SetPosition(window.X + window.Width / 2, window.Y + window.Height / 2);
        }

        private static void BreakBlock(World world)
        {
            if (_blockRaytrace == null) return;
            world.SetBlock(_blockRaytrace.BlockPos, BlockRegistry.BlockAir);
        }

        private static void PlaceBlock(World world)
        {
            if (_blockRaytrace == null) return;
            world.SetBlock(_blockRaytrace.BlockPos + _blockRaytrace.Face.GetNormali(), GameRegistry.GetBlock(_currentBlock));
        }

        public static void ResetMouse()
        {
            _oldMouseState = Mouse.GetState();
        }
    }
}