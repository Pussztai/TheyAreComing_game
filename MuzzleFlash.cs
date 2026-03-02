using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    /// <summary>
    /// Muzzle flash effekt a fegyver csövének végénél.
    /// Kódból rajzolt sugarak + sprite sheet overlay kombinációja.
    /// Garantáltan látszódik minden fegyvernél.
    /// </summary>
    public class MuzzleFlash {

        private Texture2D? spriteTexture;
        private bool       spriteLoaded = false;

        // Sprite sheet: 4 oszlop × 5 sor = 20 frame, egyenként 10×8 px
        private const int Cols        = 4;
        private const int Rows        = 5;
        private const int TotalFrames = Cols * Rows;
        private const int FrameW      = 10;
        private const int FrameH      = 8;

        private int   spriteFrame  = 0;
        private float spriteTimer  = 0f;
        private const float FrameSpeed = 0.035f;

        // Flash state
        private bool  isPlaying   = false;
        private float flashX, flashY;
        private float angle;           // radián
        private float flashScale;      // fegyver méret szorzó

        private float lifetime    = 0f;   // összidő eltelt
        private const float MaxLife = 0.12f;  // 120ms – gyors villanás

        public MuzzleFlash(string texturePath) {
            try {
                spriteTexture = Raylib.LoadTexture(texturePath);
                spriteLoaded  = spriteTexture.Value.Id != 0;
            } catch {
                spriteLoaded = false;
            }
        }

        public void Trigger(float x, float y, float aimAngle, float weaponScale = 1f) {
            flashX       = x;
            flashY       = y;
            angle        = aimAngle;
            flashScale   = weaponScale;
            isPlaying    = true;
            lifetime     = 0f;
            spriteFrame  = 0;
            spriteTimer  = 0f;
        }

        public void Update(float deltaTime) {
            if (!isPlaying) return;

            lifetime    += deltaTime;
            spriteTimer += deltaTime;

            if (spriteTimer >= FrameSpeed) {
                spriteTimer = 0f;
                spriteFrame = Math.Min(spriteFrame + 1, TotalFrames - 1);
            }

            if (lifetime >= MaxLife)
                isPlaying = false;
        }

        public void Draw() {
            if (!isPlaying) return;

            float t       = 1f - (lifetime / MaxLife);   // 1.0 → 0.0 (fade out)
            float baseSize = 18f * flashScale;

            float cosA = MathF.Cos(angle);
            float sinA = MathF.Sin(angle);

            // ── 1. Belső mag – fehér kör ─────────────────────────────────────
            byte coreAlpha = (byte)(255 * t);
            float coreR    = baseSize * 0.45f;
            Raylib.DrawCircle((int)flashX, (int)flashY, coreR,
                new Color((byte)255, (byte)255, (byte)200, coreAlpha));

            // ── 2. Külső narancssárga glóriás kör ───────────────────────────
            byte glowAlpha = (byte)(160 * t);
            Raylib.DrawCircle((int)flashX, (int)flashY, baseSize * 0.85f,
                new Color((byte)255, (byte)140, (byte)0, glowAlpha));

            // ── 3. Fősugar előre (cső irányába) ─────────────────────────────
            float rayLen = baseSize * 2.2f;
            float rayW   = baseSize * 0.55f;
            DrawRay(flashX, flashY, cosA, sinA, rayLen, rayW,
                    new Color((byte)255, (byte)220, (byte)100, (byte)(230 * t)));

            // ── 4. Keresztsugar (merőleges) ──────────────────────────────────
            float crossLen = baseSize * 1.1f;
            float crossW   = baseSize * 0.28f;
            DrawRay(flashX, flashY, -sinA, cosA, crossLen, crossW,
                    new Color((byte)255, (byte)180, (byte)50, (byte)(170 * t)));
            DrawRay(flashX, flashY,  sinA, -cosA, crossLen, crossW,
                    new Color((byte)255, (byte)180, (byte)50, (byte)(170 * t)));

            // ── 5. Kis szikrák random irányban ───────────────────────────────
            float seed = lifetime * 1000f;
            for (int i = 0; i < 5; i++) {
                float sa = angle + (((seed * (i + 1) * 7919f) % 628) / 100f - 3.14f) * 0.8f;
                float sd = baseSize * 0.6f + ((seed * (i + 3) * 6271f) % (baseSize));
                float sx = flashX + MathF.Cos(sa) * sd;
                float sy = flashY + MathF.Sin(sa) * sd;
                Raylib.DrawCircle((int)sx, (int)sy, 2.5f * flashScale,
                    new Color((byte)255, (byte)200, (byte)80, (byte)(180 * t)));
            }

            // ── 6. Sprite sheet overlay (additive) – ha betöltött ───────────
            if (spriteLoaded && spriteTexture.HasValue) {
                int col = spriteFrame % Cols;
                int row = spriteFrame / Cols;
                Rectangle src  = new Rectangle(col * FrameW, row * FrameH, FrameW, FrameH);
                float sw       = FrameW * flashScale * 5f;
                float sh       = FrameH * flashScale * 5f;
                Rectangle dest = new Rectangle(flashX, flashY, sw, sh);
                Vector2 orig   = new Vector2(sw / 2f, sh / 2f);
                float   deg    = angle * (180f / MathF.PI);

                Raylib.BeginBlendMode(BlendMode.Additive);
                Raylib.DrawTexturePro(spriteTexture.Value, src, dest, orig, deg,
                    new Color((byte)255, (byte)255, (byte)255, (byte)(200 * t)));
                Raylib.EndBlendMode();
            }
        }

        /// <summary>Egy sugár (téglalap) rajzolása irány + szélesség alapján.</summary>
        private static void DrawRay(float ox, float oy,
                                    float dx, float dy,
                                    float length, float width, Color color) {
            // Merőleges vektor a szélességhez
            float px = -dy, py = dx;

            // 4 sarokpont
            Vector2 p0 = new Vector2(ox + px * width / 2f,        oy + py * width / 2f);
            Vector2 p1 = new Vector2(ox - px * width / 2f,        oy - py * width / 2f);
            Vector2 p2 = new Vector2(ox + dx * length - px * width / 2f,
                                     oy + dy * length - py * width / 2f);
            Vector2 p3 = new Vector2(ox + dx * length + px * width / 2f,
                                     oy + dy * length + py * width / 2f);

            Raylib.DrawTriangle(p0, p1, p2, color);
            Raylib.DrawTriangle(p0, p2, p3, color);
        }

        public bool IsPlaying => isPlaying;

        public void Unload() {
            if (spriteLoaded && spriteTexture.HasValue)
                Raylib.UnloadTexture(spriteTexture.Value);
        }
    }
}
