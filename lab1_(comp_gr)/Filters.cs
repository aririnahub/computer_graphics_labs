using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Markup;
using lab1__comp_gr_;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;

namespace lab1__comp_gr_ {
    public abstract class Filters {
        public abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        
        virtual public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                {
                    return null;
                }
            }

            for (int i = 0; i< sourceImage.Width; i++)
            {
                for (int j =0; j< sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            
            return value;
        }
    }

    class InvertFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensityR = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            int intensityG = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            int intensityB = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color Intensity = Color.FromArgb(intensityR, intensityG, intensityB);
            return Intensity;
        }
    }

    class BrighterThanItWas : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensityR = (sourceColor.R + 30);
            int intensityG = (sourceColor.G + 30);
            int intensityB = (sourceColor.B + 30);
            return Color.FromArgb(
               Clamp(intensityR, 0, 255),
               Clamp(intensityG, 0, 255),
               Clamp(intensityB, 0, 255)
               );
        }
    }


    class SepiaFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            int k = 15;
            int resR = Intensity + 2*k;
            int resG = (int)(Intensity + 0.5 * k);
            int resB = Intensity - 1 * k;
            return Color.FromArgb(
               Clamp((int)resR, 0, 255),
               Clamp((int)resG, 0, 255),
               Clamp((int)resB, 0, 255)
               );

        }
    }


class RelocateFilter : Filters
{
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        Color sourceColor = sourceImage.GetPixel(Clamp(x + 50, 0, sourceImage.Width - 1), y);
        Color resultColor;
        if (x + 50 > sourceImage.Width)
        {
            resultColor = Color.Black;
        }
        else
            resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        return resultColor;
    }
}

class VerticalWaveFilter : Filters
{
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        Color sourceColor = sourceImage.GetPixel(x, Clamp((int)(y + 20 * Math.Sin(2 * Math.PI * x / 60)), 0, sourceImage.Height - 1));
        Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        return resultColor;
    }
}

class HorizontalWaveFilter : Filters
{
    public override Color calculateNewPixelColor(Bitmap sourceImage, int k, int l)
    {
        Color sourceColor = sourceImage.GetPixel(Clamp((int)(k + 20 * Math.Sin(2 * Math.PI * l / 60)), 0, sourceImage.Width - 1), l);
        Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        return resultColor;
    }
}

class NewFilter : Filters
    {
        private List<Filters> filters = new List<Filters>
    {
        new GlassFilter(),
        new TurnFilter(),
        new RelocateFilter(),
        new HorizontalWaveFilter(),
        new VerticalWaveFilter(),
        new SepiaFilter(),
        new InvertFilter(),
        new GrayScaleFilter(),
        new BrighterThanItWas()
    };

        private Filters topLeftFilter;
        private Filters topRightFilter;
        private Filters bottomLeftFilter;
        private Filters bottomRightFilter;

        public NewFilter()
        {
            Random random = new Random();
            topLeftFilter = filters[random.Next(filters.Count)];
            topRightFilter = filters[random.Next(filters.Count)];
            bottomLeftFilter = filters[random.Next(filters.Count)];
            bottomRightFilter = filters[random.Next(filters.Count)];
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;

            bool belowMainDiagonal = y > (float)height / width * x;
            bool belowAntiDiagonal = y > (float)height / width * (width - x);

            if (!belowMainDiagonal && !belowAntiDiagonal)
            {
                return topLeftFilter.calculateNewPixelColor(sourceImage, x, y);
            }
            else if (belowMainDiagonal && !belowAntiDiagonal)
            {
                return topRightFilter.calculateNewPixelColor(sourceImage, x, y);
            }
            else if (belowMainDiagonal && belowAntiDiagonal)
            {
                return bottomRightFilter.calculateNewPixelColor(sourceImage, x, y);
            }
            else
            {
                return bottomLeftFilter.calculateNewPixelColor(sourceImage, x, y);
            }
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int x = 0; x < sourceImage.Width; x++)
            {
                worker.ReportProgress((int)((float)x / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color newColor = calculateNewPixelColor(sourceImage, x, y);
                    resultImage.SetPixel(x, y, newColor);
                }
            }

            return resultImage;
        }
    }

    class GlassFilter : Filters
{
    Random rand = new Random();
    Random rand1 = new Random();
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {

        Color sourceColor = sourceImage.GetPixel(Clamp((int)(x + (rand.Next(-3, 4))), 0, sourceImage.Width - 1), Clamp((int)(y + (rand1.Next(-3, 4))), 0, sourceImage.Height - 1));
        Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        return resultColor;
    }
}

