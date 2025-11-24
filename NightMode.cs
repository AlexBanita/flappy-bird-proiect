using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace flappy_bird_proiect
{
    public class NightMode
    {
        public float Transition = 0f;
        private int direction = 1;
        private bool isTransitioning = false;

        Random rnd = new Random();
        Point[] stars;

        public NightMode()
        {
            stars = new Point[40];
            for (int i = 0; i < stars.Length; i++)
                stars[i] = new Point(rnd.Next(0, 800), rnd.Next(0, 600));
        }

        public void Update()
        {
            if (!isTransitioning)
                return;

            Transition += direction * 0.02f;

            if (Transition >= 1f)
            {
                Transition = 1f;
                isTransitioning = false;
            }
            else if (Transition <= 0f)
            {
                Transition = 0f;
                isTransitioning = false;
            }
        }

        public void Toggle()
        {
            if (isTransitioning)
                return;

            direction = (Transition == 0f ? 1 : -1);
            isTransitioning = true;
        }

        public void Draw(Graphics g, int width, int height)
        {
            Color daySky = Color.FromArgb(135, 206, 250);
            Color nightSky = Color.FromArgb(10, 10, 40);
            Color sky = LerpColor(daySky, nightSky, Transition);

            using (SolidBrush b = new SolidBrush(sky))
                g.FillRectangle(b, 0, 0, width, height);

            if (Transition > 0.05f)
            {
                int alpha = (int)(Transition * 255);
                using (SolidBrush starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                {
                    foreach (var s in stars)
                        g.FillEllipse(starBrush, s.X, s.Y, 3, 3);
                }
            }
        }

        private Color LerpColor(Color a, Color b, float t)
        {
            t = Math.Max(0, Math.Min(1, t));

            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t)
            );
        }
    }
}
