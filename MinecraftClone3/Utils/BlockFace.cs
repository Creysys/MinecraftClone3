using System;
using OpenTK;

namespace MinecraftClone3.Utils
{
    internal enum BlockFace
    {
        Left,
        Right,
        Bottom,
        Top,
        Back,
        Front
    }

    internal static class BlockFaceHelper
    {
        public static readonly BlockFace[] Faces =
        {
            BlockFace.Left, BlockFace.Right, BlockFace.Bottom,
            BlockFace.Top, BlockFace.Back, BlockFace.Front
        };

        public static Vector3 GetNormal(this BlockFace face)
        {
            var v = face.GetNormali();
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3i GetNormali(this BlockFace face)
        {
            switch (face)
            {
                case BlockFace.Left:
                    return new Vector3i(-1, 0, 0);
                case BlockFace.Right:
                    return new Vector3i(+1, 0, 0);
                case BlockFace.Bottom:
                    return new Vector3i(0, -1, 0);
                case BlockFace.Top:
                    return new Vector3i(0, +1, 0);
                case BlockFace.Back:
                    return new Vector3i(0, 0, -1);
                case BlockFace.Front:
                    return new Vector3i(0, 0, +1);
                default:
                    throw new Exception("Invalid BlockFace!");
            }
        }
    }
}