class LineContrastFilter : Filters
{
    int minR = 255, minG = 255, minB = 255;
    int maxR = 0, maxG = 0, maxB = 0;

    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 100));
            if (worker.CancellationPending)
            {
                return null;
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                Color color = sourceImage.GetPixel(i, j);
                minR = Math.Min(minR, color.R);
                minG = Math.Min(minG, color.G);
                minB = Math.Min(minB, color.B);
                maxR = Math.Max(maxR, color.R);
                maxG = Math.Max(maxG, color.G);
                maxB = Math.Max(maxB, color.B);
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }
        return resultImage;

    }

    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        Color sourceColor = sourceImage.GetPixel(x, y);
        int resultR = Clamp((sourceColor.R - minR) * 255 / (maxR - minR), 0, 255);
        int resultG = Clamp((sourceColor.G - minG) * 255 / (maxG - minG), 0, 255);
        int resultB = Clamp((sourceColor.B - minB) * 255 / (maxB - minB), 0, 255);

        return Color.FromArgb(
            Clamp(resultR, 0, 255),
            Clamp(resultG, 0, 255),
            Clamp(resultB, 0, 255)
            );

    }
}

class GrayFilter : Filters
{
    float sumR = 0;
    float sumG = 0;
    float sumB = 0;
    float avg = 0;

    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 100));
            if (worker.CancellationPending)
            {
                return null;
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                Color color = sourceImage.GetPixel(i, j);
                sumR += color.R;
                sumG += color.G;
                sumB += color.B;
            }
        }
        sumR /= sourceImage.Width * sourceImage.Height;
        sumG /= sourceImage.Width * sourceImage.Height;
        sumB /= sourceImage.Width * sourceImage.Height;
        avg = (sumR + sumG + sumB) / 3;

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }
        return resultImage;
    }

    //private void sumColor(Bitmap sourceImage)
    //{
    //    for (int i = 0; i < sourceImage.Width; i++)
    //    {
    //        for (int j = 0; j < sourceImage.Height; j++)
    //        {
    //            Color color = sourceImage.GetPixel(i, j);
    //            sumR += color.R;
    //            sumG += color.G;
    //            sumB += color.B;
    //        }
    //    }
    //    sumR /= sourceImage.Width * sourceImage.Height;
    //    sumG /= sourceImage.Width * sourceImage.Height;
    //    sumB /= sourceImage.Width * sourceImage.Height;
    //    avg = (sumR + sumG + sumB) / 3;
    //}


    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {

        Color sourceColor = sourceImage.GetPixel(x, y);
        float resultR = sourceColor.R * avg / sumR;
        float resultG = sourceColor.G * avg / sumG;
        float resultB = sourceColor.B * avg / sumB;

        return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
    }
}


