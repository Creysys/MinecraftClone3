using System;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Input;

namespace MinecraftClone3API.Entities
{
    public static class PlayerController
    {
        public static EntityPlayer PlayerEntity;
        public static Camera Camera = new Camera();

        private static MouseState? _oldMouseState;
        private static BlockRaytraceResult _blockRaytrace;
        private static string _currentBlock = "Vanilla:Torch";

        public static void SetEntity(EntityPlayer playerEntity)
        {
            PlayerEntity = playerEntity;
            Camera.ParentEntity = playerEntity;
        }

        public static void Update(GameWindow window, World world)
        {
            _blockRaytrace = world.BlockRaytrace(PlayerEntity.Position, PlayerEntity.Forward, 8);
            
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
                PlayerEntity.Move(a.Normalized() * 0.08f);

            if (ks.IsKeyDown(Key.Number1)) _currentBlock = "Vanilla:Torch";
            if (ks.IsKeyDown(Key.Number2)) _currentBlock = "Vanilla:Dirt";
            if (ks.IsKeyDown(Key.Number3)) _currentBlock = "Vanilla:Glass";

            var ms = Mouse.GetState();
            if (_oldMouseState != null)
            {
                var pitch = _oldMouseState.Value.Y - ms.Y;
                var yaw = _oldMouseState.Value.X - ms.X;
                PlayerEntity.Rotate(pitch * 0.008f, yaw * 0.008f);

                if (_oldMouseState.Value.LeftButton == ButtonState.Released && ms.LeftButton == ButtonState.Pressed)
                    BreakBlock(world);
                if (_oldMouseState.Value.RightButton == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    PlaceBlock(world);
            }
            _oldMouseState = ms;
            Mouse.SetPosition(window.X + window.Width / 2, window.Y + window.Height / 2);

            Camera.Update();
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

            Logger.Debug(PlayerEntity.Position + ":" + _blockRaytrace.BlockPos);
        }

        public static void ResetMouse()
        {
            _oldMouseState = Mouse.GetState();
        }

        public static void Render(Camera camera, Matrix4 projection)
        {
            if (_blockRaytrace == null) return;

            BoundingBoxRenderer.Render(_blockRaytrace.BoundingBox, _blockRaytrace.BlockPos.ToVector3(), 1.01f, camera,
                projection);
        }
    }
}