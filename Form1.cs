using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace ImageColorProcessor
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> sourceImage;
        private Image<Bgr, byte> resultImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox2.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox1.BorderStyle = BorderStyle.FixedSingle;
            imageBox2.BorderStyle = BorderStyle.FixedSingle;

            // Заполнение comboBox1 вариантами для изменения HSV
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Hue");
            comboBox1.Items.Add("Saturation");
            comboBox1.Items.Add("Value");

            // Можно задать значение по умолчанию
            comboBox1.SelectedIndex = 0;

            // 🔥 НОВЫЙ КОД: Настройка TrackBar
            // TrackBar для яркости (предположим trackBar1)
            trackBar1.Minimum = -100;
            trackBar1.Maximum = 100;
            trackBar1.Value = 0;
            trackBar1.TickFrequency = 10;

            // TrackBar для контраста (предположим trackBar2)
            trackBar2.Minimum = 1;
            trackBar2.Maximum = 30;
            trackBar2.Value = 10;  // Контраст 1.0
            trackBar2.TickFrequency = 5;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            try
            {
                // Тест инициализации Emgu CV
                string version = Emgu.CV.CvInvoke.BuildInformation;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    sourceImage = new Image<Bgr, byte>(fileName);
                    imageBox1.Image = sourceImage.Resize(640, 480, Inter.Linear);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}\n\nПроверьте установку Emgu CV runtime.", "Ошибка");
            }
        }

        private void btnChannelR_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var channels = sourceImage.Split();
                var redChannel = channels[2]; // Красный канал

                resultImage = redChannel.Convert<Bgr, byte>();
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);

                foreach (var channel in channels)
                    channel.Dispose();
            }
        }

        private void btnChannelG_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var channels = sourceImage.Split();
                var grayImage = channels[1]; // Зеленый канал

                resultImage = grayImage.Convert<Bgr, byte>();
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);

                foreach (var channel in channels)
                    channel.Dispose();
            }
        }

        private void btnChannelB_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var channels = sourceImage.Split();
                var blueChannel = channels[0]; // Синий канал - канал с индексом 0

                resultImage = blueChannel.Convert<Bgr, byte>();
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);

                foreach (var channel in channels)
                    channel.Dispose();
            }
        }

        private void btnGrayscale_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var grayImage = new Image<Gray, byte>(sourceImage.Size);

                for (int y = 0; y < sourceImage.Height; y++)
                {
                    for (int x = 0; x < sourceImage.Width; x++)
                    {
                        // Формула конвертации RGB в оттенки серого
                        byte gray = (byte)(0.299 * sourceImage.Data[y, x, 2] +
                                          0.587 * sourceImage.Data[y, x, 1] +
                                          0.114 * sourceImage.Data[y, x, 0]);
                        grayImage.Data[y, x, 0] = gray;
                    }
                }

                resultImage = grayImage.Convert<Bgr, byte>();
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
                grayImage.Dispose();
            }
        }

        private void btnSepia_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var sepiaImage = new Image<Bgr, byte>(sourceImage.Size);

                for (int y = 0; y < sourceImage.Height; y++)
                {
                    for (int x = 0; x < sourceImage.Width; x++)
                    {
                        byte blue = sourceImage.Data[y, x, 0];
                        byte green = sourceImage.Data[y, x, 1];
                        byte red = sourceImage.Data[y, x, 2];

                        // Формулы Sepia
                        int newRed = (int)(red * 0.393 + green * 0.769 + blue * 0.189);
                        int newGreen = (int)(red * 0.349 + green * 0.686 + blue * 0.168);
                        int newBlue = (int)(red * 0.272 + green * 0.534 + blue * 0.131);

                        // Ограничение значений
                        sepiaImage.Data[y, x, 2] = (byte)Math.Min(255, newRed);
                        sepiaImage.Data[y, x, 1] = (byte)Math.Min(255, newGreen);
                        sepiaImage.Data[y, x, 0] = (byte)Math.Min(255, newBlue);
                    }
                }

                resultImage = sepiaImage;
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
            }
        }

        private void ApplyBrightnessContrast(int brightness, double contrast)
        {
            if (sourceImage != null)
            {
                var adjustedImage = new Image<Bgr, byte>(sourceImage.Size);

                for (int y = 0; y < sourceImage.Height; y++)
                {
                    for (int x = 0; x < sourceImage.Width; x++)
                    {
                        for (int channel = 0; channel < 3; channel++)
                        {
                            double color = sourceImage.Data[y, x, channel];

                            // Применяем формулу: color = color * contrast + brightness
                            color = color * contrast + brightness;

                            // Ограничение значений от 0 до 255 (более эффективно)
                            color = Math.Max(0, Math.Min(255, color));

                            adjustedImage.Data[y, x, channel] = (byte)color;
                        }
                    }
                }

                // Освобождаем предыдущий результат
                resultImage?.Dispose();
                resultImage = adjustedImage;
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
            }
        }

        private void TrackBar_Scroll(object sender, EventArgs e)
        {
            // Получаем значения из обоих TrackBar
            int brightness = trackBar1.Value;           // Яркость от -100 до 100
            double contrast = trackBar2.Value / 10.0;   // Контраст от 0.1 до 3.0

            // Применяем изменения к изображению
            ApplyBrightnessContrast(brightness, contrast);
        }

        private void btnHSV_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                // ДОБАВИТЬ ПРОВЕРКУ:
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите параметр HSV для изменения.");
                    return;
                }

                var hsvImage = sourceImage.Convert<Hsv, byte>();

                // Парсим числовое значение из textBox1
                int shiftValue = 0;
                if (!int.TryParse(textBox1.Text, out shiftValue))
                {
                    MessageBox.Show("Введите корректное числовое значение для сдвига.");
                    return;
                }

                string selectedMode = comboBox1.SelectedItem.ToString();

                for (int y = 0; y < hsvImage.Height; y++)
                {
                    for (int x = 0; x < hsvImage.Width; x++)
                    {
                        int h = hsvImage.Data[y, x, 0];
                        int s = hsvImage.Data[y, x, 1];
                        int v = hsvImage.Data[y, x, 2];

                        switch (selectedMode)
                        {
                            case "Hue":
                                h = (h + shiftValue) % 180;
                                if (h < 0) h += 180;
                                break;

                            case "Saturation":
                                s = Math.Min(255, Math.Max(0, s + shiftValue));
                                break;

                            case "Value":
                                v = Math.Min(255, Math.Max(0, v + shiftValue));
                                break;
                        }

                        hsvImage.Data[y, x, 0] = (byte)h;
                        hsvImage.Data[y, x, 1] = (byte)s;
                        hsvImage.Data[y, x, 2] = (byte)v;
                    }
                }

                resultImage = hsvImage.Convert<Bgr, byte>();
                imageBox2.Image = resultImage.Resize(640, 480, Emgu.CV.CvEnum.Inter.Linear);

                hsvImage.Dispose();
            }
        }

        private void btnMedianBlur_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var blurredImage = sourceImage.Clone(); // Копируем исходное для границ
                int windowSize = 3;
                int offset = windowSize / 2;

                for (int y = offset; y < sourceImage.Height - offset; y++)
                {
                    for (int x = offset; x < sourceImage.Width - offset; x++)
                    {
                        for (int channel = 0; channel < 3; channel++)
                        {
                            List<byte> window = new List<byte>();

                            // Собираем значения пикселей в окне
                            for (int dy = -offset; dy <= offset; dy++)
                            {
                                for (int dx = -offset; dx <= offset; dx++)
                                {
                                    window.Add(sourceImage.Data[y + dy, x + dx, channel]);
                                }
                            }

                            // Сортируем и берем медиану
                            window.Sort();
                            blurredImage.Data[y, x, channel] = window[window.Count / 2];
                        }
                    }
                }

                resultImage = blurredImage;
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
            }
        }

        private void btnSharpen_Click(object sender, EventArgs e)
        {
            if (sourceImage != null)
            {
                var sharpenedImage = new Image<Bgr, byte>(sourceImage.Size);

                // Матрица повышения резкости
                int[,] kernel = {
            {-1, -1, -1},
            {-1,  9, -1},
            {-1, -1, -1}
        };

                int offset = 1;

                for (int y = offset; y < sourceImage.Height - offset; y++)
                {
                    for (int x = offset; x < sourceImage.Width - offset; x++)
                    {
                        for (int channel = 0; channel < 3; channel++)
                        {
                            int sum = 0;

                            // Применяем ядро свертки
                            for (int dy = -offset; dy <= offset; dy++)
                            {
                                for (int dx = -offset; dx <= offset; dx++)
                                {
                                    sum += sourceImage.Data[y + dy, x + dx, channel] *
                                           kernel[dy + offset, dx + offset];
                                }
                            }

                            // Нормализация (сумма элементов матрицы = 1)
                            sum = Math.Max(0, Math.Min(255, sum));
                            sharpenedImage.Data[y, x, channel] = (byte)sum;
                        }
                    }
                }

                resultImage = sharpenedImage;
                imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
            }
        }

        //private void btnImageAddition_Click(object sender, EventArgs e)
        //{
        //    if (sourceImage != null && secondImage != null)
        //    {
        //        var resultImg = new Image<Bgr, byte>(sourceImage.Size);

        //        for (int y = 0; y < sourceImage.Height; y++)
        //        {
        //            for (int x = 0; x < sourceImage.Width; x++)
        //            {
        //                for (int channel = 0; channel < 3; channel++)
        //                {
        //                    int sum = sourceImage.Data[y, x, channel] +
        //                             secondImage.Data[y, x, channel];
        //                    resultImg.Data[y, x, channel] = (byte)Math.Min(255, sum);
        //                }
        //            }
        //        }

        //        resultImage = resultImg;
        //        imageBox2.Image = resultImage.Resize(640, 480, Inter.Linear);
        //    }
        //}

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем вводить цифры
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false; // символ разрешен
            }
            // Разрешаем ввод знака минуса, но только если он первый символ и еще не введен
            else if (e.KeyChar == '-' && (sender as TextBox).Text.Length == 0)
            {
                e.Handled = false;
            }
            // Разрешаем клавишу Backspace (для удаления символов)
            else if (e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true; // остальные символы запрещены
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Освобождение ресурсов
            sourceImage?.Dispose();
            resultImage?.Dispose();
        }

        private void btnSaveResult_Click(object sender, EventArgs e)
        {
            if (resultImage != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Files|*.png|JPEG Files|*.jpg|BMP Files|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    resultImage.Save(saveFileDialog.FileName);
                }
            }
        }
    }
}
