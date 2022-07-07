using System;
using System.Collections.Generic;
using System.IO;
using LearnOpenTK.Common;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
//using Pertemuan7;

namespace DisneyCastle
{
    class Windows : GameWindow
    {
        /*private Mesh mesh0;*/
        private Mesh mesh1;
        private Mesh mesh2;
        private Mesh meshfire2;
        private Mesh meshfire3;
        private Mesh meshfirework2;
        private Mesh meshfirework3;
        private Mesh bulb;
        private Mesh bulb2;
        private Mesh girl;
        /*private Mesh lamp1;*/
        private Mesh moon;
        /*private Mesh mesh3;
        private Mesh mesh4;*/
        private Mesh mesh5;
        float[,] randomFireworks = new float[,] { { 0.3f, 0.5f, -0.4f }, { -0.3f, 0.5f, -0.4f }, { 0.1f, 0.5f, -0.4f } };




        Dictionary<string, List<Material>> materials_dict = new Dictionary<string, List<Material>>();

        private Camera _camera;
        private Vector3 _objectPos;

        private Vector2 _lastMousePosition;
        private bool _firstMove;
        private bool postprocessing = false;

        //Light
        List<Light> lights = new List<Light>();

        //Frame Buffers
        int fbo;

        //Shader
        Shader shader;
        Shader screenShader;
        Shader skyboxShader;

        Vector2 _lastPos;
        float _rotationSpeed = 1f;

        //Quad Screen
        float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
        };
        int _vao;
        int _vbo;
        int texColorBuffer;

        //Cubemap
        int cubemap;
        int _vao_cube;
        int _vbo_cube;
        float[] skyboxVertices = {
        // positions          
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f,  1.0f
    };

