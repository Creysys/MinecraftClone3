using System;
using System.Collections.Generic;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public class SortedVertexArrayObject : VertexArrayObject
    {
        private class FaceInfo : IComparable<FaceInfo>
        {
            private readonly Vector3 _position;

            public readonly uint[] Indices;

            public FaceInfo(Vector3 position, uint[] indices)
            {
                _position = position;
                Indices = indices;
            }

            public int CompareTo(FaceInfo other)
            {
                var cameraPos = PlayerController.Camera.Position;
                return (int) ((cameraPos - other._position).LengthSquared * 10000 - (cameraPos - _position).LengthSquared * 10000);
            }
        }

        private List<FaceInfo> _faceInfos = new List<FaceInfo>(1024);
        private FaceInfo[] _uploadedFaces;

        public override void AddFace(uint[] indices, Vector3 faceMiddle)
        {
            _indices.AddRange(indices);

            if(_faceInfos == null)
                _faceInfos = new List<FaceInfo>();

            _faceInfos.Add(new FaceInfo(faceMiddle, indices));
        }

        public override void Upload()
        {
            if (IndicesCount <= 0)
            {
                UploadedCount = 0;
                return;
            }

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
                GL.BufferData(BufferTarget.ArrayBuffer, _normals.Count * Vector4.SizeInBytes, _normals.ToArray(),
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(3);
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
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

            _uploadedFaces = _faceInfos.ToArray();

            UploadedCount = _indices.Count;
        }

        public override void Clear()
        {
            base.Clear();

            _faceInfos = null;
        }

        public override void Draw(BeginMode mode)
        {
            if (UploadedCount <= 0) return;

            Array.Sort(_uploadedFaces);

            var sortedIndices = new List<uint>();
            foreach (var face in _uploadedFaces)
                sortedIndices.AddRange(face.Indices);

            GL.BindVertexArray(_vaoId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sortedIndices.Count * sizeof(uint), sortedIndices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.DrawElements(mode, UploadedCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}