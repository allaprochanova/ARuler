// aruler_cs.cs — виртуальная линейка (AR) на C# (OpenCvSharp)

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

class ARuler : Form
{
    private VideoCapture capture;
    private Mat frame;
    private List<Point2f> points = new List<Point2f>();
    private bool calibrated = false;
    private double scale = 1.0; // мм/пикс

    public ARuler()
    {
        this.Text = "ARuler";
        this.WindowState = FormWindowState.Maximized;
        this.FormClosing += (s, e) => { capture?.Release(); };
        capture = new VideoCapture(0);
        if (!capture.IsOpened())
        {
            MessageBox.Show("Не удалось открыть камеру");
            Environment.Exit(1);
        }
        this.MouseClick += OnMouseClick;
        this.KeyDown += OnKeyDown;
        Timer timer = new Timer();
        timer.Interval = 30;
        timer.Tick += (s, e) => Invalidate();
        timer.Start();
    }

    private void OnMouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (points.Count < 2)
                points.Add(new Point2f(e.X, e.Y));
            else
            {
                points.Clear();
                points.Add(new Point2f(e.X, e.Y));
            }
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.R) points.Clear();
        else if (e.KeyCode == Keys.C && points.Count == 2)
        {
            // Калибровка
            Console.Write("Введите реальное расстояние в мм: ");
            double real = double.Parse(Console.ReadLine());
            double distPx = Distance(points[0], points[1]);
            scale = real / distPx;
            calibrated = true;
            Console.WriteLine($"Калибровка выполнена: 1 пиксель = {scale:F3} мм");
        }
        else if (e.KeyCode == Keys.S && points.Count == 2 && calibrated)
        {
            double distMm = Distance(points[0], points[1]) * scale;
            using (var sw = new StreamWriter("measurements.txt", true))
                sw.WriteLine($"{DateTime.Now}: {distMm:F1} мм");
            Console.WriteLine("Результат сохранён.");
        }
        else if (e.KeyCode == Keys.Q) Environment.Exit(0);
    }

    private double Distance(Point2f p1, Point2f p2) =>
        Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        frame = new Mat();
        if (capture.Read(frame))
        {
            Cv2.Flip(frame, frame, FlipMode.Y);
            var img = frame.ToBitmap();
            using (Graphics g = Graphics.FromImage(img))
            {
                if (points.Count == 1)
                    g.DrawEllipse(Pens.Green, points[0].X-5, points[0].Y-5, 10, 10);
                else if (points.Count == 2)
                {
                    var p1 = points[0];
                    var p2 = points[1];
                    g.DrawEllipse(Pens.Green, p1.X-5, p1.Y-5, 10, 10);
                    g.DrawEllipse(Pens.Green, p2.X-5, p2.Y-5, 10, 10);
                    g.DrawLine(Pens.Red, p1.X, p1.Y, p2.X, p2.Y);
                    double distPx = Distance(p1, p2);
                    string text = calibrated ? $"{distPx * scale:F1} мм" : $"{distPx:F1} пикс";
                    g.DrawString(text, this.Font, Brushes.Yellow, (p1.X+p2.X)/2-20, (p1.Y+p2.Y)/2-10);
                }
            }
            e.Graphics.DrawImage(img, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new ARuler());
    }
}
