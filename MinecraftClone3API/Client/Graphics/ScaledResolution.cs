﻿using OpenTK;

namespace MinecraftClone3API.Client.Graphics
{
    public static class ScaledResolution
    {
        public static readonly Vector2 GuiResolution = new Vector2(960, 540);
        public static readonly Vector2 GuiPixelSize = new Vector2(1 / GuiResolution.X, 1 / GuiResolution.Y);

        public static Vector2 PixelSize { get; private set; }
        public static Vector2 Resolution { get; private set; }
        public static float AspectRatio { get; private set; }
        public static Vector2 Middle { get; private set; }

        public static float GuiScale { get; private set; }
        public static Vector2 GuiOffset { get; private set; }

        public static void Update()
        {
            var width = ClientResources.Window.Width;
            var height = ClientResources.Window.Height;

            PixelSize = new Vector2(1f / width, 1f / height);
            Resolution = new Vector2(width, height);
            AspectRatio = (float)width / height;
            Middle = new Vector2(width / 2f, height / 2f);

            var xScale = width * GuiPixelSize.X;
            var yScale = height * GuiPixelSize.Y;
            if (yScale > xScale)
            {
                GuiScale = xScale;
                GuiOffset = new Vector2(0, (yScale - xScale) * GuiResolution.Y / 2);
            }
            else
            {
                GuiScale = yScale;
                GuiOffset = new Vector2((xScale - yScale) * GuiResolution.X / 2, 0);
            }
        }
    }
}
