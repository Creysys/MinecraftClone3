using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinecraftClone3API.Blocks;

namespace MinecraftClone3API.Util
{
    public class BlockRegistry : Registry<Block>
    {
        public static readonly Block BlockAir = new BlockAir();

        private readonly Dictionary<ushort, Block> _idsToBlocks = new Dictionary<ushort, Block>();
        private readonly Dictionary<string, ushort> _keysToIds = new Dictionary<string, ushort>();

        public BlockRegistry()
        {
            Register("System", BlockAir);
        }

        public Block this[ushort id] => _idsToBlocks[id];

        public sealed override void Register(string prefix, Block block)
        {
            base.Register(prefix, block);

            block.Id = GetBlockId(block);
            _idsToBlocks.Add(block.Id, block);
            _keysToIds.Add(block.RegistryKey, block.Id);
        }

        internal List<string> GetMissingBlocks()
            => _keysToIds.Where(entry => !_idsToBlocks.ContainsKey(entry.Value)).Select(entry => entry.Key).ToList();

        internal void Write(BinaryWriter writer)
        {
            writer.Write(_keysToIds.Count);
            foreach (var entry in _keysToIds)
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
        }

        internal void Read(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                _keysToIds.Add(reader.ReadString(), reader.ReadUInt16());
        }

        private ushort GetBlockId(Block block)
        {
            if (_keysToIds.TryGetValue(block.RegistryKey, out var id)) return id;
            
            id = 0;
            while (true)
            {
                if (!_idsToBlocks.ContainsKey(id)) return id;
                id++;
            }
        }
    }
}