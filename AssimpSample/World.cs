// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using SharpGL.SceneGraph.Quadrics;
using SharpGL;
using SharpGL.SceneGraph.Cameras;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL.SceneGraph;

namespace AssimpSample
{
    // Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    public class World : IDisposable
    {
        #region Atributi

        // Atribut kamere
        private LookAtCamera lookAtCam;

        // Atributi za animaciju
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private float golfClubAngle = 0f;
        private bool golfClubGoingUp = true;
        private float ballPosition_x = -20.0f;
        private float ballPosition_z = 20.0f;
        private float ballPosition_y = 0f;
        private bool animationActive = false;

        // Teksture
        private enum TextureObjects { Grass = 0, YellowPlastic, GolfBall };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//images//grass1.jpg", "..//..//images//plastic.jpg", "..//..//images//golfBall.jpg" };

        // Scena koja se prikazuje.
        private AssimpScene m_golf_club;

        // Ugao rotacije sveta oko X ose.
        private float m_xRotation = 0f;

        // Ugao rotacije sveta oko Y ose.
        private float m_yRotation = 0f;

        // Udaljenost scene od kamere.
        private float m_sceneDistance = 80f;

        // Transliranje po X osi preko tastature
        private float x_translate = 0.0f;

        // Transliranje po Y osi preko tastature
        private float y_translate = 0.0f;

        // Sirina OpenGL kontrole u pikselima.
        private int m_width;

        // Visina OpenGL kontrole u pikselima.
        private int m_height;

        private float ball_scale = 1.0f;

        private Color diffuseColor = Color.White;

        private float hole_x = 20f;

        private float hole_z = -5f;

        #endregion Atributi

        #region Properties

        public LookAtCamera LookAtCam
        {
            get { return lookAtCam; }
            set { lookAtCam = value; }
        }

        public float Hole_z
        {
            get { return hole_z; }
            set { hole_z = value; }
        }

        public float Hole_x
        {
            get { return hole_x; }
            set { hole_x = value; }
        }

        public float GolfClubAngle
        {
            get { return golfClubAngle; }
            set { golfClubAngle = value; }
        }

        public float BallPosition_x
        {
            get { return ballPosition_x; }
            set { ballPosition_x = value; }
        }

        public float BallPosition_z
        {
            get { return ballPosition_z; }
            set { ballPosition_z = value; }
        }

        public float BallPosition_y
        {
            get { return ballPosition_y; }
            set { ballPosition_y = value; }
        }

        public bool AnimationActive
        {
            get { return animationActive; }
            set { animationActive = value; }
        }

        public AssimpScene Scene
        {
            get { return m_golf_club; }
            set { m_golf_club = value; }
        }

        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

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

        public float Ball_scale
        {
            get { return ball_scale; }
            set { ball_scale = value; }
        }

        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        #endregion Properties

        #region Konstruktori

        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_golf_club = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        // Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        public void Initialize(OpenGL gl)
        {
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);

            // 1.1 Testiranje dubine i sakrivanje nevidljivih povrsina
            DepthTestingAndFaceCulling(gl);

            // Osvetljenje
            SetupLighting(gl);

            // 2.1 Color Tracking mehanizam
            ColorTrackingMechanism(gl);

            // 3D modeli
            Initialize3DModels();

            // Podesavanje inicijalnih parametara kamere
            /*lookAtCam = new LookAtCamera();
            lookAtCam.Position = new Vertex(0f, 0f, 0f);
            lookAtCam.Target = new Vertex(0f, 0f, 10f);
            lookAtCam.UpVector = new Vertex(0f, 1f, 0f);
            lookAtCam.Project(gl);*/

            // Definisanje tajmera za animaciju
            CreateTimers();

            // Teksture
            CreateTextures(gl);
        }

