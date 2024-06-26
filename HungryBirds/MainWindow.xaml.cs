﻿using Microsoft.Win32;
using System;
using System.IO;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace HungryBirds
{
    public partial class MainWindow : Window
    {
        List<Obstacle> obstacles = new List<Obstacle>(); //список, где хранятся препятствия
        //private string currentFilePath = "pack://application:,,,/images/input.txt";
        private string currentFilePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MakeObstacle() //создаем препятствия
        {
            Random rand = new Random();
            Obstacle obstacle = new Obstacle(rand.Next(300, 700), rand.Next(0, 390));
            hungrybirdcanvas.Children.Add(obstacle.Rectangle);
            obstacles.Add(obstacle);
        }

        private void ClickOnCanvas(object sender, MouseButtonEventArgs e)
        {
            MakeObstacle();
        } //триггер для функии

        private void Button_Click(object sender, RoutedEventArgs e) // ввод через файл как в главе 26
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;

                try
                {
                    var lines = File.ReadLines(currentFilePath).ToList();

                    if (lines.Count >= 2)
                    {
                        string firstangle = lines[0];
                        string seconveloc = lines[1];
                        textbox1.Text = firstangle;
                        textbox2.Text = seconveloc;
                    }
                    else
                    {
                        MessageBox.Show("Not enought data to operate with.");
                    }
                }
                catch
                {
                    MessageBox.Show("An unknown error occurred");
                }
            }
        }
                private void Button_Click_1(object sender, RoutedEventArgs e) // реализация кнопки feed
            {
            try
            {
                double angle = Convert.ToDouble(getAngle());
                double veloc = Convert.ToDouble(getVeloc());
                Fly(angle, veloc);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show("Invalid input.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred.");
            }
        }

        string getAngle() { return textbox1.Text; } //чтобы вытащить строки из текстбоксов

        string getVeloc() { return textbox2.Text; } //чтобы вытащить строки из текстбоксов

        private void Fly(double angle, double veloc) //реализация полета и столкновения
        {
            Food food = new Food(); 
            hungrybirdcanvas.Children.Add(food.Rectangle); //создаем объект

            var timer = new DispatcherTimer(); //описываем траеторию
            timer.Interval = TimeSpan.FromMilliseconds(10); //таймер, чтобы обновлять позицию
            const double g = 9.81;
            const double k = 0.3;
            int n = 0;
            double totalTime = (2 * veloc * Math.Sin(angle * Math.PI / 180)) / g;
            double totalTimeSteps = totalTime / timer.Interval.TotalSeconds;
            double velX1 = veloc * Math.Cos(angle * Math.PI / 180);
            double velY1 = veloc * Math.Sin(angle * Math.PI / 180);

            timer.Tick += (sender, e) =>
            {
                double timeStep = totalTime / totalTimeSteps;
                double moveX = velX1 * timeStep;
                double moveY = velY1 * timeStep;
                velY1 -= timeStep * (g + k * velY1);
                velX1 -= timeStep * k * velX1;

                int collisionCount = 0;
                // проверяем на предмет коллизии: максимальное расстояние между центрами соприкасающихся прямоугольников = 95.6
                for (int i = 0; i < obstacles.Count; i++)
                {
                    Obstacle obstacle = obstacles[i];
                    double ObscenterX = Canvas.GetLeft(obstacle.Rectangle) + (obstacle.Rectangle.ActualWidth / 2);
                    double ObscenterY = Canvas.GetTop(obstacle.Rectangle) + (obstacle.Rectangle.ActualHeight / 2);
                    double FoodcenterX = Canvas.GetLeft(food.Rectangle) + (food.Rectangle.ActualWidth / 2);
                    double FoodcenterY = Canvas.GetTop(food.Rectangle) + (food.Rectangle.ActualHeight / 2);
                    double length = Math.Sqrt(Math.Pow((ObscenterX - FoodcenterX), 2) + Math.Pow((ObscenterY - FoodcenterY), 2));
                    if (length < 95.6)
                    {
                        // Удаляем при столкновении
                        hungrybirdcanvas.Children.Remove(food.Rectangle);
                        hungrybirdcanvas.Children.Remove(obstacle.Rectangle);
                        obstacles.RemoveAt(i); // удаляем из списка

                        collisionCount++;
                        scoretext.Content = "Score: " + collisionCount;

                        timer.Stop();
                        return;
                    }
                }
                double currentX = Canvas.GetLeft(food.Rectangle);
                double currentY = Canvas.GetTop(food.Rectangle);

                if (currentY >= 540 || currentY <= 0 || currentX >= 850) //если ушёл за рамки окна - удаляем
                {
                    hungrybirdcanvas.Children.Remove(food.Rectangle);
                    timer.Stop();
                    return;
                }
                //двигаем, если все хорошо
                Canvas.SetLeft(food.Rectangle, currentX + moveX);
                Canvas.SetTop(food.Rectangle, currentY - moveY);
            };
            timer.Start();
        }

        public class Obstacle
        {
            public Rectangle Rectangle { get; private set; }

            public Obstacle(double left, double top)
            {
                Rectangle = new Rectangle
                {
                    Height = 90,
                    Width = 80,
                    Fill = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/bird.png"))
                    }
                };

                Canvas.SetLeft(Rectangle, left);
                Canvas.SetTop(Rectangle, top);
            }
        } // класс препятствия
        public class Food
        {
            public Rectangle Rectangle { get; private set; }
            public Food(double left = 0, double top = 500)
            {
                Rectangle = new Rectangle
                {
                Width = 50,
                Height = 50,
                Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/onigiri-removebg-preview.png"))
                }
            };
                Canvas.SetLeft(Rectangle, 0);
                Canvas.SetTop(Rectangle, 500);
            }
        } //класс онигири
    }
}
