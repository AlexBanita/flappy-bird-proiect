using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace flappy_bird_proiect
{
    public partial class Form1 : Form
    {
        Bird birdObj;
        Rectangle pipeTop;
        Rectangle pipeBottom;

        NightMode night = new NightMode();

        int pipeSpeed = 6;
        int score = 0;
        bool gameRunning = false;
        bool noDeath = false;
        string difficulty = "Easy";

        Random rnd = new Random();
        Timer gameTimer = new Timer();

        Label title;
        Button btnPlay;
        Button btnDifficulty;
        Button btnHighscore;
        Button btnExit;

        Panel panelDifficulty;
        Button btnEasy;
        Button btnMedium;
        Button btnHard;
        Button btnNoDeath;
        Button btnBack;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
            ActiveControl = null;
            Width = 500;
            Height = 500;
            Text = "Flappy Bird";

            CreateDatabase();
            CreateMenu();
            CreateDifficultyPanel();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;

            KeyDown += Form1_KeyDown;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && gameRunning) return false;
            if (keyData == Keys.Space) return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void CreateDatabase()
        {
            if (!System.IO.File.Exists("highscore.db"))
            {
                SQLiteConnection.CreateFile("highscore.db");
                using (var conn = new SQLiteConnection("Data Source=highscore.db"))
                {
                    conn.Open();
                    string query =
                    @"CREATE TABLE Highscore(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Score INTEGER NOT NULL,
                        Difficulty TEXT NOT NULL
                    );";
                    using (var cmd = new SQLiteCommand(query, conn))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreateMenu()
        {
            title = new Label
            {
                Text = "FLAPPY BIRD",
                Font = new Font("Arial", 32, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100
            };
            Controls.Add(title);

            btnPlay = MakeButton("PLAY", 150, () => StartGame());
            btnDifficulty = MakeButton("DIFFICULTY", 220, () => ShowDifficultyPanel());
            btnHighscore = MakeButton("HIGHSCORE", 290, () => ShowHighscore());
            btnExit = MakeButton("EXIT", 360, () => Application.Exit());
        }

        private Button MakeButton(string text, int y, Action action)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Arial", 18),
                Size = new Size(200, 50),
                Location = new Point(150, y)
            };
            b.Click += (s, e) => action();
            Controls.Add(b);
            return b;
        }

        private void CreateDifficultyPanel()
        {
            panelDifficulty = new Panel
            {
                Size = new Size(500, 500),
                Visible = false
            };
            Controls.Add(panelDifficulty);

            var titleDiff = new Label
            {
                Text = "SELECT DIFFICULTY",
                Font = new Font("Arial", 22, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };
            panelDifficulty.Controls.Add(titleDiff);

            btnEasy = MakeDiffButton("EASY", 120, () => SetDifficulty("Easy", 5, 5, false));
            btnMedium = MakeDiffButton("MEDIUM", 190, () => SetDifficulty("Medium", 6, 6, false));
            btnHard = MakeDiffButton("HARD", 260, () => SetDifficulty("Hard", 7, 7, false));
            btnNoDeath = MakeDiffButton("NO-DEATH MODE", 330, () => SetDifficulty("Training", 5, 5, true));

            btnBack = new Button
            {
                Text = "BACK",
                Font = new Font("Arial", 18),
                Size = new Size(250, 50),
                Location = new Point(120, 400)
            };
            btnBack.Click += (s, e) => HideDifficultyPanel();
            panelDifficulty.Controls.Add(btnBack);
        }

        private Button MakeDiffButton(string text, int y, Action action)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Arial", 18),
                Size = new Size(250, 50),
                Location = new Point(120, y)
            };
            b.Click += (s, e) => action();
            panelDifficulty.Controls.Add(b);
            return b;
        }

        private void SetDifficulty(string diff, int grav, int speed, bool nd)
        {
            difficulty = diff;
            pipeSpeed = speed;
            noDeath = nd;

            if (birdObj == null) Bird.DefaultGravity = grav;
            else birdObj.Gravity = grav;

            HideDifficultyPanel();
        }

        private void ShowDifficultyPanel()
        {
            panelDifficulty.Visible = true;
            title.Visible = false;
            btnPlay.Visible = false;
            btnDifficulty.Visible = false;
            btnHighscore.Visible = false;
            btnExit.Visible = false;
        }

        private void HideDifficultyPanel()
        {
            panelDifficulty.Visible = false;
            title.Visible = true;
            btnPlay.Visible = true;
            btnDifficulty.Visible = true;
            btnHighscore.Visible = true;
            btnExit.Visible = true;
        }

        private void StartGame()
        {
            title.Visible = false;
            btnPlay.Visible = false;
            btnDifficulty.Visible = false;
            btnHighscore.Visible = false;
            btnExit.Visible = false;

            ActiveControl = null;
            night.Transition = 0f;

            birdObj = new Bird(60, 200, 45, 35);
            SpawnPipes();

            score = 0;
            gameRunning = true;
            gameTimer.Start();
        }

        private void SpawnPipes()
        {
            int gap = 150;
            int topHeight = rnd.Next(50, 250);

            int startX = this.ClientSize.Width; // poziția corectă pentru marginea dreaptă

            pipeTop = new Rectangle(startX, 0, 80, topHeight);
            pipeBottom = new Rectangle(startX, topHeight + gap, 80, this.ClientSize.Height);
        }


        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameRunning) return;

            night.Update();
            birdObj.Update();

            pipeTop.X -= pipeSpeed;
            pipeBottom.X -= pipeSpeed;

            if (pipeTop.X < -100)
            {
                score++;
                SpawnPipes();
            }

            if (!noDeath)
            {
                if (birdObj.Body.Y > Height - 15 ||
                    birdObj.Body.Y < -10 ||
                    birdObj.Body.IntersectsWith(pipeTop) ||
                    birdObj.Body.IntersectsWith(pipeBottom))
                {
                    GameOver();
                }
            }

            Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && gameRunning) birdObj.Jump();
            if (e.KeyCode == Keys.N) night.Toggle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            night.Draw(g, Width, Height);
            DrawDayElements(g);

            if (gameRunning)
            {
                DrawPipe(g, pipeTop, true);
                DrawPipe(g, pipeBottom, false);
                birdObj.Draw(g);

                g.DrawString("Score: " + score, new Font("Arial", 20), Brushes.White, 10, 10);
                g.DrawString("Difficulty: " + difficulty, new Font("Arial", 14), Brushes.White, 10, 40);
            }
            else if (!title.Visible)
            {
                DrawGameOver(g);
            }
        }

        private void DrawDayElements(Graphics g)
        {
            float alpha = 1f - night.Transition;
            if (alpha <= 0.01f) return;

            Color cloudColor = Color.FromArgb((int)(alpha * 255), Color.White);
            Color grassColor = Color.FromArgb((int)(alpha * 255), 100, 220, 120);
            Color dirtColor = Color.FromArgb((int)(alpha * 255), 210, 180, 100);

            using (var cloud = new SolidBrush(cloudColor))
            {
                DrawCloud(g, 60, 80, cloud);
                DrawCloud(g, 220, 40, cloud);
                DrawCloud(g, 300, 140, cloud);
            }

            using (var grass = new SolidBrush(grassColor))
                g.FillRectangle(grass, 0, 420, Width, 80);

            using (var dirt = new SolidBrush(dirtColor))
                g.FillRectangle(dirt, 0, 460, Width, 40);
        }

        private void DrawCloud(Graphics g, int x, int y, Brush b)
        {
            g.FillEllipse(b, x, y, 120, 60);
            g.FillEllipse(b, x + 40, y - 20, 140, 80);
            g.FillEllipse(b, x + 80, y, 120, 60);
        }

        private void DrawPipe(Graphics g, Rectangle rect, bool top)
        {
            using (var b = new LinearGradientBrush(rect, Color.FromArgb(0, 180, 0), Color.FromArgb(0, 120, 0), 90))
                g.FillRectangle(b, rect);

            Rectangle head = top
                ? new Rectangle(rect.X, rect.Bottom - 20, rect.Width, 20)
                : new Rectangle(rect.X, rect.Y, rect.Width, 20);

            g.FillRectangle(Brushes.DarkGreen, head);
            g.DrawRectangle(Pens.Black, rect);
        }

        private void DrawGameOver(Graphics g)
        {
            string txt = "GAME OVER";
            Font f = new Font("Arial", 42, FontStyle.Bold);
            SizeF sz = g.MeasureString(txt, f);

            g.DrawString(txt, f, Brushes.Black, (Width - sz.Width) / 2 + 3, (Height - sz.Height) / 2 + 3);
            g.DrawString(txt, f, Brushes.White, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
        }

        private void GameOver()
        {
            gameRunning = false;
            gameTimer.Stop();

            SaveScore();

            var result = MessageBox.Show($"Ai pierdut!\nScor: {score}\n\nVrei să joci din nou?", "GAME OVER", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) StartGame();
            else
            {
                title.Visible = true;
                btnPlay.Visible = true;
                btnDifficulty.Visible = true;
                btnHighscore.Visible = true;
                btnExit.Visible = true;
            }
        }

        private void SaveScore()
        {
            using (var conn = new SQLiteConnection("Data Source=highscore.db"))
            {
                conn.Open();
                string query = "INSERT INTO Highscore (Score, Difficulty) VALUES (@s, @d)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", score);
                    cmd.Parameters.AddWithValue("@d", difficulty);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ShowHighscore()
        {
            using (var conn = new SQLiteConnection("Data Source=highscore.db"))
            {
                conn.Open();
                string query = "SELECT Score, Difficulty FROM Highscore ORDER BY Score DESC LIMIT 10";
                using (var cmd = new SQLiteCommand(query, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    string text = "TOP 10 SCORES:\n\n";
                    while (rd.Read())
                        text += $"{rd.GetInt32(0)} pts  -  {rd.GetString(1)}\n";

                    MessageBox.Show(text, "HIGHSCORE");
                }
            }
        }
    }
}
