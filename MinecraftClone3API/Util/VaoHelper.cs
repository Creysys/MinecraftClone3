using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using OpenTK;

namespace MinecraftClone3API.Util
{
    internal static class VaoHelper
    {
        private static readonly Vector3[] FacePositions = {
            //left
            new Vector3(-0.5f, +0.5f, -0.5f), new Vector3(-0.5f, +0.5f, +0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, +0.5f),
            //right
            new Vector3(+0.5f, +0.5f, +0.5f), new Vector3(+0.5f, +0.5f, -0.5f),
            new Vector3(+0.5f, -0.5f, +0.5f), new Vector3(+0.5f, -0.5f, -0.5f),
            //bottom
            new Vector3(-0.5f, -0.5f, +0.5f), new Vector3(+0.5f, -0.5f, +0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(+0.5f, -0.5f, -0.5f),
            //top
            new Vector3(-0.5f, +0.5f, -0.5f), new Vector3(+0.5f, +0.5f, -0.5f),
            new Vector3(-0.5f, +0.5f, +0.5f), new Vector3(+0.5f, +0.5f, +0.5f),
            //back
            new Vector3(+0.5f, +0.5f, -0.5f), new Vector3(-0.5f, +0.5f, -0.5f),
            new Vector3(+0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            //front
            new Vector3(-0.5f, +0.5f, +0.5f), new Vector3(+0.5f, +0.5f, +0.5f),
            new Vector3(-0.5f, -0.5f, +0.5f), new Vector3(+0.5f, -0.5f, +0.5f)
        };

        private static readonly Vector2[] FaceTexCoords =
        {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(1, 1),
        };

        private static readonly uint[] FaceIndices = {2, 1, 0, 2, 3, 1};


        public static void AddBlockToVao(World world, Vector3i blockPos, int x, int y, int z, uint id, VertexArrayObject vao)
        {
            if (id == GameRegistry.BlockAir.Id) return;

            var block = GameRegistry.GetBlock(id);
            if (!block.IsVisible(world, blockPos)) return;

            foreach (var face in BlockFaceHelper.Faces)
            {
                var faceNormal = face.GetNormali();
                var otherBlock = world.GetBlock(blockPos + faceNormal);
                if (!otherBlock.IsVisible(world, blockPos) || !otherBlock.IsOpaque(world, blockPos))
                    AddFaceToVao(x, y, z, id, face, vao);
            }
        }

        public static void AddFaceToVao(int x, int y, int z, uint id, BlockFace face, VertexArrayObject vao)
        {
            var faceId = (int) face;
            var indicesOffset = vao.VertexCount;

            for (var j = 0; j < 4; j++)
                vao.Add(FacePositions[faceId * 4 + j] + new Vector3(x, y, z), new Vector3(FaceTexCoords[j]) {Z = id - 1});

            var newIndices = new uint[FaceIndices.Length];
            for (var j = 0; j < newIndices.Length; j++)
                newIndices[j] = (uint)(FaceIndices[j] + indicesOffset);

            vao.AddIndices(newIndices);
        }
    }
}