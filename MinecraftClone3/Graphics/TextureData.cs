using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace MinecraftClone3.Graphics
{
    internal class TextureData : IDisposable
    {
        private readonly Bitmap _bmp;
        private readonly bool _ownsBmp;

        public readonly BitmapData Data;
        public int Width => Data.Width;
        public int Height => Data.Height;
        public IntPtr DataPtr => Data.Scan0;

        public TextureData(string filename) : this((Bitmap) Image.FromFile(filename))
        {
            _ownsBmp = true;
        }

        public TextureData(Bitmap bitmap)
        {
            _bmp = bitmap;
            _ownsBmp = false;

            Data = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
        }

        public void Dispose()
        {
            _bmp.UnlockBits(Data);
            if(_ownsBmp) _bmp.Dispose();
        }
    }
}