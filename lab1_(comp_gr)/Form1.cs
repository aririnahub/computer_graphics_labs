using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab1__comp_gr_
{
    public partial class Form1 : Form
    {
        Bitmap picture;
        Bitmap image;
        int filterRadius = 15;
        public Form1()
        { 
            InitializeComponent();
            pictureBox1.MouseClick += pictureBox1_MouseClick;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            /*int areaSize = 80;
            int x = e.X;
            int y = e.Y;

            float scaleX = (float)pictureBox1.Width / image.Width;
            float scaleY = (float)pictureBox1.Height / image.Height;

            int imgX = (int)(x - 80 / scaleX);
            int imgY = (int)(y + 80 / scaleY);

            ApplySimpleFilterAroundPoint(imgX, imgY, areaSize);

            pictureBox1.Image = image;
            pictureBox1.Refresh();*/


            // Получаем реальные координаты на изображении
            Point imagePoint = GetActualImagePoint(e.Location);

            // Проверяем, что клик был по изображению
            if (imagePoint.X < 0 || imagePoint.Y < 0 ||
                imagePoint.X >= image.Width || imagePoint.Y >= image.Height)
                return;

            // Применяем фильтр к области вокруг точки
            ApplyGrayScaleFilterAroundPoint(imagePoint.X, imagePoint.Y, filterRadius);

            // Обновляем изображение
            pictureBox1.Image = image;
        }

        private Point GetActualImagePoint(Point mousePoint)
        {
            //if (pictureBox1.Image == null) return Point.Empty;

            // Рассчитываем соотношение сторон
            float imageAspect = (float)image.Width / image.Height;
            float boxAspect = (float)pictureBox1.Width / pictureBox1.Height;

            float scaleFactor, offsetX = 0, offsetY = 0;
            int imgWidth, imgHeight;

            if (imageAspect > boxAspect)
            {
                // Изображение шире, чем PictureBox
                scaleFactor = (float)pictureBox1.Width / image.Width;
                imgWidth = pictureBox1.Width;
                imgHeight = (int)(image.Height * scaleFactor);
                offsetY = (pictureBox1.Height - imgHeight) / 2;
            }
            else
            {
                // Изображение уже или соответствует
                scaleFactor = (float)pictureBox1.Height / image.Height;
                imgHeight = pictureBox1.Height;
                imgWidth = (int)(image.Width * scaleFactor);
                offsetX = (pictureBox1.Width - imgWidth) / 2;
            }
            // Проверяем, попал ли клик в область изображения
            if (mousePoint.X < offsetX || mousePoint.X > offsetX + imgWidth ||
                mousePoint.Y < offsetY || mousePoint.Y > offsetY + imgHeight)
                return new Point(-1, -1);

            // Преобразуем координаты
            int x = (int)((mousePoint.X - offsetX) / scaleFactor);
            int y = (int)((mousePoint.Y - offsetY) / scaleFactor);

            return new Point(x, y);
        }

        private void ApplyGrayScaleFilterAroundPoint(int centerX, int centerY, int radius)
        {
            
            int startX = Math.Max(0, centerX - radius / 2);
            int startY = Math.Max(0, centerY - radius / 2);
            int endX = Math.Min(image.Width - 1, centerX + radius / 2);
            int endY = Math.Min(image.Height - 1, centerY + radius / 2);


            // Обрабатываем каждый пиксель в области
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    // Проверяем, попадает ли пиксель в круг радиуса radius
                    if (Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2)) <= radius)
                    {
                        Color pixel = image.GetPixel(x, y);
                        int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                        image.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }
            }
        }

        /*private void ApplySimpleFilterAroundPoint(int centerX, int centerY, int size)
        {

            int startX = Math.Max(0, centerX - size / 2);
            int startY = Math.Max(0, centerY - size / 2);
            int endX = Math.Min(image.Width - 1, centerX + size / 2);
            int endY = Math.Min(image.Height - 1, centerY + size / 2);


            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    image.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
        }*/



            private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                picture = new Bitmap(dialog.FileName);
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = picture;
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = picture;
            pictureBox1.Refresh();
            image = picture;
        }
        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*BlurFilter filter = new BlurFilter();
            Bitmap resultImage = filter.processImage(image);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
            backgroundWorker1.RunWorkerAsync(filter);*/
            GaussianFilter filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
            {
                image = newImage;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrayScaleFilter filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SepiaFilter filter = new SepiaFilter(); 
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличитьЯркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrighterThanItWas filter = new BrighterThanItWas();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void соболяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SobelFilter filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SharpnessFilter filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EmbossingFilter filter = new EmbossingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RelocateFilter filter = new RelocateFilter();  
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void вертикальныеВолныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VerticalWaveFilter filter  = new VerticalWaveFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void горизоннтальныеВолныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HorizontalWaveFilter filter = new HorizontalWaveFilter();  
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlassFilter filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void motionBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MotionBlurFilter filter = new MotionBlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкость2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SharpnessFilter2 filter = new SharpnessFilter2();
            backgroundWorker1.RunWorkerAsync(filter);
        }


        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Image files | *.png; *.jpg | All files (*.*)|*.*";
            save.FileName = " ";

            if (save.ShowDialog() == DialogResult.OK)
            {
                var newBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.DrawToBitmap(newBitmap, pictureBox1.ClientRectangle);
                newBitmap.Save(save.FileName);
            }
        }

        private void эффектСерыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrayFilter filter = new GrayFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void идеальныйОтражательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IdealReflectorFilter filter = new IdealReflectorFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LineContrastFilter filter = new LineContrastFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnFilter filter = new TurnFilter();  
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void выделениеКонтуровToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BorderFilter filter = new BorderFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void светящиесяКраяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LightBorderFilter filter = new LightBorderFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void матМорфологияToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DilationFilter filter = new DilationFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErosionFilter filter = new ErosionFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFilter filter = new NewFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

       
    }
}
