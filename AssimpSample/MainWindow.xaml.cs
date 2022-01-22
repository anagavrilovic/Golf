using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    public partial class MainWindow : Window
    {
        #region Atributi

        World m_world = null;

        public enum BallSize { XS = 0, S, M, L, XL };
        public enum LightDiffuse { WHITE = 0, RED, YELLOW, GREEN, BLUE };

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                var currentPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString();
                m_world = new World(Path.Combine(currentPath, "3D Models\\GolfClub\\source"), "Golfclub.dae", 
                                    (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(!m_world.AnimationActive)
            {
                switch (e.Key)
                {
                    case Key.F2: this.Close(); break;

                    case Key.E: 
                        if (m_world.RotationX > 0) m_world.RotationX -= 5.0f;
                        break;

                    case Key.D: 
                        if (m_world.RotationX < 90) m_world.RotationX += 5.0f; 
                        break;

                    case Key.S: m_world.RotationY -= 5.0f; break;
                    case Key.F: m_world.RotationY += 5.0f; break;
                    case Key.V: m_world.StartAnimation(); break;
                    case Key.Add: m_world.SceneDistance -= 5.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 5.0f; break;

                    case Key.J:
                        if (m_world.Hole_x > -32) m_world.Hole_x -= 2;
                        break;

                    case Key.L:
                        if (m_world.Hole_x < 32) m_world.Hole_x += 2;
                        break;

                    case Key.I:
                        if (m_world.Hole_z > -32) m_world.Hole_z -= 2;
                        break;

                    case Key.K:
                        if (m_world.Hole_z < 32) m_world.Hole_z += 2;
                        break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cb1.ItemsSource = Enum.GetValues(typeof(BallSize));
            cb2.ItemsSource = Enum.GetValues(typeof(LightDiffuse));
            cb1.SelectedIndex = 2;
            cb2.SelectedIndex = 0;
        }

        private void cb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(cb1.SelectedIndex)
            {
                case 0: m_world.Ball_scale = 0.70f; break;
                case 1: m_world.Ball_scale = 0.85f; break;
                case 2: m_world.Ball_scale = 1f; break;
                case 3: m_world.Ball_scale = 1.15f; break;
                case 4: m_world.Ball_scale = 1.3f; break;
            }
        }

        private void cb2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cb2.SelectedIndex)
            {
                case 0: m_world.DiffuseColor = System.Drawing.Color.White; break;
                case 1: m_world.DiffuseColor = System.Drawing.Color.Red; break;
                case 2: m_world.DiffuseColor = System.Drawing.Color.Yellow; break;
                case 3: m_world.DiffuseColor = System.Drawing.Color.Green; break;
                case 4: m_world.DiffuseColor = System.Drawing.Color.Blue; break;
            }
        }
    }
}
