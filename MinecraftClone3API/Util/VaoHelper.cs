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
        private static readonly uint[] FlippedFaceIndices = { 0, 2, 3, 0, 3, 1 };

        public static void AddBlockToVao(World world, Vector3i blockPos, int x, int y, int z, Block block, VertexArrayObject vao, VertexArrayObject transparentVao)
        {
            if (!block.IsVisible(world, blockPos)) return;

            foreach (var face in BlockFaceHelper.Faces)
            {
                var otherBlockPos = blockPos + face.GetNormali();
                var otherBlock = world.GetBlock(otherBlockPos);

                var fullBlock = block.IsFullBlock(world, blockPos);
                var transparent = block.IsTransparent(world, blockPos);

                var otherFullBlock = otherBlock.IsFullBlock(world, otherBlockPos);
                var otherTransparent = otherBlock.IsTransparent(world, otherBlockPos);

                if (otherBlock.IsVisible(world, otherBlockPos) &&
                    (otherTransparent ? transparent : fullBlock && otherFullBlock)) continue;

                AddFaceToVao(world, blockPos, x, y, z, block, face, transparent ? transparentVao : vao);
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


            var vPositions = new Vector3[4];
            var vTexCoords = new Vector4[4];
            var vOverlayTexCoords = new Vector4[4];
            var vBrightness = new Vector3[4];

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

                vPositions[j] = position;
                vTexCoords[j] = texCoord;
                vOverlayTexCoords[j] = overlayTexCoord;
                vBrightness[j] = brightness;
            }

            //Flip faces to fix ambient occlusion anisotrophy
            //https://0fps.net/2013/07/03/ambient-occlusion-for-minecraft-like-worlds/

            for (var j = 0; j < 4; j++)
                vao.Add(vPositions[j], vTexCoords[j], vOverlayTexCoords[j], new Vector4(normal),
                    new Vector4(color.Xyz * vBrightness[j], color.W),
                    new Vector4(overlayColor.Xyz * vBrightness[j], overlayColor.W));

            var newIndices = new uint[FaceIndices.Length];

            if ((vBrightness[0] + vBrightness[3]).LengthSquared > (vBrightness[1] + vBrightness[2]).LengthSquared)
            {
                for (var j = 0; j < newIndices.Length; j++)
                    newIndices[j] = (uint)(FlippedFaceIndices[j] + indicesOffset);
            }
            else
            {
                for (var j = 0; j < newIndices.Length; j++)
                    newIndices[j] = (uint)(FaceIndices[j] + indicesOffset);
            }

            //Calculate face middle
            var faceMiddle = Vector3.Zero;
            foreach (var pos in vPositions)
                faceMiddle += pos;
            faceMiddle = faceMiddle / vPositions.Length + blockPos.ToVector3() - new Vector3(x, y, z);

            vao.AddFace(newIndices, faceMiddle);
        }

        private static Vector3 CalculateBrightness(World world, Block block, Vector3i blockPos, BlockFace face, Vector3 vertexPosition)
        {
            //if its not a full opaque block return brightness of itself
            if (!block.IsFullBlock(world, blockPos))
                return LightLevelToBrightness(world.GetBlockLightLevel(blockPos).Vector3);

            //TODO: smooth lighting setting
            //return LightLevelToBrightness(world.GetBlockLightLevel(blockPos + face.GetNormali()));

            var normal = face.GetNormali();
            var pos = blockPos + normal;
            var offset = (vertexPosition * 2).ToVector3i();
            
            var lightValue = Vector3.Zero;

            if (normal.X != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, 0)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, 0, offset.Z)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, offset.Z)).Vector3);
            }
            else if (normal.Y != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, 0)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, 0, offset.Z)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, offset.Z)).Vector3);
            }
            else if (normal.Z != 0)
            {
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, 0, 0)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(0, offset.Y, 0)).Vector3);
                lightValue += LightLevelToBrightness(world.GetBlockLightLevel(pos + new Vector3i(offset.X, offset.Y, 0)).Vector3);
            }

            return lightValue / 4;
        }

        private const float Base = 0.8f;
        //private const float Base = 1f;
        private const float CustomBase = 0.897499991f;

        private static Vector3 LightLevelToBrightness(Vector3 lightLevel)
        {
            for (var i = 0; i < 3; i++)
                lightLevel[i] = CustomLightLevelToBrightness(lightLevel[i]);

            return lightLevel;
        }


        private static float VanillaLightLevelToBrightness(float lightLevel)
            => (float) Math.Pow(Base, Math.Max(15 - lightLevel, 0));

        private static float CustomLightLevelToBrightness(float lightLevel)
        {
            //if(lightLevel < 15) return VanillaLightLevelToBrightness(lightLevel);

            return (float)Math.Pow(CustomBase, Math.Max(31 - lightLevel, 0));
            //return (float)Math.Pow(0.8, 1 - (lightLevel - 14) / 17);
        }
    }
}