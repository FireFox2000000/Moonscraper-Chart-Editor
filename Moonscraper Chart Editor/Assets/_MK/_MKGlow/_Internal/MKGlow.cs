///////////////////////////////////////////////
// MK Glow	    							 //
//											 //
// Created by Michael Kremmel                //
// www.michaelkremmel.de                     //
// Copyright © 2015 All rights reserved.     //
///////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

namespace MKGlowSystem
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class MKGlow : MonoBehaviour
    {
        #region Get/Set
        private GameObject GlowCameraObject
        {
            get
            {
                if (!glowCameraObject)
                {
                    glowCameraObject = new GameObject("glowCameraObject");
                    glowCameraObject.hideFlags = HideFlags.HideAndDontSave;
                    glowCameraObject.AddComponent<Camera>();
                    GlowCamera.orthographic = false;
                    GlowCamera.enabled = false;
                    GlowCamera.renderingPath = RenderingPath.VertexLit;
					GlowCamera.hideFlags = HideFlags.HideAndDontSave;
                }
                return glowCameraObject;
            }
        }
        private Camera GlowCamera
        {
            get
            {
                if (glowCamera == null)
                {
                    glowCamera = GlowCameraObject.GetComponent<Camera>();
                }
                return glowCamera;
            }
        }
        public LayerMask GlowLayer
        {
            get { return glowLayer; }
            set { glowLayer = value; }
        }
        public bool ShowCutoutGlow
        {
            get { return showCutoutGlow; }
            set { showCutoutGlow = value; }
        }
        public bool ShowTransparentGlow
        {
            get { return showTransparentGlow; }
            set { showTransparentGlow = value; }
        }
        public MKGlowType GlowType
        {
            get { return glowType; }
            set { glowType = value; }
        }
        public Color GlowTint
        {
            get { return fullScreenGlowTint; }
            set { fullScreenGlowTint = value; }
        }
        public int Samples
        {
            get { return samples; }
            set { samples = value; }
        }
        public int BlurIterations
        {
            get { return blurIterations; }
            set
            {
                blurIterations = Mathf.Clamp(value, 0, 10);
            }
        }
        public float GlowIntensity
        {
            get { return glowIntensity; }
            set { glowIntensity = value; }
        }
        public float BlurSpread
        {
            get { return blurSpread; }
            set { blurSpread = value; }
        }

        private Material BlurMaterial
        {
            get
            {
                if (blurMaterial == null)
                {
                    blurMaterial = new Material(blurShader);
                    blurMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return blurMaterial;
            }
        }

        private Material CompositeMaterial
        {
            get
            {
                if (compositeMaterial == null)
                {
                    compositeMaterial = new Material(compositeShader);
                    compositeMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return compositeMaterial;
            }
        }
        #endregion


        #region Constants
        private static float[] gaussFilter = new float[11]
		{
			0.402f,0.623f,0.877f,1.120f,1.297f,1.362f,1.297f,1.120f,0.877f,0.623f,0.402f
		};
        #endregion


        #region shaders
        [SerializeField]
        private Shader blurShader;
        [SerializeField]
        private Shader compositeShader;
        [SerializeField]
        private Shader glowRenderShader;
        #endregion


        #region privates
        private Material compositeMaterial;
        private Material blurMaterial;

        private Camera glowCamera;
        private GameObject glowCameraObject;
        private RenderTexture glowTexture;

        [SerializeField]
        [Tooltip("recommend: -1")]
        private LayerMask glowLayer = -1;

        [SerializeField]
        private Camera renderCamera;

        [SerializeField]
        [Tooltip("Show glow through Cutout rendered objects")]
        private bool showCutoutGlow = false;
        [SerializeField]
        [Tooltip("Show glow through Transparent rendered objects")]
        private bool showTransparentGlow = true;
        [SerializeField]
        [Tooltip("Selective = to specifically bring objects to glow, Fullscreen = complete screen glows")]
        private MKGlowType glowType = MKGlowType.Selective;
        [SerializeField]
        [Tooltip("The glows coloration in full screen mode (only FullscreenGlowType)")]
        private Color fullScreenGlowTint = new Color(1, 1, 1, 0);
        [SerializeField]
        [Tooltip("Width of the glow effect")]
        private float blurSpread = 0.25f;
        [SerializeField]
        [Tooltip("Number of used blurs")]
        private int blurIterations = 7;
        [SerializeField]
        [Tooltip("The global luminous intensity")]
        private float glowIntensity = 0.6f;
        [SerializeField]
        [Tooltip("Significantly influences the blurs quality")]
        private int samples = 3;
        #endregion

        private void Main()
        {
            if (glowRenderShader == null)
            {
                enabled = false;
                Debug.LogWarning("Failed to load MKGlow Render Shader");
                return;
            }
            if (compositeShader == null)
            {
                enabled = false;
                Debug.LogWarning("Failed to load MKGlow Composite Shader");
                return;
            }

            if (blurShader == null)
            {
                enabled = false;
                Debug.LogWarning("Failed to load MKGlow Blur Shader");
                return;
            }

            if (renderCamera == null)
            {
                enabled = false;
                Debug.LogWarning("Failed to load render camera");
                return;
            }

            if(!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Default) || !SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
            {
                enabled = false;
                Debug.LogWarning("Glow not supported by platform");
                return;
            }
        }
        
        private void Reset()
        {
            glowLayer = -1;
            SetupShaders();
            if (renderCamera == null)
                renderCamera = GetComponent<Camera>();
        }
        
        private void OnEnable()
        {
            SetupShaders();
            if (renderCamera == null)
                renderCamera = GetComponent<Camera>();
        }

        private void SetupKeywords()
        {
            if (ShowTransparentGlow)
            {
                Shader.EnableKeyword("MKTRANSPARENT_ON");
                Shader.DisableKeyword("MKTRANSPARENT_OFF");
            }
            else
            {
                Shader.DisableKeyword("MKTRANSPARENT_ON");
                Shader.EnableKeyword("MKTRANSPARENT_OFF");
            }
            if (ShowCutoutGlow)
            {
                Shader.EnableKeyword("MKCUTOUT_ON");
                Shader.DisableKeyword("MKCUTOUT_OFF");
            }
            else
            {
                Shader.DisableKeyword("MKCUTOUT_ON");
                Shader.EnableKeyword("MKCUTOUT_OFF");
            }
        }

        private void SetupShaders()
        {
            if (!blurShader)
                blurShader = Shader.Find("Hidden/MKGlowBlur");

            if (!compositeShader)
                compositeShader = Shader.Find("Hidden/MKGlowCompose");

            if (!glowRenderShader)
                glowRenderShader = Shader.Find("Hidden/MKGlowRender");
        }

        private void OnDisable()
        {
            if (compositeMaterial)
            {
                DestroyImmediate(compositeMaterial);
            }
            if (blurMaterial)
            {
                DestroyImmediate(blurMaterial);
            }

            if (glowCamera)
                DestroyImmediate(GlowCamera);

            if (glowCameraObject)
                DestroyImmediate(GlowCameraObject);

            if (glowTexture)
            {
                RenderTexture.ReleaseTemporary(glowTexture);
                DestroyImmediate(glowTexture);
            }
        }

        private void SetupGlowCamera()
        {
            GlowCamera.CopyFrom(GetComponent<Camera>());
            GlowCamera.clearFlags = CameraClearFlags.SolidColor;
            GlowCamera.rect = new Rect(0, 0, 1, 1);
            GlowCamera.backgroundColor = new Color(0, 0, 0, 0);
            GlowCamera.cullingMask = glowLayer;
            GlowCamera.targetTexture = glowTexture;
            if (GlowCamera.actualRenderingPath != RenderingPath.VertexLit)
                GlowCamera.renderingPath = RenderingPath.VertexLit;
        }

        private void OnPreRender()
        {
            if (!gameObject.activeSelf || !enabled)
                return;

            if (glowTexture != null)
            {
                RenderTexture.ReleaseTemporary(glowTexture);
                glowTexture = null;
            }

            if (GlowType == MKGlowType.Selective)
            {
                glowTexture = RenderTexture.GetTemporary((int)((GetComponent<Camera>().pixelWidth) / samples), (int)((GetComponent<Camera>().pixelHeight) / samples), 16, RenderTextureFormat.Default);
                SetupGlowCamera();
                SetupKeywords();
                GlowCamera.RenderWithShader(glowRenderShader, "RenderType");
            }
            else
            {
                if (GlowCamera)
                    DestroyImmediate(GlowCamera);
                if (GlowCameraObject)
                    DestroyImmediate(GlowCameraObject);
            }

            BlurMaterial.color = new Color(fullScreenGlowTint.r, fullScreenGlowTint.g, fullScreenGlowTint.b, glowIntensity);
        }

        protected virtual void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!gameObject.activeSelf || !enabled)
                return;

            if (GlowType == MKGlowType.Selective)
            {
                PerformSelectiveGlow(ref src, ref dest);
            }
            else
            {
                PerformFullScreenGlow(ref src, ref dest);
            }
        }

        private void PerformBlur(ref RenderTexture src, ref RenderTexture dest)
        {
            float off = BlurSpread;
            blurMaterial.SetTexture("_MainTex", src);
            blurMaterial.SetFloat("_Shift", off);
            Graphics.Blit(src, dest, blurMaterial);
        }

        private void PerformBlur(ref RenderTexture src, ref RenderTexture dest, int iteration)
        {
            float offset =  iteration * BlurSpread;
            offset *= gaussFilter[iteration];

            blurMaterial.SetTexture("_MainTex", src);
            blurMaterial.SetFloat("_Shift", offset);
            Graphics.Blit(src, dest, blurMaterial);
        }

        private void PerformGlow(ref RenderTexture glowBuffer, ref RenderTexture dest, ref RenderTexture src)
        {
            CompositeMaterial.SetTexture("_GlowTex", src);
            Graphics.Blit(glowBuffer, dest, CompositeMaterial);
        }

        protected void PerformSelectiveGlow(ref RenderTexture source, ref RenderTexture dest)
        {
            Vector2 TextureSize;
            TextureSize.x = source.width / Samples;
            TextureSize.y = source.height / Samples;

            RenderTexture glowBuffer = RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 0, RenderTextureFormat.Default);

            PerformBlur(ref glowTexture, ref glowBuffer);

            for (int i = 0; i < BlurIterations; i++)
            {
                RenderTexture glowBufferSecond = RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 0, RenderTextureFormat.Default);
                PerformBlur(ref glowBuffer, ref glowBufferSecond, i);
                RenderTexture.ReleaseTemporary(glowBuffer);
                glowBuffer = glowBufferSecond;
            }
            
            PerformGlow(ref glowBuffer, ref dest, ref source);

            RenderTexture.ReleaseTemporary(glowBuffer);

            if (glowTexture != null)
            {
                RenderTexture.ReleaseTemporary(glowTexture);
                glowTexture = null;
            }
        }

        protected void PerformFullScreenGlow(ref RenderTexture source, ref RenderTexture destination)
        {
            Vector2 TextureSize;
            TextureSize.x = source.width / Samples;
            TextureSize.y = source.height / Samples;
            RenderTexture glowBuffer = RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 0, RenderTextureFormat.Default);

            PerformBlur(ref source, ref glowBuffer);

            for (int i = 0; i < BlurIterations; i++)
            {
                RenderTexture glowBufferSecond = RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 0, RenderTextureFormat.Default);
                PerformBlur(ref glowBuffer, ref glowBufferSecond, i);
                RenderTexture.ReleaseTemporary(glowBuffer);
                glowBuffer = glowBufferSecond;
            }
            Graphics.Blit(source, destination);

            PerformGlow(ref glowBuffer, ref destination, ref source);

            RenderTexture.ReleaseTemporary(glowBuffer);
        }
    }

    public enum MKGlowType
    {
        Selective,
        Fullscreen,
    }
}