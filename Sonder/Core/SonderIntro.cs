using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Sonder.Core
{
    
    internal static class SonderAssetLoader
    {
        public static byte[] Load(string fileName)
        {
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                foreach (string res in asm.GetManifestResourceNames())
                {
                    if (!res.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) continue;

                    using (System.IO.Stream st = asm.GetManifestResourceStream(res))
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        st.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
                Debug.LogWarning("[Aurora] " + fileName + " not found in assets (put it in the assets folder and rebuild)");
            }
            catch (Exception ex) { Debug.LogWarning("[Aurora] Could not load " + fileName + ": " + ex.Message); }
            return null;
        }
    }

    public class SonderIntroUI : MonoBehaviour
    {
        static bool autoPlayed;
        IEnumerator PlayWhenLoaded()
        {
            if (autoPlayed) yield break;
            while (GorillaTagger.Instance == null) yield return null;
            yield return new WaitForSeconds(0.5f);
            autoPlayed = true;
            Play();
        }
        const float Duration = 12f;
        const float FadeIn = 0.9f;
        const float FadeOut = 1.3f;
        const float SpinTime = 1.6f;
        const float FlyStart = 1.3f;
        const float FlyLen = 0.75f;
        const float DotStep = 0.26f;
        const int BubbleCount = 60;
        const int FogCount = 8;
        const int SparkCount = 40;
        const int StreakCount = 12;
        const float FogStrength = 1.25f;
        const float VignetteStrength = 1.15f;
        const float GlowStrength = 1.35f;
        const float SwayAmount = 1.5f;

        public static event Action Finished;
        static SonderIntroUI instance;

        public static bool IsPlaying
        {
            get { return instance != null && instance.playing; }
        }

        public static bool Exists
        {
            get { return instance != null; }
        }

        public static void Play()
        {
            if (instance == null || instance.playing) return;
            instance.t = 0f;
            instance.playing = true;
            instance.PlayIntroAudio();
        }

        Texture2D logo, logoGlowTex, bubbleTex, fogTex, glowTex, vignetteTex, sparkTex, streakTex;
        GUIStyle letterStyle, textStyle, haloStyle, titleStyle, subStyle, progressStyle;
        float t;
        bool playing;

        const float VrPanelWidth = 2.4f;
        const float VrPanelHeight = 1.4f;
        const float VrPanelDistance = 1.15f;

        GameObject vrRoot;
        Transform vrCamera;
        readonly List<Material> vrMaterials = new List<Material>();
        Material vrBackgroundMat;
        Material vrGlowMat;
        Material vrFogMat;
        Material vrVignetteMat;
        Material vrBubbleMat;
        Material vrLogoGlowMat;
        Material vrLogoMat;
        Material vrTextGlowMat;
        Renderer vrBackgroundRenderer;
        Renderer vrMainGlowRenderer;
        Renderer vrVignetteRenderer;
        Renderer vrLogoGlowRenderer;
        Renderer vrLogoRenderer;
        Renderer vrTextGlowRenderer;
        Renderer[] vrFogRenderers;
        Renderer[] vrBubbleRenderers;
        Transform[] vrFogTransforms;
        Transform[] vrBubbleTransforms;
        Transform vrLogoGlowTransform;
        Transform vrLogoTransform;
        TextMesh vrLetterText;
        TextMesh vrLoadingText;
        byte[] introAudioData;
        IntPtr introAudioBuffer = IntPtr.Zero;
        float introAudioReleaseAt;
        bool nativeAudioPlaying;

        const uint SoundAsync = 0x0001;
        const uint SoundNoDefault = 0x0002;
        const uint SoundMemory = 0x0004;

        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", SetLastError = true)]
        static extern bool PlaySoundNative(IntPtr sound, IntPtr module, uint flags);

        class Bubble { public float x, size, speed, phase, wobble; }
        class Spark { public float x, y, size, phase, twinkle; }
        class Streak { public float y, speed, length, phase, alpha; }
        readonly List<Bubble> bubbles = new List<Bubble>();
        readonly List<Spark> sparks = new List<Spark>();
        readonly List<Streak> streaks = new List<Streak>();

        byte[] LoadIntroAudio()
        {
            try
            {
                byte[] waveData = SonderAssetLoader.Load("intro.wav");
                if (waveData == null) return null;

                if (waveData.Length < 44 ||
                    Encoding.ASCII.GetString(waveData, 0, 4) != "RIFF" ||
                    Encoding.ASCII.GetString(waveData, 8, 4) != "WAVE")
                {
                    Debug.LogWarning("Aurora intro audio is not a WAV file.");
                    return null;
                }

                int pos = 12;
                int format = 0, bitsPerSample = 0, sampleRate = 0, channels = 0, dataSize = 0;
                while (pos + 8 <= waveData.Length)
                {
                    string id = Encoding.ASCII.GetString(waveData, pos, 4);
                    int size = (int)Math.Min(BitConverter.ToUInt32(waveData, pos + 4), (uint)(waveData.Length - pos - 8));
                    if (id == "fmt " && size >= 16)
                    {
                        format = BitConverter.ToUInt16(waveData, pos + 8);
                        channels = BitConverter.ToUInt16(waveData, pos + 10);
                        sampleRate = BitConverter.ToInt32(waveData, pos + 12);
                        bitsPerSample = BitConverter.ToUInt16(waveData, pos + 22);
                    }
                    else if (id == "data") { dataSize = size; break; }
                    pos += 8 + size + (size & 1);
                }

                if (format != 1 || channels < 1 || sampleRate < 1 || bitsPerSample != 16 || dataSize <= 0)
                {
                    Debug.LogWarning("Aurora intro.wav must be uncompressed 16-bit PCM WAV.");
                    return null;
                }
                return waveData;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Aurora intro audio could not be loaded: " + ex.Message);
                return null;
            }
        }

        void PlayIntroAudio()
        {
            if (introAudioData == null || introAudioData.Length < 44)
                return;
            if (Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                Debug.LogWarning("Embedded intro audio playback currently requires the Windows PC build.");
                return;
            }

            StopIntroAudio();
            try
            {
                introAudioBuffer = Marshal.AllocHGlobal(introAudioData.Length);
                Marshal.Copy(introAudioData, 0, introAudioBuffer, introAudioData.Length);
                if (!PlaySoundNative(introAudioBuffer, IntPtr.Zero, SoundAsync | SoundNoDefault | SoundMemory))
                {
                    int error = Marshal.GetLastWin32Error();
                    StopIntroAudio();
                    Debug.LogWarning("Windows could not start the Aurora intro sound. Error: " + error);
                    return;
                }

                float duration = Duration;
                uint byteRate = BitConverter.ToUInt32(introAudioData, 28);
                if (byteRate > 0) duration = introAudioData.Length / (float)byteRate;
                introAudioReleaseAt = Time.unscaledTime + duration + 0.5f;
                nativeAudioPlaying = true;
            }
            catch (Exception ex)
            {
                StopIntroAudio();
                Debug.LogWarning("Windows could not play the Aurora intro sound: " + ex.Message);
            }
        }

        void StopIntroAudio()
        {
            if (nativeAudioPlaying &&
                (Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.WindowsEditor))
            {
                try { PlaySoundNative(IntPtr.Zero, IntPtr.Zero, 0); }
                catch { }
            }

            nativeAudioPlaying = false;
            introAudioReleaseAt = 0f;
            if (introAudioBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(introAudioBuffer);
                introAudioBuffer = IntPtr.Zero;
            }
        }

        void Awake()
        {
            instance = this;
			if (!autoPlayed) StartCoroutine(PlayWhenLoaded());
            introAudioData = LoadIntroAudio();

            logo = MakeLogo(512);
            logoGlowTex = MakeLogoGlow(256);
            bubbleTex = MakeRadial(64, r => r >= 1f ? 0f : r > 0.88f ? Mathf.InverseLerp(1f, 0.88f, r) : 0.18f);
            fogTex = MakeRadial(128, r => Mathf.Pow(1f - Mathf.Clamp01(r), 2f) * 0.4f);
            glowTex = MakeRadial(128, r => Mathf.Pow(1f - Mathf.Clamp01(r), 1.4f) * 0.85f);
            vignetteTex = MakeRadial(128, r => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.4f, r)) * 0.95f);
            sparkTex = MakeSpark(48);
            streakTex = MakeStreak(256, 8);

            var rng = new System.Random(1337);
            for (int i = 0; i < BubbleCount; i++)
            {
                bubbles.Add(new Bubble
                {
                    x = (float)rng.NextDouble(),
                    size = 6f + (float)rng.NextDouble() * 26f,
                    speed = 0.08f + (float)rng.NextDouble() * 0.18f,
                    phase = (float)rng.NextDouble(),
                    wobble = 6f + (float)rng.NextDouble() * 24f
                });
            }
            for (int i = 0; i < SparkCount; i++)
            {
                sparks.Add(new Spark
                {
                    x = (float)rng.NextDouble(),
                    y = (float)rng.NextDouble(),
                    size = 2f + (float)rng.NextDouble() * 6f,
                    phase = (float)rng.NextDouble(),
                    twinkle = 0.5f + (float)rng.NextDouble() * 1.5f
                });
            }
            for (int i = 0; i < StreakCount; i++)
            {
                streaks.Add(new Streak
                {
                    y = (float)rng.NextDouble(),
                    speed = 0.15f + (float)rng.NextDouble() * 0.35f,
                    length = 0.15f + (float)rng.NextDouble() * 0.45f,
                    phase = (float)rng.NextDouble(),
                    alpha = 0.15f + (float)rng.NextDouble() * 0.35f
                });
            }
        }

        void Update()
        {
            if (nativeAudioPlaying && Time.unscaledTime >= introAudioReleaseAt)
                StopIntroAudio();

            if (!playing)
            {
                if (vrRoot != null && vrRoot.activeSelf)
                    vrRoot.SetActive(false);
                return;
            }

            t += Time.unscaledDeltaTime;
            Camera camera = Camera.main;
            if (camera != null && camera.stereoEnabled)
            {
                EnsureVRDisplay(camera);
                if (vrRoot != null)
                {
                    vrRoot.SetActive(true);
                    UpdateVRDisplay();
                }
            }
            else if (vrRoot != null && vrRoot.activeSelf)
            {
                vrRoot.SetActive(false);
            }

            if (t >= Duration)
            {
                playing = false;
                if (vrRoot != null)
                    vrRoot.SetActive(false);
                Finished?.Invoke();
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            DestroyVRDisplay();
            StopIntroAudio();
            if (logo) Destroy(logo);
            if (logoGlowTex) Destroy(logoGlowTex);
            if (bubbleTex) Destroy(bubbleTex);
            if (fogTex) Destroy(fogTex);
            if (glowTex) Destroy(glowTex);
            if (vignetteTex) Destroy(vignetteTex);
            if (sparkTex) Destroy(sparkTex);
            if (streakTex) Destroy(streakTex);
        }

        void OnGUI()
        {
            Event ev = Event.current;

            if (!playing || ev.type != EventType.Repaint || IsVRActive()) return;

            if (letterStyle == null)
            {
                letterStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                letterStyle.normal.textColor = new Color(0.09f, 0.37f, 0.62f);
                textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.BoldAndItalic };
                textStyle.normal.textColor = new Color(0.61f, 0.78f, 0.94f);
                textStyle.padding = new RectOffset(20, 0, 0, 0);
                haloStyle = new GUIStyle(textStyle);
                haloStyle.normal.textColor = Color.white;
                titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                titleStyle.normal.textColor = new Color(0.78f, 0.92f, 1f);
                subStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic };
                subStyle.normal.textColor = new Color(0.55f, 0.78f, 0.95f);
                progressStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                progressStyle.normal.textColor = new Color(0.85f, 0.95f, 1f);
            }

            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / FadeIn)) *
                         Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Duration - t) / FadeOut));

            GUI.depth = -1000;
            float W = Screen.width, H = Screen.height;

            // light glass background gradient
            for (int i = 0; i < 6; i++)
            {
                float y = i * H / 5f;
                float a = 0.95f - i * 0.08f;
                GUI.color = new Color(0.78f + i * 0.03f, 0.90f + i * 0.015f, 0.98f, fade * a);
                GUI.DrawTexture(new Rect(0, y, W, H / 5f + 2f), Texture2D.whiteTexture);
            }

            // ambient nebula glow
            GUI.color = new Color(0.18f, 0.32f, 0.55f, Mathf.Clamp01(fade * FogStrength * 0.55f));
            GUI.DrawTexture(new Rect(W / 2f - W * 0.85f, H * 0.55f - H * 0.7f, W * 1.7f, H * 1.4f), glowTex);

            GUI.color = new Color(0.35f, 0.55f, 0.85f, Mathf.Clamp01(fade * FogStrength * 0.35f));
            for (int i = 0; i < FogCount; i++)
            {
                float w = W * (0.55f + 0.12f * (i % 5)), h = H * 0.4f;
                float cx = W * Mathf.Repeat(i * 0.618f, 1f) + Mathf.Cos(t * 0.35f + i) * 90f;
                float cy = H * (0.08f + Mathf.Repeat(i * 0.382f + 0.15f, 1f) * 0.85f) + Mathf.Sin(t * 0.25f + i * 1.7f) * 30f;
                GUI.DrawTexture(new Rect(cx - w / 2f, cy - h / 2f, w, h), fogTex);
            }

            // light streaks
            for (int i = 0; i < streaks.Count; i++)
            {
                Streak s = streaks[i];
                float prog = Mathf.Repeat(t * s.speed + s.phase, 1f);
                float x = (prog - 0.5f) * W * 1.4f + W * 0.5f;
                float y = s.y * H;
                float len = s.length * W;
                float a = s.alpha * fade * (0.6f + 0.4f * Mathf.Sin(t * 2f + s.phase * 6f));
                GUI.color = new Color(0.6f, 0.85f, 1f, Mathf.Clamp01(a));
                GUI.DrawTexture(new Rect(x - len / 2f, y - 2f, len, 4f), streakTex);
            }

            // vignette
            GUI.color = new Color(0.02f, 0.05f, 0.08f, fade * VignetteStrength);
            GUI.DrawTexture(new Rect(0, 0, W, H), vignetteTex);

            // bubbles
            GUI.color = new Color(0.62f, 0.82f, 1f, 0.9f * fade);
            foreach (var b in bubbles)
            {
                float y = H - ((t * b.speed + b.phase) % 1f) * (H + 80f);
                float x = b.x * W + Mathf.Sin(t * 1.5f + b.phase * 6.28f) * b.wobble;
                GUI.DrawTexture(new Rect(x, y, b.size, b.size), bubbleTex);
            }

            // sparks / stars
            for (int i = 0; i < sparks.Count; i++)
            {
                Spark sp = sparks[i];
                float tw = 0.5f + 0.5f * Mathf.Sin(t * sp.twinkle * 3f + sp.phase * 6.28f);
                float a = tw * fade * 0.9f;
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                float sz = sp.size * (0.7f + 0.3f * tw);
                GUI.DrawTexture(new Rect(sp.x * W - sz / 2f, sp.y * H - sz / 2f, sz, sz), sparkTex);
            }

            // logo
            float size = H * 0.34f;
            var logoRect = new Rect(W / 2f - size / 2f, H / 2f - size / 2f - H * 0.08f, size, size);
            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(t / SpinTime * 360f, logoRect.center);

            float pulse = 0.82f + 0.18f * Mathf.Sin(t * 2.4f);
            float gs = size * (6.5f / 5.12f);
            // chromatic glow layers
            GUI.color = new Color(0.4f, 0.75f, 1f, Mathf.Clamp01(fade * GlowStrength * pulse * 0.4f));
            GUI.DrawTexture(new Rect(logoRect.center.x - gs / 2f - 2f, logoRect.center.y - gs / 2f, gs, gs), logoGlowTex);
            GUI.color = new Color(0.6f, 0.85f, 1f, Mathf.Clamp01(fade * GlowStrength * pulse * 0.4f));
            GUI.DrawTexture(new Rect(logoRect.center.x - gs / 2f + 2f, logoRect.center.y - gs / 2f, gs, gs), logoGlowTex);
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(fade * GlowStrength * pulse * 0.85f));
            GUI.DrawTexture(new Rect(logoRect.center.x - gs / 2f, logoRect.center.y - gs / 2f, gs, gs), logoGlowTex);

            GUI.color = new Color(1f, 1f, 1f, fade);
            GUI.DrawTexture(logoRect, logo);
            GUI.matrix = old;

            letterStyle.fontSize = (int)(size * 0.34f);
            float letterScale = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.3f) / 0.6f));
            Matrix4x4 oldLetter = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(letterScale, letterScale), logoRect.center);
            GUI.color = new Color(1f, 1f, 1f, fade);
            GUI.Label(logoRect, "S", letterStyle);
            GUI.matrix = oldLetter;

            // title reveal: SONDER
            string title = "SONDER";
            int revealed = Mathf.Clamp((int)((t - 0.4f) / 0.18f), 0, title.Length);
            string shown = title.Substring(0, revealed);
            titleStyle.fontSize = (int)(H * 0.075f);
            float titleY = H / 2f + size / 2f + H * 0.02f;
            // glow
            GUI.color = new Color(0.4f, 0.75f, 1f, Mathf.Clamp01(fade * 0.4f * GlowStrength));
            GUI.Label(new Rect(W / 2f - W * 0.4f, titleY - 2f, W * 0.8f, titleStyle.fontSize * 1.4f), shown, titleStyle);
            GUI.color = new Color(1f, 1f, 1f, fade);
            GUI.Label(new Rect(W / 2f - W * 0.4f, titleY, W * 0.8f, titleStyle.fontSize * 1.4f), shown, titleStyle);

            // subtitle
            float subAlpha = Mathf.Clamp01((t - 1.6f) / 0.6f);
            subStyle.fontSize = (int)(H * 0.026f);
            GUI.color = new Color(0.55f, 0.78f, 0.95f, fade * subAlpha);
            GUI.Label(new Rect(W / 2f - W * 0.4f, titleY + titleStyle.fontSize * 1.3f, W * 0.8f, subStyle.fontSize * 1.4f),
                "Sonder  \u2022  Mod Menu", subStyle);

            // loading text with fly-in
            float k = Mathf.Clamp01((t - FlyStart) / FlyLen);
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float e = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
            float yOff = (1f - e) * H;

            textStyle.fontSize = (int)(H * 0.032f);
            haloStyle.fontSize = textStyle.fontSize;
            int dots = 1 + Mathf.Max(0, (int)((t - FlyStart) / DotStep)) % 3;
            string label = "Loading Sonder" + new string('.', dots);
            float fullW = textStyle.CalcSize(new GUIContent("Loading Sonder...")).x + 40f;

            float sw = k * SwayAmount;
            float swayX = Mathf.Sin(t * 1.1f) * 12f * sw;
            float swayY = Mathf.Sin(t * 1.9f) * 4f * sw;
            float swayAngle = Mathf.Sin(t * 1.5f) * 2.5f * sw;

            float ty = H / 2f + size / 2f + H * 0.18f + yOff + swayY;
            var textRect = new Rect(W / 2f - fullW / 2f + swayX, ty, fullW, textStyle.fontSize * 1.6f);

            Matrix4x4 oldText = GUI.matrix;
            GUIUtility.RotateAroundPivot(swayAngle, textRect.center);

            float gw = fullW * 1.6f, gh = textStyle.fontSize * 3.4f;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(0.25f * fade * GlowStrength));
            GUI.DrawTexture(new Rect(textRect.center.x - gw / 2f, textRect.center.y - gh / 2f, gw, gh), glowTex);

            for (int ring = 0; ring < 2; ring++)
            {
                float r = textStyle.fontSize * (ring == 0 ? 0.05f : 0.11f);
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01((ring == 0 ? 0.16f : 0.09f) * fade * GlowStrength));
                for (int a = 0; a < 8; a++)
                {
                    float ang = a * Mathf.PI / 4f;
                    var o = new Rect(textRect.x + Mathf.Cos(ang) * r, textRect.y + Mathf.Sin(ang) * r, textRect.width, textRect.height);
                    GUI.Label(o, label, haloStyle);
                }
            }

            GUI.color = new Color(1f, 1f, 1f, fade);
            GUI.Label(textRect, label, textStyle);
            GUI.matrix = oldText;

            // progress bar
            float progress = Mathf.Clamp01(t / Duration);
            float barW = W * 0.42f;
            float barH = H * 0.012f;
            float barX = W / 2f - barW / 2f;
            float barY = ty + textStyle.fontSize * 2.2f;
            // track
            GUI.color = new Color(0.15f, 0.25f, 0.35f, 0.6f * fade);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
            // fill
            GUI.color = new Color(0.45f, 0.78f, 1f, 0.95f * fade);
            GUI.DrawTexture(new Rect(barX, barY, barW * progress, barH), Texture2D.whiteTexture);
            // shimmer
            float shimmerX = barX + (barW * progress) - 20f + Mathf.Sin(t * 6f) * 8f;
            GUI.color = new Color(1f, 1f, 1f, 0.5f * fade);
            GUI.DrawTexture(new Rect(shimmerX, barY - 2f, 24f, barH + 4f), glowTex);
            // percent
            progressStyle.fontSize = (int)(H * 0.016f);
            GUI.color = new Color(0.7f, 0.88f, 1f, fade);
            GUI.Label(new Rect(barX, barY + barH + 4f, barW, progressStyle.fontSize * 1.4f),
                Mathf.RoundToInt(progress * 100f) + "%", progressStyle);

            GUI.color = Color.white;
        }

        bool IsVRActive()
        {
            Camera camera = Camera.main;
            return camera != null && camera.stereoEnabled;
        }

        void EnsureVRDisplay(Camera camera)
        {
            if (vrRoot != null && vrCamera == camera.transform)
                return;

            DestroyVRDisplay();
            vrCamera = camera.transform;
            vrRoot = new GameObject("AuroraIntro_VR_Display");
            vrRoot.transform.SetParent(vrCamera, false);
            vrRoot.transform.localPosition = Vector3.zero;
            vrRoot.transform.localRotation = Quaternion.identity;
            vrRoot.transform.localScale = Vector3.one;

            vrBackgroundMat = CreateVRMaterial(Texture2D.whiteTexture);
            vrGlowMat = CreateVRMaterial(glowTex);
            vrFogMat = CreateVRMaterial(fogTex);
            vrVignetteMat = CreateVRMaterial(vignetteTex);
            vrBubbleMat = CreateVRMaterial(bubbleTex);
            vrLogoGlowMat = CreateVRMaterial(logoGlowTex);
            vrLogoMat = CreateVRMaterial(logo);
            vrTextGlowMat = CreateVRMaterial(glowTex);

            vrBackgroundRenderer = CreateVRQuad("Background", vrBackgroundMat,
                new Vector2(VrPanelWidth, VrPanelHeight), VrPanelDistance);
            vrMainGlowRenderer = CreateVRQuad("Ambient_Glow", vrGlowMat,
                new Vector2(VrPanelWidth * 1.4f, VrPanelHeight * 1.15f), VrPanelDistance - 0.015f);

            vrFogRenderers = new Renderer[FogCount];
            vrFogTransforms = new Transform[FogCount];
            for (int i = 0; i < FogCount; i++)
            {
                vrFogRenderers[i] = CreateVRQuad("Fog_" + i, vrFogMat,
                    new Vector2(VrPanelWidth * (0.6f + 0.08f * (i % 6)), VrPanelHeight * 0.45f),
                    VrPanelDistance - 0.03f);
                if (vrFogRenderers[i] != null)
                    vrFogTransforms[i] = vrFogRenderers[i].transform;
            }

            vrVignetteRenderer = CreateVRQuad("Vignette", vrVignetteMat,
                new Vector2(VrPanelWidth, VrPanelHeight), VrPanelDistance - 0.045f);

            vrBubbleRenderers = new Renderer[BubbleCount];
            vrBubbleTransforms = new Transform[BubbleCount];
            for (int i = 0; i < BubbleCount; i++)
            {
                vrBubbleRenderers[i] = CreateVRQuad("Bubble_" + i, vrBubbleMat,
                    Vector2.one * 0.02f, VrPanelDistance - 0.06f);
                if (vrBubbleRenderers[i] != null)
                    vrBubbleTransforms[i] = vrBubbleRenderers[i].transform;
            }

            float logoSize = VrPanelHeight * 0.4f;
            vrLogoGlowRenderer = CreateVRQuad("Logo_Glow", vrLogoGlowMat,
                Vector2.one * (logoSize * (6.5f / 5.12f)), VrPanelDistance - 0.075f);
            vrLogoRenderer = CreateVRQuad("Logo", vrLogoMat,
                Vector2.one * logoSize, VrPanelDistance - 0.09f);
            if (vrLogoGlowRenderer != null)
                vrLogoGlowTransform = vrLogoGlowRenderer.transform;
            if (vrLogoRenderer != null)
                vrLogoTransform = vrLogoRenderer.transform;

            vrLetterText = CreateVRText("Logo_Letter", "S", 0.2f, FontStyle.Normal, 20);
            vrLoadingText = CreateVRText("Loading_Text", "Loading Sonder...", 0.07f,
                FontStyle.BoldAndItalic, 21);
            vrTextGlowRenderer = CreateVRQuad("Loading_Text_Glow", vrTextGlowMat,
                new Vector2(VrPanelWidth * 0.62f, VrPanelHeight * 0.16f), VrPanelDistance - 0.12f);
        }

        Material CreateVRMaterial(Texture texture)
        {
            Shader shader = Shader.Find("Sprites/Default") ??
                            Shader.Find("Unlit/Transparent") ??
                            Shader.Find("Unlit/Texture") ??
                            Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogWarning("Aurora intro could not find a transparent shader for its VR display.");
                return null;
            }

            Material material = new Material(shader);
            material.mainTexture = texture != null ? texture : Texture2D.whiteTexture;
            material.renderQueue = 3000;
            vrMaterials.Add(material);
            return material;
        }

        Renderer CreateVRQuad(string name, Material material, Vector2 size, float z)
        {
            if (vrRoot == null || material == null)
                return null;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            quad.transform.SetParent(vrRoot.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, z);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);

            Renderer renderer = quad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            return renderer;
        }

        TextMesh CreateVRText(string name, string text, float characterSize, FontStyle style, int sortingOrder)
        {
            if (vrRoot == null)
                return null;
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(vrRoot.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, VrPanelDistance - 0.13f);

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = style;

            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                Material textMaterial = renderer.material;
                if (textMaterial != null)
                {
                    textMaterial.renderQueue = 3002;
                    vrMaterials.Add(textMaterial);
                }
            }
            return textMesh;
        }

        void UpdateVRDisplay()
        {
            if (vrRoot == null)
                return;

            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / FadeIn)) *
                          Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Duration - t) / FadeOut));
            if (vrBackgroundMat != null)
                vrBackgroundMat.color = new Color(0.03f, 0.09f, 0.12f, fade);
            if (vrGlowMat != null)
                vrGlowMat.color = new Color(0.28f, 0.5f, 0.72f, Mathf.Clamp01(fade * FogStrength));
            if (vrFogMat != null)
                vrFogMat.color = new Color(0.32f, 0.55f, 0.78f, Mathf.Clamp01(fade * FogStrength));
            if (vrVignetteMat != null)
                vrVignetteMat.color = new Color(0.015f, 0.04f, 0.06f, Mathf.Clamp01(fade * VignetteStrength));
            if (vrBubbleMat != null)
                vrBubbleMat.color = new Color(0.62f, 0.82f, 1f, fade * 0.9f);

            float logoSize = VrPanelHeight * 0.4f;
            float logoY = VrPanelHeight * 0.028f;
            float pulse = 0.85f + 0.15f * Mathf.Sin(t * 2f);
            if (vrLogoMat != null)
                vrLogoMat.color = new Color(1f, 1f, 1f, fade);
            if (vrLogoGlowMat != null)
                vrLogoGlowMat.color = new Color(1f, 1f, 1f,
                    Mathf.Clamp01(fade * GlowStrength * pulse * 0.75f));
            if (vrLogoGlowTransform != null)
                vrLogoGlowTransform.localPosition = new Vector3(0f, logoY, VrPanelDistance - 0.075f);
            if (vrLogoTransform != null)
            {
                vrLogoTransform.localPosition = new Vector3(0f, logoY, VrPanelDistance - 0.09f);
                vrLogoTransform.localRotation = Quaternion.Euler(0f, 0f, t / SpinTime * 360f);
            }

            for (int i = 0; i < FogCount; i++)
            {
                Transform fog = vrFogTransforms != null && i < vrFogTransforms.Length ? vrFogTransforms[i] : null;
                if (fog == null) continue;
                float nx = Mathf.Repeat(i * 0.618f, 1f);
                float ny = 0.05f + Mathf.Repeat(i * 0.381f, 0.9f);
                float x = (nx - 0.5f) * VrPanelWidth + Mathf.Cos(t * 0.14f + i) * 0.06f;
                float y = (0.5f - ny) * VrPanelHeight + Mathf.Sin(t * 0.18f + i * 0.7f) * 0.035f;
                fog.localPosition = new Vector3(x, y, VrPanelDistance - 0.03f);
            }

            for (int i = 0; i < bubbles.Count && i < BubbleCount; i++)
            {
                Transform bubbleTransform = vrBubbleTransforms != null && i < vrBubbleTransforms.Length
                    ? vrBubbleTransforms[i]
                    : null;
                if (bubbleTransform == null) continue;
                Bubble bubble = bubbles[i];
                float progress = Mathf.Repeat(t * bubble.speed + bubble.phase, 1f);
                float x = (bubble.x - 0.5f) * VrPanelWidth +
                          Mathf.Sin(t * 0.8f + bubble.phase * 6f) * bubble.wobble * (VrPanelWidth / 1920f);
                float y = -VrPanelHeight * 0.5f + progress * VrPanelHeight;
                float size = Mathf.Max(0.012f, bubble.size * VrPanelHeight / 1080f);
                bubbleTransform.localPosition = new Vector3(x, y, VrPanelDistance - 0.06f);
                bubbleTransform.localScale = new Vector3(size, size, 1f);
            }

            float k = Mathf.Clamp01((t - FlyStart) / FlyLen);
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float e = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
            float yOff = (1f - e) * VrPanelHeight;
            int dots = 1 + Mathf.Max(0, (int)((t - FlyStart) / DotStep)) % 3;
            string label = "Loading Sonder" + new string('.', dots);
            float sw = k * SwayAmount;
            float swayX = Mathf.Sin(t * 1.1f) * 12f * sw * (VrPanelWidth / 1920f);
            float swayY = Mathf.Sin(t * 1.9f) * 4f * sw * (VrPanelHeight / 1080f);
            float swayAngle = Mathf.Sin(t * 1.5f) * 2.5f * sw;
            float textY = -logoSize * 0.5f - VrPanelHeight * (10f / 1080f) - yOff + swayY;

            if (vrLetterText != null)
            {
                vrLetterText.color = new Color(0.09f, 0.37f, 0.62f, fade);
                vrLetterText.transform.localPosition = new Vector3(0f, logoY, VrPanelDistance - 0.105f);
            }
            if (vrLoadingText != null)
            {
                vrLoadingText.text = label;
                vrLoadingText.color = new Color(0.61f, 0.78f, 0.94f, fade);
                vrLoadingText.transform.localPosition = new Vector3(swayX, textY, VrPanelDistance - 0.13f);
                vrLoadingText.transform.localRotation = Quaternion.Euler(0f, 0f, swayAngle);
            }
            if (vrTextGlowMat != null)
                vrTextGlowMat.color = new Color(1f, 1f, 1f, Mathf.Clamp01(0.22f * fade * GlowStrength));
            if (vrTextGlowRenderer != null)
            {
                vrTextGlowRenderer.transform.localPosition = new Vector3(swayX, textY, VrPanelDistance - 0.12f);
                vrTextGlowRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, swayAngle);
            }
        }

        void DestroyVRDisplay()
        {
            if (vrRoot != null)
                Destroy(vrRoot);
            vrRoot = null;
            vrCamera = null;

            for (int i = 0; i < vrMaterials.Count; i++)
            {
                if (vrMaterials[i] != null)
                    Destroy(vrMaterials[i]);
            }
            vrMaterials.Clear();

            vrBackgroundMat = null;
            vrGlowMat = null;
            vrFogMat = null;
            vrVignetteMat = null;
            vrBubbleMat = null;
            vrLogoGlowMat = null;
            vrLogoMat = null;
            vrTextGlowMat = null;
            vrBackgroundRenderer = null;
            vrMainGlowRenderer = null;
            vrVignetteRenderer = null;
            vrLogoGlowRenderer = null;
            vrLogoRenderer = null;
            vrTextGlowRenderer = null;
            vrFogRenderers = null;
            vrBubbleRenderers = null;
            vrFogTransforms = null;
            vrBubbleTransforms = null;
            vrLogoGlowTransform = null;
            vrLogoTransform = null;
            vrLetterText = null;
            vrLoadingText = null;
        }

        static readonly Color32 CWhite = new Color32(255, 255, 255, 255);
        static readonly Color32 CSquare = new Color32(0, 0, 0, 0);
        static readonly Color32 CSquareBorder = new Color32(0, 0, 0, 0);
        static readonly Color32 CLine = new Color32(0x26, 0x70, 0xB0, 255);
        static readonly Color32 CTriangle = new Color32(0x1A, 0x48, 0x75, 255);
        static readonly Color32 CClear = new Color32(0, 0, 0, 0);

        static Color32 LogoColor(float x, float y)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            float s = ax + ay, m = Mathf.Max(ax, ay);

            if (s <= 2.44f) return CWhite;
            if (m <= 2.54f) return m <= 2.50f ? CSquare : CSquareBorder;
            if (s <= 4.22f)
            {
                if (s >= 4.22f - 0.113f || ax < 0.04f || ay < 0.04f) return CLine;
                return CTriangle;
            }
            return CClear;
        }

        static Texture2D MakeLogo(int n)
        {
            const float R = 5.12f;
            const int ss = 3;
            var px = new Color32[n * n];

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    int count = 0, r = 0, g = 0, b = 0;
                    for (int sy = 0; sy < ss; sy++)
                    {
                        for (int sx = 0; sx < ss; sx++)
                        {
                            float u = ((x + (sx + 0.5f) / ss) / n * 2f - 1f) * R;
                            float v = ((y + (sy + 0.5f) / ss) / n * 2f - 1f) * R;
                            Color32 c = LogoColor(u, v);
                            if (c.a == 0) continue;
                            count++; r += c.r; g += c.g; b += c.b;
                        }
                    }
                    if (count > 0)
                        px[y * n + x] = new Color32((byte)(r / count), (byte)(g / count), (byte)(b / count),
                                                    (byte)(255 * count / (ss * ss)));
                }
            }

            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeLogoGlow(int n)
        {
            const float R = 6.5f;
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float u = ((x + 0.5f) / n * 2f - 1f) * R;
                    float v = ((y + 0.5f) / n * 2f - 1f) * R;
                    float s = Mathf.Abs(u) + Mathf.Abs(v);
                    float a = s <= 4.22f ? 0.75f : 0.75f * Mathf.Exp(-(s - 4.22f) / 1.4142f * 2.5f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255f));
                }
            }
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeRadial(int n, Func<float, float> alphaByRadius)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f;
                    float dy = (y + 0.5f) / n * 2f - 1f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alphaByRadius(r)));
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D MakeSpark(int n)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float c = n / 2f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = x - c, dy = y - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) / c;
                    float ang = Mathf.Atan2(dy, dx);
                    float spike = Mathf.Pow(Mathf.Abs(Mathf.Cos(ang * 4f)), 8f);
                    float a = (1f - Mathf.Clamp01(r)) * (0.35f + 0.65f * spike);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(a)));
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D MakeStreak(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dy = Mathf.Abs((y + 0.5f) / h * 2f - 1f);
                    float v = Mathf.Pow(1f - Mathf.Clamp01(dy), 3f);
                    float edge = Mathf.SmoothStep(0f, 1f, (float)x / w) * Mathf.SmoothStep(0f, 1f, 1f - (float)x / w);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, v * edge));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}