        private static void DepthTestingAndFaceCulling(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GL_CULL_FACE);
        }

        private void SetupLighting(OpenGL gl)
        {
            float[] global_ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);

            float[] light0pos = new float[] { 0.0f, 10.0f, -10.0f, 1.0f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            // Ukljuci automatsku normalizaciju nad normalama
            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        private static void ColorTrackingMechanism(OpenGL gl)
        {
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            gl.ClearColor(0.65f, 0.92f, 0.98f, 1.0f);
        }

        private void Initialize3DModels()
        {
            m_golf_club.LoadScene();
            m_golf_club.Initialize();
        }

        private void CreateTimers()
        {
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(10);
            timer1.Tick += new EventHandler(RunGolfClubAnimation);

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(10);
            timer2.Tick += new EventHandler(RunBallAnimation);
        }

        private void CreateTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_REPLACE);

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);		// Wrapping - GL_REPEAT - S
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);		// Wrapping - GL_REPEAT - T
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);		// Nearest Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);		// Nearest Filtering

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        // Iscrtavanje OpenGL kontrole.
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //lookAtCam.Project(gl);

            float ground_size = 35f;

            gl.PushMatrix();
            gl.Translate(0f, 0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

                #region Ground
                gl.PushMatrix();
                gl.Translate(0f, -10f, 0f);

                #region Grass
                gl.MatrixMode(OpenGL.GL_TEXTURE);

                    gl.PushMatrix();
                    gl.Scale(1.1f, 1.1f, 1.1f);
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Color(0.19f, 0.85f, 0.26f);
                    gl.Normal(0.0f, 1.0f, 0.0f);
                    gl.TexCoord(0.0f, 0.0f);
                    gl.Vertex(ground_size, 0f, ground_size);
                    gl.TexCoord(0.0f, 5.0f);
                    gl.Vertex(ground_size, 0f, -ground_size);
                    gl.TexCoord(5.0f, 5.0f);
                    gl.Vertex(-ground_size, 0f, -ground_size);
                    gl.TexCoord(5.0f, 0.0f);
                    gl.Vertex(-ground_size, 0f, ground_size);
                    gl.End();
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
                    gl.PopMatrix();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);

                gl.Begin(OpenGL.GL_QUADS);
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

                    #region Hole
                    gl.PushMatrix();
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
                    gl.Translate(hole_x, 0.01f, hole_z);
                    gl.Rotate(-90f, 1f, 0f, 0f);
                    gl.Color(0.19f, 0.15f, 0.11f);
                    Disk disk = new Disk();
                    disk.NormalGeneration = Normals.Smooth;
                    disk.Slices = 40;
                    disk.InnerRadius = 0f;
                    disk.OuterRadius = 2.5f;
                    disk.CreateInContext(gl);
                    disk.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

                        #region Flag Stick
                        gl.Disable(OpenGL.GL_CULL_FACE);
                        gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.YellowPlastic]);
                        gl.PushMatrix();
                        gl.Color(1f, 1f, 1f);
                        Cylinder stick = new Cylinder();
                        stick.NormalGeneration = Normals.Smooth;
                        stick.Slices = 20;
                        stick.BaseRadius = 0.3f;
                        stick.TopRadius = 0.3f;
                        stick.Height = 40f;
                        stick.CreateInContext(gl);
                        stick.TextureCoords = true;
                        stick.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                        gl.PopMatrix();
                        gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        #endregion

                        #region Flag
                        gl.PushMatrix();
                        gl.Translate(0.2f, 0.2f, 38f);
                        gl.Rotate(90f, 0f, 1f, 0f);
                        gl.Rotate(90f, 1f, 0f, 0f);
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

                    gl.PopMatrix();
                    #endregion

                    #region Golf Tee
                    gl.PushMatrix();
                    gl.Disable(OpenGL.GL_CULL_FACE);
                    gl.Translate(-20f, -1.8f, 20f);
                    gl.Rotate(-90f, 1f, 0f, 0f);
                    gl.Color(0.6f, 0.52f, 0.39f);
                    Cylinder tee = new Cylinder();
                    tee.NormalGeneration = Normals.Smooth;
                    tee.Slices = 20;
                    tee.BaseRadius = 0f;
                    tee.TopRadius = 0.3f;
                    tee.Height = 3f;
                    tee.CreateInContext(gl);
                    tee.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.Enable(OpenGL.GL_CULL_FACE);
                    gl.PopMatrix();
                    #endregion

                    #region Golf Ball
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.GolfBall]);
                    gl.PushMatrix();
                    gl.Translate(ballPosition_x, 2f + ballPosition_y, ballPosition_z);
                    gl.Scale(ball_scale, ball_scale, ball_scale);
                    gl.Color(1f, 1f, 1f);
                    Sphere ball = new Sphere();
                    ball.NormalGeneration = Normals.Smooth;
                    ball.Slices = 40;
                    ball.Stacks = 40;
                    ball.CreateInContext(gl);
                    ball.TextureCoords = true;
                    ball.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
                    gl.PopMatrix();
                    #endregion

                gl.PopMatrix();
                #endregion

                #region Golf Club
                gl.PushMatrix();
                gl.Translate(-21f, 4.5f, 18f);
                gl.Rotate(130f, 0f, 1f, 0f);
                gl.Rotate(GolfClubAngle, 1f, 0f, 0f);
                m_golf_club.Draw();
                gl.PopMatrix();
                #endregion
           
            gl.PopMatrix();

            #region Light
            gl.PushMatrix();

            Sphere light = new Sphere();
            light.CreateInContext(gl);
            light.NormalGeneration = Normals.Smooth;
            light.Material = new SharpGL.SceneGraph.Assets.Material();
            light.Material.Emission = Color.White;

            float[] light_position = { 0f, 19f, -50f, 1f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light_position);

            float[] light0diffuse = new float[] { 1f, 1f, 1f, 1.0f };
            if (diffuseColor == Color.White) light0diffuse = new float[] { 1f, 1f, 1f, 1.0f };
            else if (diffuseColor == Color.Red) light0diffuse = new float[] { 1f, 0f, 0f, 1.0f };
            else if (diffuseColor == Color.Yellow) light0diffuse = new float[] { 1f, 1f, 0f, 1.0f };
            else if (diffuseColor == Color.Green) light0diffuse = new float[] { 0f, 1f, 0f, 1.0f };
            else if (diffuseColor == Color.Blue) light0diffuse = new float[] { 0f, 0f, 1f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);

            gl.Translate(0f, 19f, -50f);
            gl.Scale(0.5f, 0.5f, 0.5f);

            light.Material.Bind(gl);
            light.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            light.Material.Emission = Color.Black;
            light.Material.Bind(gl);
            gl.PopMatrix();
            #endregion

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

        public void StartAnimation()
        {
            AnimationActive = true;
            timer1.Start();
        }

        public void RunGolfClubAnimation(object sender, EventArgs e)
        {
            if (golfClubAngle >= 39) golfClubGoingUp = false;
            if (golfClubAngle == 0f) golfClubGoingUp = true;

            if (golfClubAngle == -1f)
            {
                timer1.Stop();
                timer2.Start();
                golfClubGoingUp = true;
                golfClubAngle = 0f;
            }

            if (golfClubGoingUp) golfClubAngle += 3f;
            if (!golfClubGoingUp) golfClubAngle -= 4f;
        }

        public void RunBallAnimation(object sender, EventArgs e)
        {
            if (ballPosition_y <= -7f)
            {
                timer2.Stop();
                AnimationActive = false;

                ballPosition_x = -21.0f;
                ballPosition_z = 20.0f;
                ballPosition_y = 0f;
            }

            if (ballPosition_x >= hole_x && ballPosition_z >= hole_z)
            {
                ballPosition_y -= 0.2f;
            } else
            {
                ballPosition_x += 1f;
                ballPosition_z = (20f - hole_z) / (-20f - hole_x) * (ballPosition_x - hole_x) + hole_z;
                ballPosition_y -= 0.025f;
            }
        }

        // Podesava viewport i projekciju za OpenGL kontrolu.
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            // Definisanje projekcije i Viewporta
            gl.Viewport(0, 0, width, height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);                            // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 500f);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                                              // resetuj ModelView Matrix
        }

        // Implementacija IDisposable interfejsa.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_golf_club.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
