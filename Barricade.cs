using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public class Barricade {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int   HP    { get; private set; } = 800;
        public int   MaxHP { get; private set; } = 800;
        public bool  IsAlive => HP > 0;

        // 90 fokkal elfordítva – magasabb mint széles
        public const float HalfW = 22f;
        public const float HalfH = 56f;

        private float hitFlashTimer = 0f;
        private const float HitFlashDuration = 0.12f;

        public Barricade(float x, float y) {
            X = x; Y = y;
        }

        public void TakeDamage(int dmg) {
            HP = Math.Max(0, HP - dmg);
            hitFlashTimer = HitFlashDuration;
        }

        public void Update(float dt) {
            if (hitFlashTimer > 0) hitFlashTimer -= dt;
        }

        /// <summary>Zombi ütközés – blokkolja a zombit.</summary>
        public bool CheckCollision(float zx, float zy, float radius) {
            float dx = MathF.Abs(zx - X);
            float dy = MathF.Abs(zy - Y);
            return dx < (HalfW + radius) && dy < (HalfH + radius);
        }

        /// <summary>Golyó ÁTMEGY a barrikádon (mindig false).</summary>
        public bool CheckBulletCollision(float bx, float by) {
            return false;
        }

        public void Draw() {
            if (!IsAlive) return;

            float hpPct = (float)HP / MaxHP;
            int w = (int)(HalfW * 2), h = (int)(HalfH * 2);
            int left = (int)(X - HalfW), top = (int)(Y - HalfH);

            bool flashing = hitFlashTimer > 0;

            float d = hpPct;
            Color woodDark  = flashing ? Color.Red :
                new Color((byte)(100*d+25), (byte)(60*d+12), (byte)(25*d+5), (byte)255);
            Color wood = flashing ? new Color(255, 80, 80, 255) :
                new Color((byte)(140*d+35), (byte)(90*d+18), (byte)(43*d+8), (byte)255);
            Color woodLight = new Color((byte)(180*d+25), (byte)(120*d+15), (byte)(60*d+8), (byte)255);
            Color nail = new Color(200, 200, 180, 255);

            // Alap téglatest
            Raylib.DrawRectangle(left, top, w, h, woodDark);

            // 3 FÜGGŐLEGES deszka oszlop (90 fokkal elforgatva)
            int colW = w / 3;
            for (int col = 0; col < 3; col++) {
                int cx = left + col * colW;
                Raylib.DrawRectangle(cx + 1, top + 1, colW - 2, h - 2, wood);
                Raylib.DrawLine(cx, top, cx, top + h, woodDark);
                Raylib.DrawLine(cx + 1, top + 1, cx + 1, top + h - 1, woodLight);
            }

            // Vízszintes tartógerendák (volt: függőleges oszlopok)
            foreach (int py in new[]{ top + h/4, top + h/2, top + 3*h/4 }) {
                Raylib.DrawRectangle(left, py - 3, w, 6, woodDark);
                Raylib.DrawRectangle(left + 1, py - 2, w - 2, 2, woodLight);
            }

            // Szegecs sarkok
            foreach (int nx in new[]{ left+3, left+w-6 })
                foreach (int ny in new[]{ top+4, top+h/4-3, top+h/2-3, top+3*h/4-3, top+h-7 })
                    Raylib.DrawRectangle(nx, ny, 4, 4, nail);

            // Keret
            Raylib.DrawRectangleLinesEx(new Rectangle(left, top, w, h), 2, new Color(60, 35, 10, 255));

            // Repedések károsodás szerint
            Color crk1 = new Color(40, 20, 5, 200);
            Color crk2 = new Color(20, 8, 0, 240);
            if (hpPct < 0.75f) {
                Raylib.DrawLine(left + 2,      top + h/4,    left + colW,    top + h/4+8,  crk1);
                Raylib.DrawLine(left + w-3,    top + 3*h/4,  left + w-colW,  top + 3*h/4-6, crk1);
            }
            if (hpPct < 0.5f) {
                Raylib.DrawLine(left + 3,      top + h/2-4,  left + w-3,     top + h/2+5,  crk2);
                Raylib.DrawLine(left + colW,   top + 5,      left + 2*colW,  top + h/3,    crk2);
                Raylib.DrawLine(left + 2,      top + 3*h/4,  left + colW,    top + h-4,    crk2);
            }
            if (hpPct < 0.25f) {
                Raylib.DrawLine(left + 2,      top + 2,      left + w-2,     top + h/3,    crk2);
                Raylib.DrawLine(left + 2,      top + h/3,    left + w/2-2,   top + h/2,    crk2);
                Raylib.DrawLine(left + 4,      top + 2*h/3,  left + w-2,     top + h-3,    crk2);
                Raylib.DrawRectangle(left-1,   top + h/4-3, 4, 8, new Color(22,22,30,255));
                Raylib.DrawRectangle(left+w-2, top + 3*h/4, 4, 7, new Color(22,22,30,255));
            }

            // HP sáv
            Color hpCol = hpPct > 0.5f  ? new Color(80, 200, 80, 220) :
                          hpPct > 0.25f ? new Color(230, 150, 30, 220) :
                                          new Color(220, 40, 40, 220);
            Raylib.DrawRectangle(left - 6, top, 3, h, new Color(20, 20, 20, 180));
            Raylib.DrawRectangle(left - 6, top + (int)(h * (1f - hpPct)), 3, (int)(h * hpPct), hpCol);
        }

        public static void DrawIcon(int x, int y, int size, Color tint) {
            // Függőleges ikon (90 fokkal elfordítva: magasabb mint széles)
            int hw = (int)(size * 0.55f), hh = size;
            int left = x - hw/2, top = y - hh/2;
            Color wood = new Color(
                (byte)(139 * tint.R / 255f), (byte)(90 * tint.G / 255f),
                (byte)(43  * tint.B / 255f), tint.A);
            Color dark  = new Color((byte)60,  (byte)35,  (byte)10,  tint.A);
            Color light = new Color((byte)200, (byte)150, (byte)80,  tint.A);
            Raylib.DrawRectangle(left, top, hw, hh, dark);
            int dw = hw / 3;
            for (int i = 0; i < 3; i++)
                Raylib.DrawRectangle(left + i*dw + 1, top+1, dw-1, hh-2, wood);
            // Vízszintes tartók
            Raylib.DrawRectangle(left, top + hh/3, hw, 2, dark);
            Raylib.DrawRectangle(left, top + 2*hh/3, hw, 2, dark);
            Raylib.DrawLine(left, top, left, top+hh, light);
            Raylib.DrawLine(left, top, left+hw, top, light);
            Raylib.DrawRectangleLinesEx(new Rectangle(left, top, hw, hh), 1, dark);
        }
    }
}
