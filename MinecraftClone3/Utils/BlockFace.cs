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

        private static readonly Vector3i[] Normals =
        {
            new Vector3i(-1, 0, 0), new Vector3i(+1, 0, 0), new Vector3i(0, -1, 0),
            new Vector3i(0, +1, 0), new Vector3i(0, 0, -1), new Vector3i(0, 0, +1)
        };

        public static Vector3 GetNormal(this BlockFace face) => face.GetNormali().ToVector3();
        public static Vector3i GetNormali(this BlockFace face) => Normals[(int) face];
    }
}