class IdealReflectorFilter : Filters
{
    int maxR = 0, maxG = 0, maxB = 0;

    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 100));
            if (worker.CancellationPending)
            {
                return null;
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                Color color = sourceImage.GetPixel(i, j);

                maxR = Math.Max(maxR, color.R);
                maxG = Math.Max(maxG, color.G);
                maxB = Math.Max(maxB, color.B);
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            for (int j = 0; j < sourceImage.Height; j++)
            {
                resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }
        return resultImage;
    }

    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        Color sourceColor = sourceImage.GetPixel(x, y);
        int resultR = Clamp((sourceColor.R * 255 / maxR), 0, 255);
        int resultG = Clamp((sourceColor.G * 255 / maxG), 0, 255);
        int resultB = Clamp((sourceColor.B * 255 / maxB), 0, 255);

        return Color.FromArgb(
            Clamp(resultR, 0, 255),
            Clamp(resultG, 0, 255),
            Clamp(resultB, 0, 255)
            );

    }
}

 //--------------------------------------------------------------------------------------


class MatrixFilters : Filters
{
    protected float[,] kernel = null;
    protected MatrixFilters() { }
    public MatrixFilters(float[,] kernel)
    {
        this.kernel = kernel;
    }

    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int radiusX = kernel.GetLength(0) / 2;
        int radiusY = kernel.GetLength(1) / 2;
        float resultR = 0;
        float resultG = 0;
        float resultB = 0;
        for (int l = -radiusY; l <= radiusY; l++)
        {
            for (int k = -radiusX; k <= radiusY; k++)
            {
                int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                Color neighborColor = sourceImage.GetPixel(idX, idY);
                resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

            }
        }
        return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );

    }
    }
}
class SobelFilter : MatrixFilters
{
    public SobelFilter()
    {
        int sizeX = 3;
        int sizeY = 3;
        int[,] g_x = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        int[,] g_y = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        kernel = new float[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                kernel[i, j] = g_x[i, j] + g_y[i, j];
            }
        }
    }
}


class BlurFilter : MatrixFilters
{
    public BlurFilter()
    {
        int sizeX = 3;
        int sizeY = 3;
        kernel = new float[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
            }
        }
    }
}

class GaussianFilter : MatrixFilters
{
    public GaussianFilter()
    {
        createGaussianKernel(3, 2);
    }
    public void createGaussianKernel(int radius, float sigma)
    {
        int size = 2 * radius + 1;
        kernel = new float[size, size];
        float norm = 0;
        for (int i = -radius; i < radius; i++)
        {
            for (int j = -radius; j < radius; j++)
            {
                kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                norm += kernel[i + radius, j + radius];
            }
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] /= norm;
            }
        }
    }
}

class EmbossingFilter : MatrixFilters
{
    public EmbossingFilter()
    {
        int size = 3;
        int[,] yadro = { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = yadro[i, j];
            }
        }
    }

    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int radiusX = kernel.GetLength(0) / 2;
        int radiusY = kernel.GetLength(1) / 2;
        float resultR = 0;
        float resultG = 0;
        float resultB = 0;
        for (int l = -radiusY; l <= radiusY; l++)
        {
            for (int k = -radiusX; k <= radiusY; k++)
            {
                int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                Color neighborColor = sourceImage.GetPixel(idX, idY);
                resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

            }
        }
        return Color.FromArgb(
            Clamp((int)resultR + 70, 0, 255),
            Clamp((int)resultG + 70, 0, 255),
            Clamp((int)resultB + 70, 0, 255)
            );
    }
}


class MotionBlurFilter : MatrixFilters
{
    public MotionBlurFilter()
    {
        int size = 3;
        kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == j) kernel[i, j] = (float)1 / 3;
                else kernel[i, j] = 0;

            }
        }
    }
}


class SharpnessFilter : MatrixFilters
{
    public SharpnessFilter()
    {
        int size = 3;
        int[,] mat = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = mat[i, j];
            }
        }
    }
}

class SharpnessFilter2 : MatrixFilters
{
    public SharpnessFilter2()
    {
        int size = 3;
        int[,] mat = { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
        kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = mat[i, j];
            }
        }
    }
}

class BorderFilter : MatrixFilters
{
    public BorderFilter()
    {
        int size = 3;
        int[,] OY = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
        int[,] OX = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
        kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = OX[i, j] + OY[i, j];
            }
        }
    }
}

