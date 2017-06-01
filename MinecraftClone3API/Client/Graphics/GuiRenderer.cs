using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Client.Graphics
{
    public static class GuiRenderer
    {
        public static void DrawTexture(Texture texture, Rectangle rect, Rectangle? uvRect, bool gui = true)
        {
            //Convert pixel space to normalized coords (0)-(1)
            var r = new Vector4(rect.MinX, rect.MinY, rect.MaxX, rect.MaxY);

            var pixelSize = new Vector4(
                ScaledResolution.PixelSize.X, ScaledResolution.PixelSize.Y,
                ScaledResolution.PixelSize.X, ScaledResolution.PixelSize.Y);

            uvRect = uvRect ?? new Rectangle(0, 0, texture.Width, texture.Height);
            var uvrect = new Vector4((float) uvRect.Value.MinX / texture.Width, (float) uvRect.Value.MinY / texture.Height,
                (float) uvRect.Value.MaxX / texture.Width, (float) uvRect.Value.MaxY / texture.Height);

            if (gui)
                DrawTexture(texture, (ScaledResolution.GuiScale * r + new Vector4(ScaledResolution.GuiOffset.X,
                                          ScaledResolution.GuiOffset.Y, ScaledResolution.GuiOffset.X,
                                          ScaledResolution.GuiOffset.Y)) * pixelSize, uvrect);
            else
                DrawTexture(texture, r * pixelSize, uvrect);
        }

        public static void DrawTexture(Texture texture, Vector4 rect, Vector4 uvRect)
        {
            //Convert to clip space (-1)-(+1)
            rect = rect * 2 + new Vector4(-1);

            ClientResources.SpriteShader.Bind();

            GL.Uniform4(0, rect);
            GL.Uniform4(1, uvRect);

            texture.Bind(TextureUnit.Texture0);
            ClientResources.ScreenRectVao.Draw();
        }
    }
}