using MinecraftClone3API.Blocks;
using OpenTK;

namespace MinecraftClone3API.Util
{
    public class BlockRaytraceResult
    {
        public readonly Block Block;
        public readonly BlockFace Face;
        public readonly Vector3i BlockPos;
        public readonly float Distance;
        public readonly Vector3 Point;

        public BlockRaytraceResult(Block block, BlockFace face, Vector3i blockPos, float distance, Vector3 point)
        {
            Block = block;
            Face = face;
            BlockPos = blockPos;
            Distance = distance;
            Point = point;
        }

        public override string ToString()
        {
            return
                $"BlockRaytraceResult (Face:{Face}; BlockPos:{BlockPos}; Distance:{Distance}; Point:{Point})";
        }
    }
}