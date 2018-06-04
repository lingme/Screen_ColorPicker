﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZonxScreenColor
{
    public static class ScreenColorGrabberUtil
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        private static Bitmap screenPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        /// <summary>
        /// 获取鼠标指针所在颜色
        /// </summary>
        /// <param name="mousePointerPosition"></param>
        /// <returns></returns>
        public static System.Windows.Media.Color GetColorUnderMousePointer(int mouseX , int mouseY)
        {
            var hDesk = GetDesktopWindow();
            var hSrce = GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, 1, 1);
            var hOldBmp = SelectObject(hDest, hBmp);
            var b = BitBlt(hDest, 0, 0, 1, 1, hSrce, mouseX, mouseY, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);

            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);

            var c = bmp.GetPixel(0, 0);

            bmp.Dispose();

            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// 获取屏幕截图
        /// </summary>
        /// <param name="center"></param>
        /// <param name="roiWidth"></param>
        /// <param name="roiHeight"></param>
        /// <returns></returns>
        public static Bitmap GetScreenArea(System.Windows.Point center, int roiWidth, int roiHeight)
        {
            var origin = new Point((int)center.X - roiWidth / 2, (int)center.Y - roiHeight / 2);

            var hDesk = GetDesktopWindow();
            var hSrce = GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, roiWidth, roiHeight);
            var hOldBmp = SelectObject(hDest, hBmp);
            var b = BitBlt(hDest, 0, 0, roiWidth, roiHeight, hSrce, origin.X, origin.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);

            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);

            return bmp;
        }

        /// <summary>
        /// 位图转WPF支持System.Windows.Media.Imaging.BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        /// <summary>
        /// 绘制中心坐标
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Bitmap DrawCenter(Bitmap bitmap)
        {
            bitmap.SetPixel(bitmap.Size.Width / 2, bitmap.Size.Height / 2, Color.Red);
            return bitmap;
        }
    }
}