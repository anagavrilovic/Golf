// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;

namespace AssimpSample
{
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    public class World : IDisposable
    {
        #region Atributi

        ///	 Scena koja se prikazuje.
        private AssimpScene m_golf_club;

        ///	 Ugao rotacije sveta oko X ose.
        private float m_xRotation = 0.0f;

        ///	 Ugao rotacije sveta oko Y ose.
        private float m_yRotation = -35f;

        ///	 Udaljenost scene od kamere.
        private float m_sceneDistance = 80.0f;

        /// Transliranje po X osi preko tastature
        private float x_translate = 0.0f;

        /// Transliranje po Y osi preko tastature
        private float y_translate = 0.0f;

        ///	 Sirina OpenGL kontrole u pikselima.
        private int m_width;

        ///	 Visina OpenGL kontrole u pikselima.
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_golf_club; }
            set { m_golf_club = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float X_translate
        {
            get { return x_translate; }
            set { x_translate = value; }
        }

        public float Y_translate
        {
            get { return y_translate; }
            set { y_translate = value; }
        }

        #endregion Properties

        #region Konstruktori

        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_golf_club = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.65f, 0.92f, 0.98f, 1.0f);

            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);

            // 1. Testiranje dubine i sakrivanje nevidljivih povrsina
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GL_CULL_FACE);

            // 3D modeli
            m_golf_club.LoadScene();
            m_golf_club.Initialize();
        }

        ///  Iscrtavanje OpenGL kontrole.
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            float ground_size = 35f;

            gl.PushMatrix();
            gl.Translate(0f, 0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

                #region Ground
                gl.PushMatrix();
                gl.Translate(0f, -10f, 0f);

                    #region Hole
                    gl.PushMatrix();
                    gl.Translate(20f, 0.01f, -20f);
                    gl.Rotate(-90f, 1f, 0f, 0f);
                    gl.Color(0.19f, 0.15f, 0.11f);
                    Disk disk = new Disk();
                    disk.Slices = 40;
                    disk.InnerRadius = 0f;
                    disk.OuterRadius = 2.5f;
                    disk.CreateInContext(gl);
                    disk.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

                    gl.Translate(0f, 0f, 0.1f);
                    gl.Color(1f, 1f, 1f);
                    disk.Slices = 40;
                    disk.InnerRadius = 0f;
                    disk.OuterRadius = 1.8f;
                    disk.CreateInContext(gl);
                    disk.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.PopMatrix();
                    #endregion

                #region Grass
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(0.19f, 0.85f, 0.26f);
                gl.Vertex(ground_size, 0f, ground_size);
                gl.Vertex(ground_size, 0f, -ground_size);
                gl.Vertex(-ground_size, 0f, -ground_size);
                gl.Vertex(-ground_size, 0f, ground_size);

                gl.Color(0.19f, 0.15f, 0.11f);
                gl.Vertex(ground_size, 0f, ground_size);
                gl.Vertex(ground_size, -3f, ground_size);
                gl.Vertex(ground_size, -3f, -ground_size);
                gl.Vertex(ground_size, 0f, -ground_size);

                gl.Vertex(ground_size, 0f, ground_size);
                gl.Vertex(-ground_size, 0f, ground_size);
                gl.Vertex(-ground_size, -3f, ground_size);
                gl.Vertex(ground_size, -3f, ground_size);

                gl.Vertex(-ground_size, 0f, ground_size);
                gl.Vertex(-ground_size, 0f, -ground_size);
                gl.Vertex(-ground_size, -3f, -ground_size);
                gl.Vertex(-ground_size, -3f, ground_size);

                gl.Vertex(-ground_size, 0f, -ground_size);
                gl.Vertex(ground_size, 0f, -ground_size);
                gl.Vertex(ground_size, -3f, -ground_size);
                gl.Vertex(-ground_size, -3f, -ground_size);

                gl.Vertex(ground_size, -3f, ground_size);
                gl.Vertex(-ground_size, -3f, ground_size);
                gl.Vertex(-ground_size, -3f, -ground_size);
                gl.Vertex(ground_size, -3f, -ground_size);
                gl.End();
                #endregion

                    #region Flag Stick
                    gl.Disable(OpenGL.GL_CULL_FACE);
                    gl.PushMatrix();
                    gl.Translate(20f, 0.01f, -20f);
                    gl.Rotate(-90f, 1f, 0f, 0f);
                    gl.Color(1f, 1f, 1f);
                    Cylinder stick = new Cylinder();
                    stick.Slices = 20;
                    stick.BaseRadius = 0.3f;
                    stick.TopRadius = 0.3f;
                    stick.Height = 40f;
                    stick.CreateInContext(gl);
                    stick.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.PopMatrix();
                    gl.Enable(OpenGL.GL_CULL_FACE);
                    #endregion

                    #region Flag
                    gl.PushMatrix();
                    gl.Translate(20.2f, 38f, -20.2f);
                    gl.Rotate(-90f, 0f, 0f, 1f);
                    gl.Color(0.79f, 0.11f, 0.11f);
                    gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                    gl.Vertex(2.5f, 10f, 0.2f);
                    gl.Vertex(0f, 0f, 0.4f);
                    gl.Vertex(5f, 0f, 0.4f);
                    gl.Vertex(5f, 0f, 0f);
                    gl.Vertex(0f, 0f, 0f);
                    gl.Vertex(0f, 0f, 0.4f);
                    gl.End();
                    gl.PopMatrix();
                    #endregion

                    #region Golf Tee
                    gl.PushMatrix();
                    gl.Translate(-15f, -1.2f, 15f);
                    gl.Rotate(-90f, 1f, 0f, 0f);
                    gl.Color(0.6f, 0.52f, 0.39f);
                    Cylinder tee = new Cylinder();
                    tee.Slices = 20;
                    tee.BaseRadius = 0f;
                    tee.TopRadius = 0.3f;
                    tee.Height = 3f;
                    tee.CreateInContext(gl);
                    tee.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.PopMatrix();
                    #endregion

                    #region Golf Ball
                    gl.PushMatrix();
                    gl.Translate(-15f, 2.7f, 15f);
                    gl.Color(1f, 1f, 1f);
                    Sphere ball = new Sphere();
                    ball.CreateInContext(gl);
                    ball.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.PopMatrix();
                    #endregion

                gl.PopMatrix();
                #endregion

                #region Golf Club
                gl.PushMatrix();
                gl.Translate(-15f, 4.5f, 20f);
                gl.Rotate(30f, 0f, 1f, 0f);
                m_golf_club.Draw();
                gl.PopMatrix();
                #endregion
           
            gl.PopMatrix();

            #region Text
            gl.FrontFace(OpenGL.GL_CW);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.PushMatrix();
            gl.Color(1f, 0f, 0f);
            gl.Translate(-37f, 19.5f, -50);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.03f, "Predmet: Racunarska grafika");

            gl.Translate(-10.6f, -1.3f, 0f);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.03f, "Sk. god: 2021/22.");

            gl.Translate(-6.6f, -1.3f, 0f);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.03f, "Ime: Ana");

            gl.Translate(-3.5f, -1.3f, 0f);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.03f, "Prezime: Gavrilovic");

            gl.Translate(-6.9f, -1.3f, 0f);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.03f, "Sifra zad: 15.2");
            gl.PopMatrix();
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.FrontFace(OpenGL.GL_CCW);
            #endregion

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        /// Podesava viewport i projekciju za OpenGL kontrolu.
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            // Definisanje projekcije i Viewporta
            gl.Viewport(0, 0, width, height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);                            // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 2000f);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                                              // resetuj ModelView Matrix
        }

        ///  Implementacija IDisposable interfejsa.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_golf_club.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
