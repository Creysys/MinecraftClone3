using System.IO;
using MinecraftClone3.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3.Graphics
{
    internal class Shader
    {
        public const string FragmentShaderExt = ".fs";
        public const string VertexShaderExt = ".vs";

        private readonly int _programId;


        public Shader(string path)
            : this(path, File.ReadAllText(path + FragmentShaderExt), File.ReadAllText(path + VertexShaderExt))
        {
        }

        public Shader(string name, string fsSource, string vsSource)
        {
            _programId = GL.CreateProgram();

            AttachShader(ShaderType.FragmentShader, fsSource);
            AttachShader(ShaderType.VertexShader, vsSource);

            GL.LinkProgram(_programId);
            var infoLog = GL.GetProgramInfoLog(_programId);
            if (!string.IsNullOrEmpty(infoLog))
                Logger.Error($"There was an error linking shader \"{name}\": {infoLog}");
        }

        public void Bind() => GL.UseProgram(_programId);


        private void AttachShader(ShaderType type, string source)
        {
            var id = GL.CreateShader(type);
            GL.ShaderSource(id, source);
            GL.CompileShader(id);
            GL.AttachShader(_programId, id);
        }
    }
}
