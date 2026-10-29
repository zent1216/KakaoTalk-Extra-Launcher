// 앱 아이콘 그림. 아이콘 파일 생성(MakeIcon)과 런처 헤더에서 같이 쓴다.
// 남색 둥근 사각형 위에 말풍선 두 개, 앞쪽 말풍선에 "+" 표시.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace KakaoExtra
{
    static class Shapes
    {
        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    static class IconArt
    {
        static readonly Color BgTop = Color.FromArgb(108, 122, 246);
        static readonly Color BgBottom = Color.FromArgb(56, 64, 182);
        static readonly Color Back = Color.FromArgb(200, 207, 252);
        static readonly Color Front = Color.FromArgb(255, 203, 64);
        static readonly Color Plus = Color.FromArgb(40, 45, 118);

        public static void Draw(Graphics g, float x, float y, float s)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var bg = new RectangleF(x, y, s, s);
            using (var path = Shapes.Round(bg, s * 0.23f))
            using (var brush = new LinearGradientBrush(new RectangleF(x, y - 1, s, s + 2), BgTop, BgBottom, 90f))
                g.FillPath(brush, path);

            bool small = s < 24;
            if (!small)
            {
                using (var b = new SolidBrush(Back))
                    FillBubble(g, b, x + s * 0.13f, y + s * 0.16f, s * 0.50f, s * 0.40f, false);
            }

            float fw = small ? s * 0.70f : s * 0.56f;
            float fh = fw * 0.80f;
            float fx = small ? x + (s - fw) / 2 : x + s * 0.32f;
            float fy = small ? y + s * 0.15f : y + s * 0.37f;
            using (var b = new SolidBrush(Front))
                FillBubble(g, b, fx, fy, fw, fh, true);

            float cx = fx + fw / 2, cy = fy + fh / 2;
            float arm = fw * 0.21f;
            float th = Math.Max(1.6f, fw * 0.11f);
            using (var pen = new Pen(Plus, th))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, cx - arm, cy, cx + arm, cy);
                g.DrawLine(pen, cx, cy - arm, cx, cy + arm);
            }
        }

        // 타원과 꼬리를 따로 칠해 겹치는 부분이 비지 않게 한다.
        static void FillBubble(Graphics g, Brush b, float x, float y, float w, float h, bool tailRight)
        {
            g.FillEllipse(b, x, y, w, h);
            PointF[] tail = tailRight
                ? new[] { new PointF(x + w * 0.60f, y + h * 0.82f), new PointF(x + w * 0.90f, y + h * 1.12f), new PointF(x + w * 0.84f, y + h * 0.60f) }
                : new[] { new PointF(x + w * 0.16f, y + h * 0.60f), new PointF(x + w * 0.08f, y + h * 1.08f), new PointF(x + w * 0.42f, y + h * 0.84f) };
            g.FillPolygon(b, tail);
        }
    }
}
