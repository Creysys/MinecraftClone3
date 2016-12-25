using System;
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


        public static void AddBlockToVao(World world, Vector3i blockPos, int x, int y, int z, Block block, VertexArrayObject vao)
        {
            if (!block.IsVisible(world, blockPos)) return;

            foreach (var face in BlockFaceHelper.Faces)
            {
                var otherBlockPos = blockPos + face.GetNormali();
                var otherBlock = world.GetBlock(otherBlockPos);
                if (!block.IsOpaqueFullBlock(world, blockPos) || !otherBlock.IsVisible(world, otherBlockPos) || !otherBlock.IsOpaqueFullBlock(world, otherBlockPos))
                    AddFaceToVao(world, blockPos, x, y, z, block, face, vao);
            }
        }

        public static void AddFaceToVao(World world, Vector3i blockPos, int x, int y, int z, Block block, BlockFace face, VertexArrayObject vao)
        {
            var faceId = (int) face;
            var indicesOffset = vao.VertexCount;

            var transform = block.GetTransform(world, blockPos, face);
            var texture = block.GetTexture(world, blockPos, face);
            var overlayTexture = block.GetOverlayTexture(world, blockPos, face);
            var texCoords = block.GetTexCoords(world, blockPos, face) ?? FaceTexCoords;
            var overlayTexCoords = block.GetOverlayTexCoords(world, blockPos, face) ?? FaceTexCoords;
            var color = block.GetColor(world, blockPos, face).ToVector4();
            var overlayColor = block.GetOverlayColor(world, blockPos, face).ToVector4();
            var normal = face.GetNormal();

            if(texCoords.Length != 4) throw new Exception($"\"{block}\" invalid texture coords array length!");

            for (var j = 0; j < 4; j++)
            {
                var vertexPosition = FacePositions[faceId * 4 + j];
                var position = (new Vector4(vertexPosition, 1) * transform).Xyz + new Vector3(x, y, z);

                //tex coords are -1 if texture is null
                var texCoord = texture == null ? new Vector4(-1) : new Vector4(texCoords[j])
                {
                    //texCoord z = texId, w = textureArrayId
                    Z = texture.TextureId,
                    W = texture.ArrayId
                };

                var overlayTexCoord = overlayTexture == null ? new Vector4(-1) : new Vector4(overlayTexCoords[j])
                {
                    Z = overlayTexture.TextureId,
                    W = overlayTexture.ArrayId
                };

                //per vertex light value interpolation (smooth lighting + free ambient occlusion)
                var brightness = CalculateBrightness(world, block, blockPos, face, vertexPosition);

                //TODO: transform normals

                vao.Add(position, texCoord, overlayTexCoord, normal, new Vector4(color.Xyz * brightness, color.W),
                    new Vector4(overlayColor.Xyz * brightness, overlayColor.W));
            }

            var newIndices = new uint[FaceIndices.Length];
            for (var j = 0; j < newIndices.Length; j++)
                newIndices[j] = (uint)(FaceIndices[j] + indicesOffset);

            vao.AddIndices(newIndices);
        }

        private static float CalculateBrightness(World world, Block block, Vector3i blockPos, BlockFace face, Vector3 vertexPosition)
        {
            //if its not a full opaque block return brightness of itself
            if (!block.IsOpaqueFullBlock(world, blockPos))
                return LightLevelToBrightness(world.GetBlockLightLevel(blockPos));

            //TODO: smooth lighting setting
            //return 1;
            //return LightLevelToBrightness(world.GetBlockLightLevel(blockPos + face.GetNormali()));

            var normal = face.GetNormali();
            var pos = blockPos + normal;
            var offset = (vertexPosition * 2).ToVector3i();
            
            var lightValue = 0f;

            if (normal.X != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, 0)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, 0, offset.Z)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, offset.Z)));
            }
            else if (normal.Y != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, 0)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, 0, offset.Z)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, offset.Z)));
            }
            else if (normal.Z != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, 0)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, 0)));
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, offset.Y, 0)));
            }
            else throw new Exception("lol");

            return lightValue / 4;
        }

        private static float LightLevelToBrightness(byte lightLevel) => (float)Math.Pow(0.8, 15 - lightLevel);
    }
}