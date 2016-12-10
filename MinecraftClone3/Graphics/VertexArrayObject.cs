using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3.Graphics
{
    internal class VertexArrayObject : IDisposable
    {
        private readonly int _vaoId;
        private readonly int[] _bufferIds = new int[2];
        private readonly int _indicesId;

        private List<Vector3> _positions;
        private List<Vector3> _texCoords;
        private List<uint> _indices;

        public int UploadedCount;
        public int VertexCount => (_positions?.Count).GetValueOrDefault();
        public int IndicesCount => (_indices?.Count).GetValueOrDefault();

        public VertexArrayObject()
        {
            _vaoId = GL.GenVertexArray();
            GL.GenBuffers(_bufferIds.Length, _bufferIds);
            _indicesId = GL.GenBuffer();
        }

        public void Add(Vector3 position, Vector3 texCoord)
        {
            if (_positions == null)
            {
                _positions = new List<Vector3>(1024);
                _texCoords = new List<Vector3>(1024);
                _indices = new List<uint>(1024);
            }

            _positions.Add(position);
            _texCoords.Add(texCoord);
        }

        public void AddIndices(uint[] indices) => _indices.AddRange(indices);

        public void Upload()
        {
            if (IndicesCount <= 0) return;

            GL.BindVertexArray(_vaoId);

            if (UploadedCount == 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _positions.Count * Vector3.SizeInBytes, _positions.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[1]);
                GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector3.SizeInBytes, _texCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _positions.Count * Vector3.SizeInBytes, _positions.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[1]);
                GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector3.SizeInBytes, _texCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(),
                BufferUsageHint.StaticDraw);

            UploadedCount = _indices.Count;
        }

        public void Draw()
        {
            if (UploadedCount <= 0) return;

            GL.BindVertexArray(_vaoId);
            GL.DrawElements(BeginMode.Triangles, UploadedCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Clear()
        {
            _positions = null;
            _texCoords = null;
            _indices = null;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_indicesId);
            GL.DeleteBuffers(_bufferIds.Length, _bufferIds);
            GL.DeleteVertexArray(_vaoId);
        }
    }
}