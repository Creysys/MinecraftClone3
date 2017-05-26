namespace MinecraftClone3API.Util
{
    public struct Rectangle
    {
        public int MinX;
        public int MinY;

        public int MaxX;
        public int MaxY;

        public int Width => MaxX - MinX;
        public int Height => MaxY - MinY;

        public Rectangle(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public static Rectangle FromSize(int x, int y, int width, int height)
            => new Rectangle(x, y, x + width, y + height);
    }
}