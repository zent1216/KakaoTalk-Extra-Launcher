// 빌드 도구: IconArt 그림으로 여러 크기의 PNG를 담은 .ico 파일을 만든다.
// 사용법: MakeIcon.exe <출력.ico> [미리보기.png]

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace KakaoExtra
{
    static class MakeIcon
    {
        static int Main(string[] args)
        {
            if (args.Length < 1) { Console.Error.WriteLine("usage: MakeIcon <out.ico> [preview.png]"); return 1; }
            int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var images = new List<byte[]>();
            foreach (int s in sizes) images.Add(Render(s));

            using (var fs = File.Create(args[0]))
            using (var w = new BinaryWriter(fs))
            {
                w.Write((short)0);
                w.Write((short)1);
                w.Write((short)sizes.Length);
                int offset = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
                    w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
                    w.Write((byte)0);
                    w.Write((byte)0);
                    w.Write((short)1);
                    w.Write((short)32);
                    w.Write(images[i].Length);
                    w.Write(offset);
                    offset += images[i].Length;
                }
                foreach (var img in images) w.Write(img);
            }

            if (args.Length > 1)
            {
                // 크기별 미리보기 한 장
                int total = 0;
                foreach (int s in sizes) total += s + 12;
                using (var bmp = new Bitmap(total + 12, 256 + 24, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    int x = 12;
                    foreach (int s in sizes)
                    {
                        IconArt.Draw(g, x, 12 + (256 - s), s);
                        x += s + 12;
                    }
                    bmp.Save(args[1], ImageFormat.Png);
                }
            }
            return 0;
        }

        static byte[] Render(int size)
        {
            using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    IconArt.Draw(g, 0, 0, size);
                }
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
    }
}