        public Windows(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
        {
            var rads = MathHelper.DegreesToRadians(degree);

            var secretFormula = new float[4, 4] {
                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
                { 0, 0, 0, 1}
            };
            var secretFormulaMatrix = new Matrix4(
                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
            );

            return secretFormulaMatrix;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.2f, 0.5f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            //Shader
            shader = new Shader("../../../Shaders/shader.vert",
                "../../../Shaders/lighting.frag");
            shader.Use();

            //Screen Shader
            screenShader = new Shader("../../../Shaders/PostProcessing.vert",
                "../../../Shaders/PostProcessing.frag");
            screenShader.Use();
            screenShader.SetInt("screenTexture", 0);
            //Frame Buffers
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            //Add Texture to Frame Buffer
            GL.GenTextures(1, out texColorBuffer);
            Console.WriteLine("TexColorBuffer: " + texColorBuffer);
            GL.BindTexture(TextureTarget.Texture2D, texColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 800, 600, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D
                , texColorBuffer, 0);
            //Render Buffer
            int rbo;
            GL.GenRenderbuffers(1, out rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 800, 600);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, rbo);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Screen Frame Buffer Created");
            }
            else
            {
                Console.WriteLine("Screen Frame Buffer NOT complete");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Initialize default material
            InitDefaultMaterial();
            //Create Cube Map
            CreateCubeMap();
            skyboxShader = new Shader("../../../Shaders/skybox.vert",
                "../../../Shaders/skybox.frag");

            //Vertices
            //Inisialiasi VBO
            _vbo_cube = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_cube);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float),
                skyboxVertices, BufferUsageHint.StaticDraw);

            //Inisialisasi VAO
            _vao_cube = GL.GenVertexArray();
            GL.BindVertexArray(_vao_cube);
            var vertexLocation = skyboxShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            skyboxShader.Use();
            skyboxShader.SetInt("skybox", 4);
            //Screen Quad
            GL.GenVertexArrays(1, out _vao);
            GL.GenBuffers(1, out _vbo);
            GL.BindVertexArray(_vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            //Light Position
            /*lights.Add(new PointLight(new Vector3(-1.5f, 0.8f, 2.2f), new Vector3(0.05f, 0.05f, 0.05f),
               new Vector3(0.8f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), 0.5f, 0.5f, 0.5f));*/
            /* lights.Add(new PointLight(new Vector3(3.5f, 0.8f, 2.2f), new Vector3(0.05f, 0.05f, 0.05f),
                      new Vector3(0.0f, 0.8f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), 0.5f, 0.5f, 0.5f));*/
            lights.Add(new PointLight(new Vector3(0.1f, 0.5f, -0.4f), new Vector3(0.05f, 0.05f, 0.05f),
                 new Vector3(0.8f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0.5f, 0.5f, 0.5f));
            lights.Add(new PointLight(new Vector3(-0.07f, 0.11f, 0.15f), new Vector3(0.05f, 0.05f, 0.05f),
              new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.0f, 0.0f, 0.0f), 0.5f, 0.5f, 0.5f));

            lights.Add(new DirectionLight(new Vector3(0.096f, 6f, 0.43f), new Vector3(0.05f, 0.05f, 0.05f),
              new Vector3(0.968f * 1.5f, 0.968f * 1.5f, 0.286f * 1.5f), new Vector3(1f, 1f, 1f), new Vector3(1f, -1f, 0f)));

            lights.Add(new PointLight(new Vector3(0.27f, 0.155f, 0.168f), new Vector3(0.05f, 0.05f, 0.05f),
             new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 0.0f), 0.5f, 0.5f, 0.5f));

            //lights.Add(new Light(new Vector3(0f,0f,0f), new Vector3(0.05f, 0.05f, 0.05f),
            //  new Vector3(1f, 1f, 1f), new Vector3(1f, 1f, 1f)));
            //x = kiri kanan
            //y = atas bawah
            //z = haruse depan belakang


            /* lights.Add(new PointLight(new Vector3(0.096f, 6f, 0.43f), new Vector3(0.05f, 0.05f, 0.05f),
               new Vector3(0.968f*2.5f, 0.968f* 2.5f, 0.286f* 2.5f), new Vector3(1f, 1f, 1f), 0.5f, 0.5f, 1f));*/



            /*  lights.Add(new PointLight(new Vector3(0.6f, 0.2f, 0.6f), new Vector3(0.05f, 0.05f, 0.05f),
                new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), 0.5f, 0.5f, 0.5f));*/
            /* lights.Add(new DirectionLight(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.05f, 0.05f, 0.05f),
                new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 1.0f, 1.0f), new Vector3(1f, 1f, 1f)));
 */
            //Inialisasi Mesh here
            mesh1 = LoadObjFile("../../../Resources/fire3.obj");
            mesh1.setupObject(1.0f, 1.0f);
            mesh1.translate(new Vector3(0.1f, 0.5f, -0.4f));


            meshfirework2 = LoadObjFile("../../../Resources/fire1.obj");
            meshfirework2.setupObject(1.0f, 1.0f);
            meshfirework2.translate(new Vector3(0.1f, 0.5f, -0.4f));
            meshfirework2.scale(0f);

            meshfirework3 = LoadObjFile("../../../Resources/fire2.obj");
            meshfirework3.setupObject(1.0f, 1.0f);
            meshfirework3.translate(new Vector3(0.1f, 0.5f, -0.4f));
            meshfirework3.scale(0f);



            bulb = LoadObjFile("../../../Resources/bulb.obj");
            bulb.setupObject(1.0f, 1.0f);
            bulb.translate(new Vector3(-0.07f, 0.11f, 0.15f));

            girl = LoadObjFile("../../../Resources/gir.obj");
            girl.setupObject(1.0f, 1.0f);
            girl.translate(new Vector3(0.22f, 0.0f, 0.07f));

            meshfire2 = LoadObjFile("../../../Resources/market.obj");
            meshfire2.setupObject(1.0f, 1.0f);
            meshfire2.translate(new Vector3(-0.23f, 0.255f, 0.2f));


            bulb2 = LoadObjFile("../../../Resources/bulb2.obj");
            bulb2.setupObject(1.0f, 1.0f);
            bulb2.translate(new Vector3(0.27f, 0.155f, 0.168f));

            meshfire3 = LoadObjFile("../../../Resources/tend2.obj");
            meshfire3.setupObject(1.0f, 1.0f);
            meshfire3.translate(new Vector3(-0.2f, 0.36f, 0.3f));
            meshfire3.scale(0.3f);

            mesh2 = LoadObjFile("../../../Resources/castel.obj");
            mesh2.setupObject(1.0f, 1.0f);
            mesh2.translate(new Vector3(0f, 0.0f, 0.0f));

            moon = LoadObjFile("../../../Resources/moon.obj");
            moon.setupObject(1.0f, 1.0f);
            moon.translate(new Vector3(0.096f, 7f, 0.43f));

            mesh5 = LoadObjFile("../../../Resources/rex.obj");
            mesh5.setupObject(1.0f, 1.0f);
            mesh5.scale(0.25f);
            mesh5.rotate(0f, 180f, 0f);
            mesh5.translate(new Vector3(0.096f, 0.0925f, 0.43f));
            _objectPos = mesh5.getTransform().ExtractTranslation();

            var _cameraPosInit = new Vector3(0, 0.5f, 0.5f);
            _camera = new Camera(_cameraPosInit, Size.X / (float)Size.Y);
            /* _camera.Fov = 90f;
             _camera.Yaw -= 90f;*/
            CursorGrabbed = true;
            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (GLFW.GetTime() > 0.02)
            {
                LampRevolution();
                GLFW.SetTime(0.0);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (postprocessing)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Textured Rendering
                GL.ActiveTexture(TextureUnit.Texture0);
                shader.Use();
                for (int i = 0; i < lights.Count; i++)
                {
                    mesh1.calculateTextureRender(_camera, lights[i], i);
                    mesh2.calculateTextureRender(_camera, lights[i], i);
                    mesh5.calculateTextureRender(_camera, lights[i], i);
                    bulb.calculateTextureRender(_camera, lights[i], i);
                    bulb2.calculateTextureRender(_camera, lights[i], i);
                    /*  lamp1.calculateTextureRender(_camera, lights[i], i);*/
                    moon.calculateTextureRender(_camera, lights[i], i);
                    meshfire2.calculateTextureRender(_camera, lights[i], i);
                    meshfire3.calculateTextureRender(_camera, lights[i], i);
                    meshfirework2.calculateTextureRender(_camera, lights[i], i);
                    meshfirework3.calculateTextureRender(_camera, lights[i], i);
                    girl.calculateTextureRender(_camera, lights[i], i);
                }

                GL.BindVertexArray(0);

                //Default FrameBuffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Disable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                screenShader.Use();
                screenShader.SetInt("screenTexture", texColorBuffer);
                GL.BindVertexArray(_vao);
                GL.BindTexture(TextureTarget.Texture2D, texColorBuffer);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
            else
            {
                shader.Use();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Enable(EnableCap.DepthTest);
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Textured Rendering
                GL.ActiveTexture(TextureUnit.Texture0);
                for (int i = 0; i < lights.Count; i++)
                {
                    /* mesh0.calculateTextureRender(_camera, lights[i], i);*/
                    mesh1.calculateTextureRender(_camera, lights[i], i);
                    mesh2.calculateTextureRender(_camera, lights[i], i);
                    meshfire2.calculateTextureRender(_camera, lights[i], i);
                    meshfire3.calculateTextureRender(_camera, lights[i], i);
                    /*mesh3.calculateTextureRender(_camera, lights[i], i);
                    mesh4.calculateTextureRender(_camera, lights[i], i);*/
                    mesh5.calculateTextureRender(_camera, lights[i], i);
                    bulb.calculateTextureRender(_camera, lights[i], i);
                    bulb2.calculateTextureRender(_camera, lights[i], i);
                    /* lamp1.calculateTextureRender(_camera, lights[i], i);*/
                    moon.calculateTextureRender(_camera, lights[i], i);
                    meshfirework2.calculateTextureRender(_camera, lights[i], i);
                    meshfirework3.calculateTextureRender(_camera, lights[i], i);
                    girl.calculateTextureRender(_camera, lights[i], i);
                }

                //Render Skybox
                GL.DepthFunc(DepthFunction.Lequal);
                //GL.DepthMask(false);
                skyboxShader.Use();
                Matrix4 skyview = _camera.GetViewMatrix().ClearTranslation().ClearScale();
                skyboxShader.SetMatrix4("view", skyview);

                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov),
                    Size.X / (float)Size.Y, 1f, 100f);
                skyboxShader.SetMatrix4("projection", projection);
                skyboxShader.SetInt("skybox", 4);
                GL.BindVertexArray(_vao_cube);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.TextureCubeMap, cubemap);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                GL.DepthFunc(DepthFunction.Less);

            }

            SwapBuffers();

            base.OnRenderFrame(args);
        }


        int lightCounter = 0;
        int zCounter = 0;
        int xCounter = 0;

        bool canMove(string check, int[] elementToCheck)
        {

            int[,] frontBanned = new int[,] { { -18, -3 }, { -18, 3 }, { -189, -73 }, { -190, -69 } };
            int[,] backBanned = new int[,] { { -49, -73 }, { -41, -69 }, { -42, -70 } };
            int[,] rightBanned = new int[,] { { -52, -21 } };
            int[,] leftBanned = new int[,] { { -190, -69 }, { -41, -69 }, { -42, -70 } };


            int[] bridgeKiriDepan = new int[] { -18, -3 };
            int[] bridgeKananDepan = new int[] { -18, 3 };
            int[] bridgeKiriBelakang = new int[] { 0, -3 };
            int[] bridgeKananBelakang = new int[] { 0, 3 };

            int[] toCasKiriDepan = new int[] { -53, -2 };
            int[] toCasKananDepan = new int[] { -53, 2 };
            int[] toCasKiriBelakang = new int[] { -20, -2 };
            int[] toCasKananBelakang = new int[] { 20, 2 };

            int[] CasKiriDepan = new int[] { -189, -73 };
            int[] CasKiriDepanK = new int[] { -189, -71 };
            int[] CasKiriBelakang = new int[] { -49, -73 };

            int[] CasKiriDep = new int[] { -46, -69 };
            int[] CasKiri = new int[] { -41, -69 };
            int[] CasKanan = new int[] { -41, -30 };

            int[] CasKananD = new int[] { -48, -30 };

            int[] CasSKiri = new int[] { -52, -27 };
            int[] CasSKanan = new int[] { -52, -21 };

            int[] CasDKiri = new int[] { -190, -70 };
            int[] CasDKanan = new int[] { -190, -37 };
            int[] CasDKananBe = new int[] { -164, -37 };

            int[] CasS2Kiri = new int[] { -162, -35 };
            int[] CasS2Kanan = new int[] { -162, -28 };
            int[] CasS2KananBe = new int[] { -158, -28 };
            int[] CasS2KirinBe = new int[] { -158, -34 };

            int[] CasS2D = new int[] { -157, -35 };
            int[] CasS2B = new int[] { -137, -35 };

            int[] CasS3Kiri = new int[] { -136, -32 };
            int[] CasS3Kanan = new int[] { -137, -26 };

            int[] CasS3KananB = new int[] { -133, -26 };
            int[] CasS3KiriB = new int[] { -133, -35 };

            int[] Cas3De = new int[] { -130, -37 };
            int[] Cas3Be = new int[] { -104, -37 };

            int[] Cas4Kiri = new int[] { -101, -35 };
            int[] Cas4Kanan = new int[] { -101, -15 };
            int[] Cas4KananD = new int[] { -112, -15 };
            int[] Cas4KananKD = new int[] { -112, -8 };
            int[] Cas4KananKB = new int[] { -108, -8 };
            int[] Cas4KananKBK = new int[] { -108, -3 };
            int[] Cas5D = new int[] { -120, -3 };
            int[] Cas5Ka = new int[] { -120, 3 };


            int[] inCasKananFirst = new int[] { -56, 2 };
            int[] inCasKananSecond = new int[] { -56, 21 };
            int[] inCasKananThird = new int[] { -52, 21 };
            int[] inCasKananFourth = new int[] { -52, 29 };
            int[] inCasKananFifth = new int[] { -39, 29 };
            int[] inCasKananSixth = new int[] { -39, 69 };
            int[] inCasKananSeventh = new int[] { -50, 69 };
            int[] inCasKananEight = new int[] { -50, 74 };
            int[] inCasKananNinth = new int[] { -189, 74 };
            int[] inCasKananTenth = new int[] { -189, 69 };
            int[] inCasKananEleventh = new int[] { -191, 69 };
            int[] inCasKananTwelfth = new int[] { -191, 37 };
            int[] inCasKananThirteen = new int[] { -161, 37 };
            int[] inCasKananFourteenth = new int[] { -161, 26 };
            int[] inCasKananFifteen = new int[] { -158, 26 };
            int[] inCasKananSixteen = new int[] { -158, 34 };
            int[] inCasKananSeventeen = new int[] { -136, 34 };
            int[] inCasKananEighteen = new int[] { -136, 26 };
            int[] inCasKananNineteen = new int[] { -133, 26 };
            int[] inCasKanan20 = new int[] { -133, 37 };
            int[] inCasKanan21 = new int[] { -103, 37 };
            int[] inCasKanan22 = new int[] { -103, 15 };
            int[] inCasKanan23 = new int[] { -113, 15 };
            int[] inCasKanan24 = new int[] { -113, 8 };
            int[] inCasKanan25 = new int[] { -107, 8 };
            int[] inCasKanan26 = new int[] { -107, 3 };
            int[] inCasKanan27 = new int[] { -120, 3 };

            int[] standKiriBlkg = new int[] { -55, 38 };
            int[] standKananBlkg = new int[] { -55, 57 };
            int[] standKananDpn = new int[] { -84, 57 };
            int[] standKirinDpn = new int[] { -84, 38 };

            int[] tentKanBlkg = new int[] { -54, -22 };
            int[] tentKanDe = new int[] { -67, -22 };

            int[] tentKirBlkg = new int[] { -54, -63 };
            int[] tentKirDe = new int[] { -98, -63 };

            int[] tentKanD2 = new int[] { -98, -22 };
            int[] tentKanB2 = new int[] { -76, -22 };

            int[] lampRightFar = new int[] { -78, -43 };
            int[] lampRightNear = new int[] { -69, -43 };
            int[] lampLeftNear = new int[] { -69, -46 };
            int[] lampLeftFar = new int[] { -78, -46 };

            int[] tentInKirBlkg = new int[] { -65, -51 };
            int[] tentInKirDe = new int[] { -83, -51 };

            int[] tentInKanD = new int[] { -83, -34 };
            int[] tentInKanD2 = new int[] { -79, -34 };
            int[] tentInKanB2 = new int[] { -68, -34 };
            int[] tentInKanB = new int[] { -65, -34 };


            if (check == "front")
            {

                for (int i = 0; i < frontBanned.GetLength(0); i++)
                {

                    if (frontBanned[i, 0] == elementToCheck[0] && frontBanned[i, 1] == elementToCheck[1])
                    {
                        return false;
                    }
                }
                if (CasDKanan[1] >= elementToCheck[1] && CasDKiri[1] <= elementToCheck[1] && CasDKiri[0] == elementToCheck[0])
                {
                    return false;
                }
                if (tentKanBlkg[1] >= elementToCheck[1] && tentKirBlkg[1] <= elementToCheck[1] && tentKirBlkg[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasKiriDepanK[1] >= elementToCheck[1] && CasKiriDepan[1] <= elementToCheck[1] && CasKiriDepan[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasS2Kanan[1] >= elementToCheck[1] && CasS2Kiri[1] <= elementToCheck[1] && CasS2Kiri[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasS3Kanan[1] >= elementToCheck[1] && CasS3Kiri[1] <= elementToCheck[1] && CasS3Kiri[0] == elementToCheck[0])
                {
                    return false;
                }

                if (standKananBlkg[1] >= elementToCheck[1] && standKiriBlkg[1] <= elementToCheck[1] && standKiriBlkg[0] == elementToCheck[0])
                {
                    return false;
                }
                if (Cas4KananKD[1] >= elementToCheck[1] && Cas4KananD[1] <= elementToCheck[1] && Cas4KananD[0] == elementToCheck[0])
                {
                    return false;
                }
                if (Cas4Kanan[1] >= elementToCheck[1] && Cas4Kiri[1] <= elementToCheck[1] && Cas4Kiri[0] == elementToCheck[0])
                {
                    return false;
                }
                if (Cas4KananKBK[1] >= elementToCheck[1] && Cas4KananKB[1] <= elementToCheck[1] && Cas4KananKB[0] == elementToCheck[0])
                {
                    return false;
                }
                if (Cas5Ka[1] >= elementToCheck[1] && Cas5D[1] <= elementToCheck[1] && Cas5D[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananNinth[1] >= elementToCheck[1] && inCasKananTenth[1] <= elementToCheck[1] && inCasKananTenth[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananEleventh[1] >= elementToCheck[1] && inCasKananTwelfth[1] <= elementToCheck[1] && inCasKananEleventh[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananThirteen[1] >= elementToCheck[1] && inCasKananFourteenth[1] <= elementToCheck[1] && inCasKananFourteenth[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananSeventeen[1] >= elementToCheck[1] && inCasKananEighteen[1] <= elementToCheck[1] && inCasKananEighteen[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKanan21[1] >= elementToCheck[1] && inCasKanan22[1] <= elementToCheck[1] && inCasKanan22[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKanan23[1] >= elementToCheck[1] && inCasKanan24[1] <= elementToCheck[1] && inCasKanan23[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKanan25[1] >= elementToCheck[1] && inCasKanan26[1] <= elementToCheck[1] && inCasKanan25[0] == elementToCheck[0])
                {
                    return false;
                }
                if (lampRightNear[1] >= elementToCheck[1] && lampLeftNear[1] <= elementToCheck[1] && lampLeftNear[0] == elementToCheck[0])
                {
                    return false;
                }
                if (tentInKanD[1] >= elementToCheck[1] && tentInKirDe[1] <= elementToCheck[1] && tentInKirDe[0] == elementToCheck[0])
                {
                    return false;
                }

            }
            /*    if (check == "back")
                {
                    Console.WriteLine(backBanned.GetLength(0));
                    for (int i = 0; i < backBanned.GetLength(0); i++)
                    {

                        if (backBanned[i, 0] == elementToCheck[0] && backBanned[i, 1] == elementToCheck[1])
                        {
                            return false;
                        }
                    }
                }*/
            if (check == "back")
            {

                if (bridgeKananBelakang[1] >= elementToCheck[1] && bridgeKiriBelakang[1] <= elementToCheck[1] && bridgeKiriBelakang[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasKanan[1] >= elementToCheck[1] && CasKiri[1] <= elementToCheck[1] && CasKiri[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasSKanan[1] >= elementToCheck[1] && CasSKiri[1] <= elementToCheck[1] && CasSKiri[0] == elementToCheck[0])
                {
                    return false;
                }
                if (tentKanD2[1] >= elementToCheck[1] && tentKirDe[1] <= elementToCheck[1] && tentKirDe[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasS2KananBe[1] >= elementToCheck[1] && CasS2KirinBe[1] <= elementToCheck[1] && CasS2KirinBe[0] == elementToCheck[0])
                {
                    return false;
                }
                if (CasS3KananB[1] >= elementToCheck[1] && CasS3KiriB[1] <= elementToCheck[1] && CasS3KiriB[0] == elementToCheck[0])
                {
                    return false;
                }
                if (standKananDpn[1] >= elementToCheck[1] && standKirinDpn[1] <= elementToCheck[1] && standKirinDpn[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananSecond[1] >= elementToCheck[1] && inCasKananFirst[1] <= elementToCheck[1] && inCasKananFirst[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananFourth[1] >= elementToCheck[1] && inCasKananThird[1] <= elementToCheck[1] && inCasKananThird[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananSixth[1] >= elementToCheck[1] && inCasKananFifth[1] <= elementToCheck[1] && inCasKananSixth[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananEight[1] >= elementToCheck[1] && inCasKananSeventh[1] <= elementToCheck[1] && inCasKananEight[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKananSixteen[1] >= elementToCheck[1] && inCasKananFifteen[1] <= elementToCheck[1] && inCasKananFifteen[0] == elementToCheck[0])
                {
                    return false;
                }
                if (inCasKanan20[1] >= elementToCheck[1] && inCasKananNineteen[1] <= elementToCheck[1] && inCasKananNineteen[0] == elementToCheck[0])
                {
                    return false;
                }
                if (lampRightFar[1] >= elementToCheck[1] && lampLeftFar[1] <= elementToCheck[1] && lampLeftFar[0] == elementToCheck[0])
                {
                    return false;
                }
                if (tentInKanB[1] >= elementToCheck[1] && tentInKirBlkg[1] <= elementToCheck[1] && tentInKirBlkg[0] == elementToCheck[0])
                {
                    return false;
                }
                for (int i = 0; i < backBanned.GetLength(0); i++)
                {

                    if (backBanned[i, 0] == elementToCheck[0] && backBanned[i, 1] == elementToCheck[1])
                    {
                        return false;
                    }
                }

            }
            if (check == "left")
            {

                if (bridgeKiriBelakang[0] >= elementToCheck[0] && bridgeKiriDepan[0] <= elementToCheck[0] && bridgeKiriBelakang[1] == elementToCheck[1])
                {
                    return false;
                }
                if (toCasKiriBelakang[0] >= elementToCheck[0] && toCasKiriDepan[0] <= elementToCheck[0] && toCasKiriBelakang[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasKiriBelakang[0] >= elementToCheck[0] && CasKiriDepan[0] <= elementToCheck[0] && CasKiriBelakang[1] == elementToCheck[1])
                {

                    return false;
                }
                if (tentKanBlkg[0] >= elementToCheck[0] && tentKanDe[0] <= elementToCheck[0] && tentKanBlkg[1] == elementToCheck[1])
                {

                    return false;
                }
                if (tentKanB2[0] >= elementToCheck[0] && tentKanD2[0] <= elementToCheck[0] && tentKanB2[1] == elementToCheck[1])
                {

                    return false;
                }

                if (CasKiri[0] >= elementToCheck[0] && CasKiriDep[0] <= elementToCheck[0] && CasKiri[1] == elementToCheck[1])
                {
                    return false;
                }
                if (standKananBlkg[0] >= elementToCheck[0] && standKananDpn[0] <= elementToCheck[0] && standKananBlkg[1] == elementToCheck[1])
                {
                    return false;
                }
                if (Cas4Kanan[0] >= elementToCheck[0] && Cas4KananD[0] <= elementToCheck[0] && Cas4Kanan[1] == elementToCheck[1])
                {
                    return false;
                }
                if (Cas4KananKBK[0] >= elementToCheck[0] && Cas5D[0] <= elementToCheck[0] && Cas4KananKBK[1] == elementToCheck[1])
                {
                    return false;
                }

                if (inCasKananThird[0] >= elementToCheck[0] && inCasKananSecond[0] <= elementToCheck[0] && inCasKananThird[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananFifth[0] >= elementToCheck[0] && inCasKananFourth[0] <= elementToCheck[0] && inCasKananFifth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananThirteen[0] >= elementToCheck[0] && inCasKananTwelfth[0] <= elementToCheck[0] && inCasKananTwelfth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananFifteen[0] >= elementToCheck[0] && inCasKananFourteenth[0] <= elementToCheck[0] && inCasKananFourteenth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananSeventeen[0] >= elementToCheck[0] && inCasKananSixteen[0] <= elementToCheck[0] && inCasKananSeventeen[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananNineteen[0] >= elementToCheck[0] && inCasKananEighteen[0] <= elementToCheck[0] && inCasKananEighteen[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKanan21[0] >= elementToCheck[0] && inCasKanan20[0] <= elementToCheck[0] && inCasKanan21[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKanan25[0] >= elementToCheck[0] && inCasKanan24[0] <= elementToCheck[0] && inCasKanan25[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKanan25[0] >= elementToCheck[0] && inCasKanan24[0] <= elementToCheck[0] && inCasKanan25[1] == elementToCheck[1])
                {
                    return false;
                }

                if (lampRightNear[0] >= elementToCheck[0] && lampRightFar[0] <= elementToCheck[0] && lampRightFar[1] == elementToCheck[1])
                {
                    return false;
                }

                if (tentInKirBlkg[0] >= elementToCheck[0] && tentInKirDe[0] <= elementToCheck[0] && tentInKirBlkg[1] == elementToCheck[1])
                {

                    return false;
                }

                for (int i = 0; i < leftBanned.GetLength(0); i++)
                {

                    if (leftBanned[i, 0] == elementToCheck[0] && leftBanned[i, 1] == elementToCheck[1])
                    {
                        return false;
                    }
                }

            }
            if (check == "right")
            {

                if (bridgeKananBelakang[0] >= elementToCheck[0] && bridgeKananDepan[0] <= elementToCheck[0] && bridgeKananBelakang[1] == elementToCheck[1])
                {
                    return false;
                }
                if (toCasKananBelakang[0] >= elementToCheck[0] && toCasKananDepan[0] <= elementToCheck[0] && toCasKananBelakang[1] == elementToCheck[1])
                {
                    return false;
                }
                if (standKiriBlkg[0] >= elementToCheck[0] && standKirinDpn[0] <= elementToCheck[0] && standKiriBlkg[1] == elementToCheck[1])
                {
                    return false;
                }
                if (tentKirBlkg[0] >= elementToCheck[0] && tentKirDe[0] <= elementToCheck[0] && tentKirBlkg[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasKanan[0] >= elementToCheck[0] && CasKananD[0] <= elementToCheck[0] && CasKanan[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasDKananBe[0] >= elementToCheck[0] && CasDKanan[0] <= elementToCheck[0] && CasDKanan[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasS2KananBe[0] >= elementToCheck[0] && CasS2Kanan[0] <= elementToCheck[0] && CasS2Kanan[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasS2B[0] >= elementToCheck[0] && CasS2D[0] <= elementToCheck[0] && CasS2D[1] == elementToCheck[1])
                {
                    return false;
                }
                if (Cas4KananKB[0] >= elementToCheck[0] && Cas4KananKD[0] <= elementToCheck[0] && Cas4KananKD[1] == elementToCheck[1])
                {
                    return false;
                }
                if (CasS3KananB[0] >= elementToCheck[0] && CasS3Kanan[0] <= elementToCheck[0] && CasS3Kanan[1] == elementToCheck[1])
                {
                    return false;
                }
                if (Cas3Be[0] >= elementToCheck[0] && Cas3De[0] <= elementToCheck[0] && Cas3De[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananSixth[0] >= elementToCheck[0] && inCasKananSeventh[0] <= elementToCheck[0] && inCasKananSixth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananEight[0] >= elementToCheck[0] && inCasKananNinth[0] <= elementToCheck[0] && inCasKananNinth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKananTenth[0] >= elementToCheck[0] && inCasKananEleventh[0] <= elementToCheck[0] && inCasKananTenth[1] == elementToCheck[1])
                {
                    return false;
                }
                if (inCasKanan22[0] >= elementToCheck[0] && inCasKanan23[0] <= elementToCheck[0] && inCasKanan22[1] == elementToCheck[1])
                {
                    return false;
                }
                if (lampLeftNear[0] >= elementToCheck[0] && lampLeftFar[0] <= elementToCheck[0] && lampLeftFar[1] == elementToCheck[1])
                {
                    return false;
                }
                if (tentInKanB[0] >= elementToCheck[0] && tentInKanB2[0] <= elementToCheck[0] && tentInKanB[1] == elementToCheck[1])
                {
                    return false;
                }
                if (tentInKanD2[0] >= elementToCheck[0] && tentInKanD[0] <= elementToCheck[0] && tentInKanD2[1] == elementToCheck[1])
                {
                    return false;
                }



                for (int i = 0; i < rightBanned.GetLength(0); i++)
                {

                    if (rightBanned[i, 0] == elementToCheck[0] && rightBanned[i, 1] == elementToCheck[1])
                    {
                        return false;
                    }
                }


            }



            return true;
        }



        protected override void OnUpdateFrame(FrameEventArgs args)
        {


            lightCounter++;


            if (lightCounter == 60)
            {
                Random rd = new Random();
                int rand_num = rd.Next(0, 3);
                meshfirework2.setupObject(1f, 1f);
                float x = randomFireworks[rand_num, 0];
                float y = randomFireworks[rand_num, 1];
                float z = randomFireworks[rand_num, 2];

                meshfirework2.translate(new Vector3(x, y, z));
                mesh1.scale(0);

                lights[0].Diffuse = new Vector3(0f, 0.8f, 0f);
                lights[0].Position = new Vector3(x, y, z);


            }
            else if (lightCounter == 120)
            {
                Random rd = new Random();
                int rand_num = rd.Next(0, 3);
                meshfirework2.setupObject(1f, 1f);
                float x = randomFireworks[rand_num, 0];
                float y = randomFireworks[rand_num, 1];
                float z = randomFireworks[rand_num, 2];

                meshfirework3.setupObject(1f, 1f);
                meshfirework3.translate(new Vector3(x, y, z));
                meshfirework2.scale(0);
                lights[0].Diffuse = new Vector3(0f, 0f, 0.8f);
                lights[0].Position = new Vector3(x, y, z);

            }
            else if (lightCounter == 180)
            {
                Random rd = new Random();
                int rand_num = rd.Next(0, 3);
                meshfirework2.setupObject(1f, 1f);
                float x = randomFireworks[rand_num, 0];
                float y = randomFireworks[rand_num, 1];
                float z = randomFireworks[rand_num, 2];


                mesh1.setupObject(1f, 1f);
                mesh1.translate(new Vector3(x, y, z));
                meshfirework2.scale(0);
                meshfirework3.scale(0);
                /*  lights[0].Diffuse = new Vector3(0f, 0.8f, 0f);*/
                lights[0].Diffuse = new Vector3(0.8f, 0f, 0f);
                lights[0].Position = new Vector3(x, y, z);
                lightCounter = 0;
                /*mesh1.scale(1f);
                meshfire2.scale(0f);
                meshfire3.scale(0f);*/

            }


            const float cameraSpeed = 1.5f;
            // Escape keyboard
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            // Zoom in
            if (KeyboardState.IsKeyDown(Keys.Up))
            {

                _camera.Fov -= 0.5f;
            }
            // Zoom out
            if (KeyboardState.IsKeyDown(Keys.Down))
            {

                _camera.Fov += 0.5f;
            }

            // Rotasi X di pivot Camera
            // Lihat ke atas (T)
            if (KeyboardState.IsKeyDown(Keys.T))
            {
                _camera.Pitch += 0.05f;
            }
            // Lihat ke bawah (G)
            if (KeyboardState.IsKeyDown(Keys.G))
            {
                _camera.Pitch -= 0.05f;
            }
            // Rotasi Y di pivot Camera
            // Lihat ke kiri (F)
            if (KeyboardState.IsKeyDown(Keys.F))
            {
                _camera.Yaw -= 0.05f;
            }
            // Lihat ke kanan (H)
            if (KeyboardState.IsKeyDown(Keys.H))
            {
                _camera.Yaw += 0.05f;
            }

            // Maju (W)
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                //_camera.Position += _camera.Front * cameraSpeed * (float)args.Time;
                //_objectPos += _camera.Position;
                int[] curCoord = new int[] { zCounter, xCounter };
                if (canMove("front", curCoord))
                {
                    mesh5.translate(new Vector3(0, 0, -0.0025f) * cameraSpeed);
                    Vector3 vec = mesh5.getTransform().ExtractTranslation();
                    _camera.Position = new Vector3(vec.X, vec.Y + 0.1f, vec.Z + 0.1f);
                    zCounter--;
                }

                Console.WriteLine("Zcounter = " + zCounter);
                Console.WriteLine("xcounter = " + xCounter);

            }
            // Mundur (S)
            if (KeyboardState.IsKeyDown(Keys.S))
            {

                int[] curCoord = new int[] { zCounter, xCounter };
                if (canMove("back", curCoord))
                {
                    mesh5.translate(-(new Vector3(0, 0, -0.0025f) * cameraSpeed));
                    Vector3 vec = mesh5.getTransform().ExtractTranslation();
                    _camera.Position = new Vector3(vec.X, vec.Y + 0.1f, vec.Z + 0.1f);
                    zCounter++;
                }
                //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;
                //_objectPos += _camera.Position;

                Console.WriteLine("Zcounter = " + zCounter);
                Console.WriteLine("xcounter = " + xCounter);
            }
            // Kiri (A)
            if (KeyboardState.IsKeyDown(Keys.A))
            {

                int[] curCoord = new int[] { zCounter, xCounter };
                if (canMove("left", curCoord))
                {
                    mesh5.translate(-(new Vector3(0.0025f, 0, 0) * cameraSpeed));
                    Vector3 vec = mesh5.getTransform().ExtractTranslation();
                    _camera.Position = new Vector3(vec.X, vec.Y + 0.1f, vec.Z + 0.1f);
                    xCounter--;
                }

                //_camera.Position -= new Vector3(1, 0, 0) * cameraSpeed * (float)args.Time;
                //_objectPos += _camera.Position;

                Console.WriteLine("Zcounter = " + zCounter);
                Console.WriteLine("xcounter = " + xCounter);
            }
            // Kanan (D)
            if (KeyboardState.IsKeyDown(Keys.D))
            {

                int[] curCoord = new int[] { zCounter, xCounter };
                if (canMove("right", curCoord))
                {
                    mesh5.translate((new Vector3(0.0025f, 0, 0) * cameraSpeed));
                    Vector3 vec = mesh5.getTransform().ExtractTranslation();
                    _camera.Position = new Vector3(vec.X, vec.Y + 0.1f, vec.Z + 0.1f);
                    xCounter++;
                }

                //_camera.Position += new Vector3(1, 0, 0) * cameraSpeed * (float)args.Time;
                //_objectPos += _camera.Position;

                Console.WriteLine("Zcounter = " + zCounter);
                Console.WriteLine("xcounter = " + xCounter);

            }
            // Naik (Spasi)
            if (KeyboardState.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;
            }
            // Turun (Ctrl)
            if (KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;
            }
            // Blinn
            if (KeyboardState.IsKeyReleased(Keys.F1))
            {
                /* mesh0.setBlinn(!mesh0.getBlinn());*/
                mesh1.setBlinn(!mesh1.getBlinn());
                mesh2.setBlinn(!mesh2.getBlinn());
                /* mesh3.setBlinn(!mesh3.getBlinn());
                 mesh4.setBlinn(!mesh4.getBlinn());*/
                mesh5.setBlinn(!mesh5.getBlinn());
                moon.setBlinn(!mesh5.getBlinn());
                bulb.setBlinn(!mesh5.getBlinn());
                /*   lamp1.setBlinn(!mesh5.getBlinn());*/
                meshfire2.setBlinn(!mesh5.getBlinn());
                meshfire3.setBlinn(!mesh5.getBlinn());
                meshfirework2.setBlinn(!mesh5.getBlinn());
                meshfirework3.setBlinn(!mesh5.getBlinn());

            }
            if (KeyboardState.IsKeyReleased(Keys.F2))
            {
                /*  mesh0.setGamma(!mesh0.getGamma());*/
                mesh1.setGamma(!mesh1.getGamma());
                mesh2.setGamma(!mesh2.getGamma());
                /*  mesh3.setGamma(!mesh3.getGamma());
                  mesh4.setGamma(!mesh4.getGamma());*/
                mesh5.setGamma(!mesh5.getGamma());
                moon.setGamma(!mesh5.getGamma());
                /* lamp1.setGamma(!mesh5.getGamma());*/
                bulb.setGamma(!mesh5.getGamma());
                meshfire2.setGamma(!mesh5.getGamma());
                meshfire3.setGamma(!mesh5.getGamma());
                meshfirework2.setGamma(!mesh5.getGamma());
                meshfirework3.setGamma(!mesh5.getGamma());
            }
            if (KeyboardState.IsKeyReleased(Keys.F3))
            {
                postprocessing = !postprocessing;
            }

            const float _rotationSpeed = 0.1f;
            // K (atas -> Rotasi sumbu x)
            if (KeyboardState.IsKeyDown(Keys.K))
            {
                _objectPos *= 2;
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objectPos;
                _camera.Pitch -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // M (bawah -> Rotasi sumbu x)
            if (KeyboardState.IsKeyDown(Keys.M))
            {
                _objectPos *= 2;
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objectPos;
                _camera.Pitch += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            // N (kiri -> Rotasi sumbu y)
            if (KeyboardState.IsKeyDown(Keys.N))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // , (kanan -> Rotasi sumbu y)
            if (KeyboardState.IsKeyDown(Keys.Comma))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            // J (putar -> Rotasi sumbu z)
            if (KeyboardState.IsKeyDown(Keys.J))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 0, 1);
                _camera.Position -= _objectPos;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, mesh5.getTransform().ExtractTranslation(), _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // L (putar -> Rotasi sumbu z)
            if (KeyboardState.IsKeyDown(Keys.L))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 0, 1);
                _camera.Position -= _objectPos;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, mesh5.getTransform().ExtractTranslation(), -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            if (!IsFocused)
            {
                return;
            }

            const float sensitivity = 0.2f;
            if (_firstMove)
            {
                _lastMousePosition = new Vector2(MouseState.X, MouseState.Y);
                _firstMove = false;
            }
            else
            {
                // Hitung selisih mouse position
                var deltaX = MouseState.X - _lastMousePosition.X;
                var deltaY = MouseState.Y - _lastMousePosition.Y;
                _lastMousePosition = new Vector2(MouseState.X, MouseState.Y);


                //YAW
                _objectPos *= 2;
                var axisYaw = new Vector3(0, 1, 0);
                _camera.Position -= mesh5.getTransform().ExtractTranslation();
                _camera.Yaw += deltaX * _rotationSpeed * sensitivity * 0.1f;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axisYaw, mesh5.getTransform().ExtractTranslation(), deltaX).ExtractRotation());
                _camera.Position += mesh5.getTransform().ExtractTranslation();

                _camera._front = -Vector3.Normalize(_camera.Position - mesh5.getTransform().ExtractTranslation());
                _objectPos /= 2;

                //Pitch
                _objectPos *= 2;
                var axisPitch = new Vector3(1, 0, 0);
                _camera.Position -= mesh5.getTransform().ExtractTranslation();
                _camera.Pitch -= deltaY * _rotationSpeed * sensitivity * 0.1f;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axisPitch, mesh5.getTransform().ExtractTranslation(), deltaY).ExtractRotation());
                _camera.Position += mesh5.getTransform().ExtractTranslation();

                _camera._front = -Vector3.Normalize(_camera.Position - mesh5.getTransform().ExtractTranslation());
                _objectPos /= 2;
                //_camera.Yaw += deltaX * sensitivity;
                //_camera.Pitch -= deltaY * sensitivity;
            }

            base.OnUpdateFrame(args);
        }

        private void InitDefaultMaterial()
        {
            List<Material> materials = new List<Material>();
            Texture diffuseMap = Texture.LoadFromFile("../../../Resources/white.jpg");
            Texture textureMap = Texture.LoadFromFile("../../../Resources/white.jpg");
            materials.Add(new Material("Default", 128.0f, new Vector3(0.1f), new Vector3(1f), new Vector3(1f),
                    1.0f, diffuseMap, textureMap));

            materials_dict.Add("Default", materials);

        }

        public Mesh LoadObjFile(string path, bool usemtl = true)
        {
            Mesh mesh = new Mesh("../../../Shaders/shader.vert",
                "../../../Shaders/lighting.frag");
            List<Vector3> temp_vertices = new List<Vector3>();
            List<Vector3> temp_normals = new List<Vector3>();
            List<Vector3> temp_textureVertices = new List<Vector3>();
            List<uint> temp_vertexIndices = new List<uint>();
            List<uint> temp_normalsIndices = new List<uint>();
            List<uint> temp_textureIndices = new List<uint>();
            List<string> temp_name = new List<string>();
            List<String> temp_materialsName = new List<string>();
            string current_materialsName = "";
            string material_library = "";
            int mesh_count = 0;
            int mesh_created = 0;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }

            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    words.RemoveAll(s => s == string.Empty);

                    if (words.Count == 0)
                        continue;
                    string type = words[0];

                    words.RemoveAt(0);

                    switch (type)
                    {
                        //Render tergantung nama dan objek apa sehingga bisa buat hirarki
                        case "o":
                            if (mesh_count > 0)
                            {
                                Mesh mesh_tmp = new Mesh();
                                //Attach Shader
                                mesh_tmp.setShader(shader);
                                mesh_tmp.setDepthShader(skyboxShader);
                                for (int i = 0; i < temp_vertexIndices.Count; i++)
                                {
                                    uint vertexIndex = temp_vertexIndices[i];
                                    mesh_tmp.AddVertices(temp_vertices[(int)vertexIndex - 1]);
                                }
                                for (int i = 0; i < temp_textureIndices.Count; i++)
                                {
                                    uint textureIndex = temp_textureIndices[i];
                                    mesh_tmp.AddTextureVertices(temp_textureVertices[(int)textureIndex - 1]);
                                }
                                for (int i = 0; i < temp_normalsIndices.Count; i++)
                                {
                                    uint normalIndex = temp_normalsIndices[i];
                                    mesh_tmp.AddNormals(temp_normals[(int)normalIndex - 1]);
                                }
                                mesh_tmp.setName(temp_name[mesh_created]);

                                //Material
                                if (usemtl)
                                {

                                    List<Material> mtl = materials_dict[material_library];
                                    for (int i = 0; i < mtl.Count; i++)
                                    {
                                        if (mtl[i].Name == current_materialsName)
                                        {
                                            mesh_tmp.setMaterial(mtl[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    List<Material> mtl = materials_dict["Default"];
                                    for (int i = 0; i < mtl.Count; i++)
                                    {
                                        if (mtl[i].Name == "Default")
                                        {
                                            mesh_tmp.setMaterial(mtl[i]);
                                        }
                                    }
                                }


                                if (mesh_count == 1)
                                {
                                    mesh = mesh_tmp;
                                }
                                else
                                {
                                    mesh.child.Add(mesh_tmp);
                                }

                                mesh_created++;
                            }
                            temp_name.Add(words[0]);
                            mesh_count++;
                            break;
                        case "v":
                            temp_vertices.Add(new Vector3(float.Parse(words[0]) / 10, float.Parse(words[1]) / 10, float.Parse(words[2]) / 10));
                            break;

                        case "vt":
                            temp_textureVertices.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]),
                                                            words.Count < 3 ? 0 : float.Parse(words[2])));
                            break;

                        case "vn":
                            temp_normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "mtllib":
                            if (usemtl)
                            {
                                string resourceName = "../../../Resources/" + words[0];
                                string nameWOExt = words[0].Split(".")[0];
                                Console.WriteLine(nameWOExt);
                                materials_dict.Add(nameWOExt, LoadMtlFile(resourceName));
                                material_library = nameWOExt;
                            }

                            break;
                        case "usemtl":
                            if (usemtl)
                            {
                                current_materialsName = words[0];
                            }

                            break;
                        // face
                        case "f":
                            foreach (string w in words)
                            {
                                if (w.Length == 0)
                                    continue;

                                string[] comps = w.Split('/');
                                for (int i = 0; i < comps.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (comps[0].Length > 0)
                                        {
                                            temp_vertexIndices.Add(uint.Parse(comps[0]));
                                        }

                                    }
                                    else if (i == 1)
                                    {
                                        if (comps[1].Length > 0)
                                        {
                                            temp_textureIndices.Add(uint.Parse(comps[1]));
                                        }

                                    }
                                    else if (i == 2)
                                    {
                                        if (comps[2].Length > 0)
                                        {
                                            temp_normalsIndices.Add(uint.Parse(comps[2]));
                                        }

                                    }
                                }

                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            if (mesh_created < mesh_count)
            {

                Mesh mesh_tmp = new Mesh();
                //Attach Shader
                mesh_tmp.setShader(shader);
                mesh_tmp.setDepthShader(skyboxShader);
                for (int i = 0; i < temp_vertexIndices.Count; i++)
                {
                    uint vertexIndex = temp_vertexIndices[i];
                    mesh_tmp.AddVertices(temp_vertices[(int)vertexIndex - 1]);
                }
                for (int i = 0; i < temp_textureIndices.Count; i++)
                {
                    uint textureIndex = temp_textureIndices[i];
                    mesh_tmp.AddTextureVertices(temp_textureVertices[(int)textureIndex - 1]);
                }
                for (int i = 0; i < temp_normalsIndices.Count; i++)
                {
                    uint normalIndex = temp_normalsIndices[i];
                    mesh_tmp.AddNormals(temp_normals[(int)normalIndex - 1]);
                }
                mesh_tmp.setName(temp_name[mesh_created]);

                //Material
                if (usemtl)
                {

                    List<Material> mtl = materials_dict[material_library];
                    for (int i = 0; i < mtl.Count; i++)
                    {
                        if (mtl[i].Name == current_materialsName)
                        {
                            mesh_tmp.setMaterial(mtl[i]);
                        }
                    }
                }
                else
                {
                    List<Material> mtl = materials_dict["Default"];
                    for (int i = 0; i < mtl.Count; i++)
                    {
                        if (mtl[i].Name == "Default")
                        {
                            mesh_tmp.setMaterial(mtl[i]);
                        }
                    }
                }


                if (mesh_count == 1)
                {
                    mesh = mesh_tmp;
                }
                else
                {
                    mesh.child.Add(mesh_tmp);
                }

                mesh_created++;
            }
            return mesh;
        }
        public List<Material> LoadMtlFile(string path)
        {
            Console.WriteLine("Load MTL file");
            List<Material> materials = new List<Material>();
            List<string> name = new List<string>();
            List<float> shininess = new List<float>();
            List<Vector3> ambient = new List<Vector3>();
            List<Vector3> diffuse = new List<Vector3>();
            List<Vector3> specular = new List<Vector3>();
            List<float> alpha = new List<float>();
            List<string> map_kd = new List<string>();
            List<string> map_ka = new List<string>();

            //komputer ngecek, apakah file bisa diopen atau tidak
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            //lanjut ke sini
            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    words.RemoveAll(s => s == string.Empty);

                    if (words.Count == 0)
                        continue;

                    string type = words[0];

                    words.RemoveAt(0);
                    switch (type)
                    {
                        case "newmtl":
                            if (map_kd.Count < name.Count)
                            {
                                map_kd.Add("white.jpg");
                            }
                            if (map_ka.Count < name.Count)
                            {
                                map_ka.Add("white.jpg");
                            }
                            name.Add(words[0]);
                            break;
                        //Shininess
                        case "Ns":
                            shininess.Add(float.Parse(words[0]));
                            break;
                        case "Ka":
                            ambient.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "Kd":
                            diffuse.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "Ks":
                            specular.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "d":
                            alpha.Add(float.Parse(words[0]));
                            break;
                        case "map_Kd":
                            map_kd.Add(words[0]);
                            break;
                        case "map_Ka":
                            map_ka.Add(words[0]);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (map_kd.Count < name.Count)
            {
                map_kd.Add("white.jpg");
            }
            if (map_ka.Count < name.Count)
            {
                map_ka.Add("white.jpg");
            }

            Dictionary<string, Texture> texture_map_Kd = new Dictionary<string, Texture>();
            for (int i = 0; i < map_kd.Count; i++)
            {
                if (!texture_map_Kd.ContainsKey(map_kd[i]))
                {
                    Console.WriteLine("List of map_Kd key: " + map_kd[i]);
                    texture_map_Kd.Add(map_kd[i],
                        Texture.LoadFromFile("../../../Resources/" + map_kd[i]));
                }
            }

            Dictionary<string, Texture> texture_map_Ka = new Dictionary<string, Texture>();
            for (int i = 0; i < map_ka.Count; i++)
            {
                if (!texture_map_Ka.ContainsKey(map_ka[i]))
                {
                    texture_map_Ka.Add(map_ka[i],
                        Texture.LoadFromFile("../../../Resources/" + map_ka[i]));
                }
            }

            for (int i = 0; i < name.Count; i++)
            {
                materials.Add(new Material(name[i], shininess[i], ambient[i], diffuse[i], specular[i],
                    alpha[i], texture_map_Kd[map_kd[i]], texture_map_Ka[map_ka[i]]));
            }

            return materials;
        }

        //CubeMap
        public void CreateCubeMap()
        {
            string[] skyboxPath =
            {
                "../../../Resources/nightSky.jpg",
                "../../../Resources/nightSky.jpg",
                "../../../Resources/nightSky.jpg",
                "../../../Resources/nightSky.jpg",
                "../../../Resources/nightSky.jpg",
                "../../../Resources/nightSky.jpg",
            };
            GL.GenTextures(1, out cubemap);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap);
            Console.WriteLine("Cubemap: " + cubemap);
            for (int i = 0; i < skyboxPath.Length; i++)
            {
                using (var image = new Bitmap(skyboxPath[i]))
                {
                    Console.WriteLine(skyboxPath[i] + " LOADED");

                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                        0,
                        PixelInternalFormat.Rgb,
                        1280,
                        1280,
                        0,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);
                }
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            }


        }
        //Animation
        public void LampRevolution()
        {
            //lights[0].Position = lamp0.getTransform().ExtractTranslation();
            //lamp0.rotate(0f, 1.0f, 0.0f);
            //lights[1].Position = lamp1.getTransform().ExtractTranslation();
            //lamp1.rotate(0f, -1f, 0.0f);
            ////lights[2].Position = lamp2.getTransform().ExtractTranslation();
            ////lamp2.rotate(1f, 0f, 0f);
        }
    }
}