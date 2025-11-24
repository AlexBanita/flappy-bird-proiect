using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace flappy_bird_proiect
{
    public class Bird
    {
        public Rectangle Body;
        private float wingAngle = 0;
        private float wingSpeed = 1.5f;
        private float wingDirection = 1;

        public int Gravity = 5;
        public int JumpForce = 45;
        public static int DefaultGravity = 5;

        public Bird(int x, int y, int width, int height)
        {
            Body = new Rectangle(x, y, width, height);
            Gravity = DefaultGravity;
        }

        public void Update()
        {
            Body.Y += Gravity;
            wingAngle += wingSpeed * wingDirection;

            if (wingAngle > 25) wingDirection = -1;
            if (wingAngle < -25) wingDirection = 1;
        }

        public void Jump()
        {
            Body.Y -= JumpForce;
            if (Body.Y < 5) Body.Y = 5;
        }

        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillEllipse(Brushes.Gold, Body);

            g.FillEllipse(Brushes.White,
                Body.X + Body.Width * 0.55f,
                Body.Y + Body.Height * 0.25f,
                Body.Width * 0.30f,
                Body.Height * 0.30f);

            g.FillEllipse(Brushes.Black,
                Body.X + Body.Width * 0.68f,
                Body.Y + Body.Height * 0.35f,
                Body.Width * 0.10f,
                Body.Height * 0.10f);

            g.FillPolygon(Brushes.Orange, new Point[]
            {
                new Point(Body.X + Body.Width, Body.Y + Body.Height / 2),
                new Point(Body.X + Body.Width + 12, Body.Y + Body.Height / 2 - 5),
                new Point(Body.X + Body.Width + 12, Body.Y + Body.Height / 2 + 5)
            });

            DrawWing(g);
        }

        private void DrawWing(Graphics g)
        {
            Rectangle wing = new Rectangle(
                Body.X + (int)(Body.Width * 0.1),
                Body.Y + (int)(Body.Height * 0.4),
                (int)(Body.Width * 0.45),
                (int)(Body.Height * 0.45)
            );

            PointF center = new PointF(
                wing.X + wing.Width / 2f,
                wing.Y + wing.Height / 2f
            );

            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(wingAngle);
            g.TranslateTransform(-center.X, -center.Y);

            g.FillEllipse(Brushes.Orange, wing);

            g.ResetTransform();
        }
    }
}