class TurnFilter : Filters
{
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int x0 = sourceImage.Width - 1;
        int y0 = sourceImage.Height - 1;
        double u = 45 * Math.PI / 180;
        Color sourceColor = sourceImage.GetPixel(Clamp((int)((x - x0) * Math.Cos(u) - (y - y0) * Math.Sin(u) + x0), 0, sourceImage.Width - 1), Clamp((int)((x - x0) * Math.Sin(u) + (y - y0) * Math.Cos(u) + y0), 0, sourceImage.Width - 1));
        Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        return resultColor;
    }
}

class LightBorderFilter : MatrixFilters
{
    public LightBorderFilter()
    {
        int size = 3;
        kernel = new float[size, size];
        int[,] OY = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
        int[,] OX = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = OX[i, j] + OY[i, j];
            }
        }
    }
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int radiusX = kernel.GetLength(0) / 2;
        int radiusY = kernel.GetLength(1) / 2;
        float resultR = 0;
        float resultG = 0;
        float resultB = 0;
        for (int l = -radiusY; l <= radiusY; l++)
        {
            for (int k = -radiusX; k <= radiusY; k++)
            {
                int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                Color neighborColor = sourceImage.GetPixel(idX, idY);
                resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                //resultR = Math.Max(resultR, neighborColor.R * kernel[k + radiusX, l + radiusY]);
                resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                //resultG = Math.Max(resultR, neighborColor.G * kernel[k + radiusX, l + radiusY]);
                resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                //resultB = Math.Max(resultR, neighborColor.B * kernel[k + radiusX, l + radiusY]);

            }
        }
        return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );

    }
}

class DilationFilter : MatrixFilters
{
    public DilationFilter()
    {
        kernel = new float[,]
    {
         { 1.0f, 1.0f, 1.0f},
         { 1.0f, 1.0f, 1.0f},
         { 1.0f, 1.0f, 1.0f},
         {1.0f, 1.0f, 1.0f},
         {1.0f, 1.0f, 1.0f}
    };
    }
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int radiusX = kernel.GetLength(0) / 2;
        int radiusY = kernel.GetLength(1) / 2;
        Color maxColor = Color.Black;
        Color color = sourceImage.GetPixel(x, y);
        for (int l = -radiusY; l <= radiusY; l++)
        {
            for (int k = -radiusX; k <= radiusX; k++)
            {
                int newX = x + k;
                int newY = y + l;
                if (newX >= 0 && newX < sourceImage.Width && newY >= 0 && newY < sourceImage.Height)
                {
                    Color neighborColor = sourceImage.GetPixel(newX, newY);
                    if (neighborColor.GetBrightness() > maxColor.GetBrightness())
                    {
                        maxColor = neighborColor;
                    }
                }
            }
        }
        return maxColor;
    }
}

class ErosionFilter : MatrixFilters
{
    public ErosionFilter()
    {
        kernel = new float[,]
    {
         { 1.0f, 1.0f, 1.0f},
         { 1.0f, 1.0f, 1.0f},
         { 1.0f, 1.0f, 1.0f },
         {1.0f, 1.0f, 1.0f},
         {1.0f, 1.0f, 1.0f}
    };
    }
    public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int radiusX = kernel.GetLength(0) / 2;
        int radiusY = kernel.GetLength(1) / 2;
        Color minColor = Color.White;
        for (int l = -radiusY; l <= radiusY; l++)
        {
            for (int k = -radiusX; k <= radiusX; k++)
            {
                int newX = x + k;
                int newY = y + l;
                if (newX >= 0 && newX < sourceImage.Width && newY >= 0 && newY < sourceImage.Height)
                {
                    Color neighborCol = sourceImage.GetPixel(newX, newY);
                    if (neighborCol.GetBrightness() < minColor.GetBrightness())
                    {
                        minColor = neighborCol;
                    }
                }
            }
        }
        return minColor;
    }

}





