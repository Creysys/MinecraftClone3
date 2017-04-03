using MinecraftClone3API.Entities;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;

namespace MinecraftClone3API.Blocks
{
    public enum TransparencyType
    {
        None,
        Cutoff,
        Transparent
    }
    public enum ConnectionType
    {
        Undefined,
        Connected,
        Disconnected
    }

    public class Block : RegistryEntry
    {
        public static readonly AxisAlignedBoundingBox DefaultAlignedBoundingBox =
            new AxisAlignedBoundingBox(new Vector3(-0.5f), new Vector3(0.5f));
        
        public BlockModel Model = ClientResources.MissingModel;

        public Block(string name) : base(name)
        {
        }

        public ushort Id { get; internal set; }

        public virtual bool IsVisible(World world, Vector3i blockPos) => true;
        public virtual bool IsFullBlock(World world, Vector3i blockPos) => true;
        public virtual TransparencyType IsTransparent(World world, Vector3i blockPos) => TransparencyType.None;

        public virtual ConnectionType ConnectsToBlock(World world, Vector3i blockPos, Vector3i otherBlockPos,
            Block otherBlock) => ConnectionType.Undefined;

        public virtual bool CanPassThrough(World world, Vector3i blockPos) => false;
        public virtual bool CanTarget(World world, Vector3i vector3I) => true;

        public virtual AxisAlignedBoundingBox GetBoundingBox(World world, Vector3i blockPos)
            => DefaultAlignedBoundingBox;

        public virtual Color4 GetTintColor(World world, Vector3i blockPos, int tintId) => Color4.White;
        public virtual LightLevel GetLightLevel(World world, Vector3i blockPos) => LightLevel.Zero;

        public virtual void OnPlaced(World world, Vector3i blockPos, EntityPlayer player)
        {
        }

        public virtual int OnLightPassThrough(World world, Vector3i blockPos, int lightLevel, int color)
            => lightLevel - 1;

        public bool IsOpaqueFullBlock(World world, Vector3i blockPos) =>
            IsVisible(world, blockPos) && IsFullBlock(world, blockPos) &&
            IsTransparent(world, blockPos) == TransparencyType.None;
    }
}
