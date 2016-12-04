using OpenTK;

namespace MinecraftClone3.Utils
{
    internal class BlockRaytraceResult
    {
        public readonly BlockFace Face;
        public readonly Vector3i BlockPos;
        public readonly float Distance;
        public readonly Vector3 Point;

        public BlockRaytraceResult(BlockFace face, Vector3i blockPos, float distance, Vector3 point)
        {
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