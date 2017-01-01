using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public class VertexArrayObject : IDisposable
    {
        private readonly int _vaoId;
        private readonly int[] _bufferIds = new int[6];
        private readonly int _indicesId;

        private List<Vector3> _positions;
        private List<Vector4> _texCoords;
        private List<Vector4> _overlayTexCoords;
        private List<Vector3> _normals;
        private List<Vector4> _colors;
        private List<Vector4> _overlayColors;
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

        public void Add(Vector3 position, Vector4 texCoord, Vector4 overlayTexCoord, Vector3 normal, Vector4 color, Vector4 overlayColor)
        {
            if (_positions == null)
            {
                _positions = new List<Vector3>(1024);
                _texCoords = new List<Vector4>(1024);
                _overlayTexCoords = new List<Vector4>(1024);
                _normals = new List<Vector3>(1024);
                _colors = new List<Vector4>(1024);
                _overlayColors = new List<Vector4>(1024);

                _indices = new List<uint>(1024);
            }

            _positions.Add(position);
            _texCoords.Add(texCoord);
            _overlayTexCoords.Add(overlayTexCoord);
            _normals.Add(normal);
            _colors.Add(color);
            _overlayColors.Add(overlayColor);
        }
        
        public void AddIndices(uint[] indices) => _indices.AddRange(indices);

        public void Upload()
        {
            if (IndicesCount <= 0) return;

            GL.BindVertexArray(_vaoId);

            if (UploadedCount == 0)
            {
                //0 positions
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _positions.Count * Vector3.SizeInBytes, _positions.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                //1 texCoords
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[1]);
                GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector4.SizeInBytes, _texCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);
                //2 overlayTexCoords
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[2]);
                GL.BufferData(BufferTarget.ArrayBuffer, _overlayTexCoords.Count * Vector4.SizeInBytes, _overlayTexCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, 0);
                //3 normals
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[3]);
                GL.BufferData(BufferTarget.ArrayBuffer, _normals.Count * Vector3.SizeInBytes, _normals.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(3);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                //4 colors
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[4]);
                GL.BufferData(BufferTarget.ArrayBuffer, _colors.Count * Vector4.SizeInBytes, _colors.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(4);
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
                //5 overlayColors
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[5]);
                GL.BufferData(BufferTarget.ArrayBuffer, _overlayColors.Count * Vector4.SizeInBytes, _overlayColors.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(5);
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            else
            {
                //0 positions
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _positions.Count * Vector3.SizeInBytes, _positions.ToArray(),
                    BufferUsageHint.StaticDraw);
                //1 texCoords
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[1]);
                GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector4.SizeInBytes, _texCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
                //2 overlayTexCoords
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[2]);
                GL.BufferData(BufferTarget.ArrayBuffer, _overlayTexCoords.Count * Vector4.SizeInBytes, _overlayTexCoords.ToArray(),
                    BufferUsageHint.StaticDraw);
                //3 normals
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[3]);
                GL.BufferData(BufferTarget.ArrayBuffer, _normals.Count * Vector3.SizeInBytes, _normals.ToArray(),
                    BufferUsageHint.StaticDraw);
                //4 colors
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[4]);
                GL.BufferData(BufferTarget.ArrayBuffer, _colors.Count * Vector4.SizeInBytes, _colors.ToArray(),
                    BufferUsageHint.StaticDraw);
                //5 overlayColors
                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufferIds[5]);
                GL.BufferData(BufferTarget.ArrayBuffer, _overlayColors.Count * Vector4.SizeInBytes, _overlayColors.ToArray(),
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
            _overlayTexCoords = null;
            _normals = null;
            _colors = null;
            _overlayColors = null;
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