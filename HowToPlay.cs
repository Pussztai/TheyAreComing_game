using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class HowToPlay {
        private const int W = 800;
        private const int H = 600;

        // 0 = English, 1 = Magyar
        private int lang = 0;

        private Rectangle btnEN;
        private Rectangle btnHU;
        private Rectangle btnExit;

        // Scroll
        private float scrollY    = 0f;
        private const float ScrollSpeed = 28f;
        private const float ContentHeight = 1460f;  // teljes tartalom magassága

        public bool ExitClicked { get; private set; } = false;

        public HowToPlay() {
            btnEN   = new Rectangle(220, 36, 100, 30);
            btnHU   = new Rectangle(330, 36, 100, 30);
            btnExit = new Rectangle(W - 130, H - 46, 110, 32);
        }

        public void Open() {
            ExitClicked = false;
            scrollY     = 0f;
            lang        = 0;
        }

        public void Update() {
            Vector2 mp = Raylib.GetMousePosition();

            if (Raylib.CheckCollisionPointRec(mp, btnEN) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                lang = 0; scrollY = 0f;
            }
            if (Raylib.CheckCollisionPointRec(mp, btnHU) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                lang = 1; scrollY = 0f;
            }
            if (Raylib.CheckCollisionPointRec(mp, btnExit) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                ExitClicked = true;
            }

            // Scroll
            float wheel = Raylib.GetMouseWheelMove();
            scrollY -= wheel * ScrollSpeed * 4f;

            if (Raylib.IsKeyDown(KeyboardKey.Down)) scrollY += ScrollSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.Up))   scrollY -= ScrollSpeed;

            float maxScroll = Math.Max(0f, ContentHeight - (H - 80));
            scrollY = Math.Clamp(scrollY, 0f, maxScroll);
        }

        public void Draw() {
            Raylib.ClearBackground(Color.Black);

            // ── fejléc (fix, nem scrollozik) ────────────────────────────────
            string title = "HOW TO PLAY";
            Raylib.DrawText(title, W / 2 - Raylib.MeasureText(title, 28) / 2, 8, 28, Color.Red);
            Raylib.DrawLine(0, 38, W, 38, new Color(60, 60, 60, 255));

            // Lang gombok
            DrawTabBtn(btnEN, "ENGLISH", lang == 0);
            DrawTabBtn(btnHU, "MAGYAR",  lang == 1);

            Raylib.DrawLine(0, 72, W, 72, new Color(60, 60, 60, 255));

            // ── scrollozható tartalom területe ──────────────────────────────
            Raylib.BeginScissorMode(0, 73, W, H - 73 - 50);

            int y = 73 - (int)scrollY;

            if (lang == 0) DrawEnglish(ref y);
            else           DrawMagyar(ref y);

            Raylib.EndScissorMode();

            // ── Scrollbar ───────────────────────────────────────────────────
            float viewH      = H - 73 - 50;
            float totalH     = ContentHeight;
            float scrollRatio = scrollY / Math.Max(1f, totalH - viewH);
            float barH        = viewH * (viewH / totalH);
            int   barY        = 73 + (int)(scrollRatio * (viewH - barH));
            Raylib.DrawRectangle(W - 6, 73, 6, (int)viewH, new Color(30, 30, 30, 255));
            Raylib.DrawRectangle(W - 6, barY, 6, (int)barH, new Color(100, 100, 100, 255));

            // ── Alsó sáv (fix) ───────────────────────────────────────────────
            Raylib.DrawRectangle(0, H - 50, W, 50, new Color(10, 10, 10, 255));
            Raylib.DrawLine(0, H - 50, W, H - 50, new Color(60, 60, 60, 255));

            // Scroll hint
            Raylib.DrawText("SCROLL: mouse wheel / arrow keys",
                20, H - 33, 14, new Color(70, 70, 70, 255));

            // EXIT gomb
            Vector2 mp = Raylib.GetMousePosition();
            bool overExit = Raylib.CheckCollisionPointRec(mp, btnExit);
            Raylib.DrawRectangleRec(btnExit, overExit ? new Color(180, 30, 30, 255) : new Color(120, 20, 20, 255));
            Raylib.DrawRectangleLinesEx(btnExit, 1, new Color(200, 50, 50, 255));
            string et = "< BACK";
            Raylib.DrawText(et,
                (int)btnExit.X + (int)btnExit.Width  / 2 - Raylib.MeasureText(et, 16) / 2,
                (int)btnExit.Y + (int)btnExit.Height / 2 - 8,
                16, Color.White);
        }

        // ── Tab gomb ──────────────────────────────────────────────────────────
        private void DrawTabBtn(Rectangle b, string text, bool active) {
            Color bg     = active ? new Color(60, 60, 60, 255) : new Color(20, 20, 20, 255);
            Color border = active ? new Color(160, 160, 160, 255) : new Color(60, 60, 60, 255);
            Color txtCol = active ? Color.White : new Color(100, 100, 100, 255);
            Raylib.DrawRectangleRec(b, bg);
            Raylib.DrawRectangleLinesEx(b, 1, border);
            int tw = Raylib.MeasureText(text, 14);
            Raylib.DrawText(text,
                (int)b.X + (int)b.Width  / 2 - tw / 2,
                (int)b.Y + (int)b.Height / 2 - 7,
                14, txtCol);
        }

        // ── Segédek ───────────────────────────────────────────────────────────
        private static Color C(byte r, byte g, byte b) => new Color(r, g, b, (byte)255);
        private static Color DIM  = C(100, 100, 100);
        private static Color MID  = C(160, 160, 160);
        private static Color LITE = C(210, 210, 210);
        private static Color RED  = C(200, 50,  50);
        private static Color WHT  = Color.White;

        private void SectionTitle(string text, ref int y) {
            y += 18;
            Raylib.DrawText(text, 20, y, 18, RED);
            Raylib.DrawLine(20, y + 22, W - 20, y + 22, C(50, 50, 50));
            y += 30;
        }

        private void Row(string label, string value, ref int y, int indent = 30) {
            Raylib.DrawText(label, indent, y, 15, MID);
            if (value != "") {
                int lw = Raylib.MeasureText(label, 15);
                Raylib.DrawText(value, indent + lw + 6, y, 15, LITE);
            }
            y += 20;
        }

        private void BulletRow(string text, ref int y, int indent = 30) {
            Raylib.DrawText("-", indent, y, 15, DIM);
            Raylib.DrawText(text, indent + 14, y, 15, LITE);
            y += 20;
        }

        private void KeyRow(string key, string desc, ref int y) {
            // key box
            int kw = Raylib.MeasureText(key, 14) + 12;
            Raylib.DrawRectangle(30, y - 1, kw, 20, C(30, 30, 30));
            Raylib.DrawRectangleLinesEx(new Rectangle(30, y - 1, kw, 20), 1, C(80, 80, 80));
            Raylib.DrawText(key, 36, y + 1, 14, WHT);
            Raylib.DrawText(desc, 30 + kw + 10, y + 1, 14, MID);
            y += 24;
        }

        private void WaveRow(string waveLabel, string desc, ref int y, bool highlight = false) {
            Color wc = highlight ? RED : DIM;
            Raylib.DrawText(waveLabel, 30, y, 15, wc);
            int lw = Raylib.MeasureText(waveLabel, 15);
            Raylib.DrawText(desc, 30 + lw + 10, y, 15, highlight ? LITE : MID);
            y += 22;
        }

        private void EnemyRow(string name, string hp, string speed, string dmg, string reward, ref int y) {
            Raylib.DrawText(name,   30,  y, 14, LITE);
            Raylib.DrawText("HP:"  + hp,   130, y, 14, DIM);
            Raylib.DrawText("SPD:" + speed, 220, y, 14, DIM);
            Raylib.DrawText("DMG:" + dmg,   310, y, 14, DIM);
            Raylib.DrawText("$"    + reward, 400, y, 14, DIM);
            y += 20;
        }

        private void WeaponRow(string name, string cost, string desc, ref int y) {
            Raylib.DrawText(name, 30,  y, 14, LITE);
            Raylib.DrawText(cost, 160, y, 14, DIM);
            Raylib.DrawText(desc, 260, y, 14, DIM);
            y += 20;
        }

        // ═════════════════════════════════════════════════════════════════════
        private void DrawEnglish(ref int y) {

            // CONTROLS
            SectionTitle("CONTROLS", ref y);
            KeyRow("W A S D",   "Move your soldier", ref y);
            KeyRow("LMB",       "Shoot toward cursor", ref y);
            KeyRow("R",         "Reload weapon", ref y);
            KeyRow("B",         "Place barricade (if you have charges)", ref y);
            KeyRow("MOUSE",     "Aim direction", ref y);

            // WAVE SYSTEM
            SectionTitle("WAVE SYSTEM", ref y);
            WaveRow("Wave  1:", "Normal zombies only. Low count, slow spawns. Learn the basics.", ref y);
            WaveRow("Wave  2:", "More zombies, faster spawns. Fast zombies may appear.", ref y);
            WaveRow("Wave  3:", "UPGRADE SELECT + WEAPON SHOP unlocks for the first time!", ref y, true);
            WaveRow("Wave  4:", "Tanks join. Higher HP, heavy barricade damage.", ref y);
            WaveRow("Wave  5:", "BOSS spawns. 300+ HP, 30 damage, $100 reward.", ref y, true);
            WaveRow("Wave  6:", "Another Upgrade Select + Weapon Shop.", ref y, true);
            WaveRow("Wave  9:", "Another Upgrade Select + Weapon Shop.", ref y, true);
            WaveRow("Wave  N:", "Zombie HP scales x1.25 per wave. Endless survival.", ref y);
            y += 6;
            Raylib.DrawText("Every 3rd wave triggers Upgrade Select + Weapon Shop.", 30, y, 14, DIM);
            y += 22;
            Raylib.DrawText("Wave bonus gold: wave number x $25 at end of each wave.", 30, y, 14, DIM);
            y += 26;

            // ENEMIES
            SectionTitle("ENEMIES", ref y);
            Raylib.DrawText("NAME         HP     SPD    DMG    REWARD", 30, y, 13, DIM);
            Raylib.DrawLine(30, y + 17, W - 30, y + 17, C(40, 40, 40));
            y += 22;
            EnemyRow("Normal",  " 50",  " 42", " 10", "10",  ref y);
            EnemyRow("Fast",    " 30",  " 70", "  8", "15",  ref y);
            EnemyRow("Tank",    "150",  " 35", " 20", "30",  ref y);
            EnemyRow("Boss",    "300+", " 36", " 30", "100", ref y);
            y += 6;
            BulletRow("All zombies CRAWL at 50% HP: speed -60%, smaller, less damage.", ref y);
            BulletRow("HEADSHOT = 2x damage. Aim at the top of the zombie sprite.", ref y);
            BulletRow("Boss spawns every 5th wave (wave 5, 10, 15...) with chance.", ref y);

            // WEAPONS
            SectionTitle("WEAPONS", ref y);
            Raylib.DrawText("NAME         COST   DETAILS", 30, y, 13, DIM);
            Raylib.DrawLine(30, y + 17, W - 30, y + 17, C(40, 40, 40));
            y += 22;
            WeaponRow("Pistol",   "Free",  "12 ammo  |  dmg 20  |  cooldown 0.35s  |  reload 1.5s",  ref y);
            WeaponRow("SMG",      "$150",  "30 ammo  |  dmg 10  |  cooldown 0.10s  |  reload 2.0s",  ref y);
            WeaponRow("Shotgun",  "$200",  " 8 ammo  |  dmg 14  |  5 pellets/shot  |  reload 2.5s",  ref y);
            WeaponRow("M4 Rifle", "$350",  "25 ammo  |  dmg 35  |  cooldown 0.18s  |  reload 2.2s",  ref y);
            WeaponRow("Sniper",   "$500",  " 5 ammo  |  dmg 120 |  cooldown 1.20s  |  reload 3.0s",  ref y);
            y += 6;
            BulletRow("You keep your weapon between waves.", ref y);
            BulletRow("Weapon shop opens after every Upgrade Select (every 3rd wave).", ref y);

            // BARRICADES
            SectionTitle("BARRICADES", ref y);
            BulletRow("Press B to place a barricade in front of your soldier.", ref y);
            BulletRow("800 HP. Zombies stop and attack it instead of you.", ref y);
            BulletRow("Cracks appear as it takes damage. Bullets pass through.", ref y);
            BulletRow("Barricade charges come from Upgrades.", ref y);

            // UPGRADES
            SectionTitle("UPGRADES (every 3rd wave)", ref y);
            BulletRow("Pick 1 of 3 random upgrades each time.", ref y);
            BulletRow("Examples: extra barricade charges, max HP boost,", ref y);
            BulletRow("lifesteal on hit, faster reload, damage multiplier.", ref y);

            // META SHOP
            SectionTitle("GAME OVER  /  META SHOP", ref y);
            BulletRow("On death, session gold is added to your permanent total.", ref y);
            BulletRow("+5% Damage per level  (cost: $30, +$15/level)", ref y);
            BulletRow("+10 Max HP per level  (cost: $25, +$10/level)", ref y);
            BulletRow("+5% Speed per level   (cost: $35, +$20/level)", ref y);
            BulletRow("Bonuses carry over to every future run.", ref y);

            // TIPS
            SectionTitle("TIPS", ref y);
            BulletRow("Use barricades to create safe reload windows.", ref y);
            BulletRow("Always aim for the head against Tanks and Bosses.", ref y);
            BulletRow("Save money early to reach Rifle or Sniper by wave 3.", ref y);
            BulletRow("Focus fire on the Boss immediately — its damage adds up fast.", ref y);
            BulletRow("Invest permanent upgrades in Damage first — affects everything.", ref y);

            y += 20;
        }

        // ═════════════════════════════════════════════════════════════════════
        private void DrawMagyar(ref int y) {

            // IRÁNYÍTÁS
            SectionTitle("IRÁNYÍTÁS", ref y);
            KeyRow("W A S D",   "Katona mozgatása", ref y);
            KeyRow("BAL KATT",  "Lövés a kurzor felé", ref y);
            KeyRow("R",         "Újratöltés", ref y);
            KeyRow("B",         "Barrikád lehelyezése (ha van tölteted)", ref y);
            KeyRow("EGÉR",      "Célzás iránya", ref y);

            // HULLÁMRENDSZER
            SectionTitle("HULLÁMRENDSZER", ref y);
            WaveRow("1. kör:",  "Csak Normal zombik, kevesen, lassú spawn. Tanulj.", ref y);
            WaveRow("2. kör:",  "Több zombi, gyorsabb spawn. Gyors zombik is jönnek.", ref y);
            WaveRow("3. kör:",  "FEJLESZTÉS VÁLASZTÓ + FEGYVERBOLT először megnyílik!", ref y, true);
            WaveRow("4. kör:",  "Tank zombik érkeznek. Nagy HP, erős barrikád-rombolás.", ref y);
            WaveRow("5. kör:",  "BOSS megjelenik. 300+ HP, 30 sebzés, $100 jutalom.", ref y, true);
            WaveRow("6. kör:",  "Újra Fejlesztés Választó + Fegyverbolt.", ref y, true);
            WaveRow("9. kör:",  "Újra Fejlesztés Választó + Fegyverbolt.", ref y, true);
            WaveRow("N. kör:",  "Zombik HP-ja körönként x1.25. Végtelen túlélés.", ref y);
            y += 6;
            Raylib.DrawText("Minden 3. körben: Fejlesztés Választó + Fegyverbolt.", 30, y, 14, DIM);
            y += 22;
            Raylib.DrawText("Körös arany bónusz: körszám x $25 minden teljesített kör végén.", 30, y, 14, DIM);
            y += 26;

            // ELLENSÉGEK
            SectionTitle("ELLENSÉGEK", ref y);
            Raylib.DrawText("NÉV          HP     SPD    DMG    JUTALOM", 30, y, 13, DIM);
            Raylib.DrawLine(30, y + 17, W - 30, y + 17, C(40, 40, 40));
            y += 22;
            EnemyRow("Normal",  " 50",  " 42", " 10", "10",  ref y);
            EnemyRow("Gyors",   " 30",  " 70", "  8", "15",  ref y);
            EnemyRow("Tank",    "150",  " 35", " 20", "30",  ref y);
            EnemyRow("Boss",    "300+", " 36", " 30", "100", ref y);
            y += 6;
            BulletRow("50% HP alatt minden zombi KÚSZIK: sebesség -60%, kisebb, kevesebb sebzés.", ref y);
            BulletRow("FEJLÖVÉS = 2x sebzés. Célozz a zombi sprite tetejére.", ref y);
            BulletRow("Boss minden 5. körben spawnel (5., 10., 15...) véletlenszerűen.", ref y);

            // FEGYVEREK
            SectionTitle("FEGYVEREK", ref y);
            Raylib.DrawText("NÉV          ERRE    ADATOK", 30, y, 13, DIM);
            Raylib.DrawLine(30, y + 17, W - 30, y + 17, C(40, 40, 40));
            y += 22;
            WeaponRow("Pistol",   "Ingyen", "12 löv  |  20 seb  |  0.35s CD  |  1.5s tölt", ref y);
            WeaponRow("SMG",      "$150",   "30 löv  |  10 seb  |  0.10s CD  |  2.0s tölt", ref y);
            WeaponRow("Shotgun",  "$200",   " 8 löv  |  14 seb  |  5 pellet  |  2.5s tölt", ref y);
            WeaponRow("M4 Rifle", "$350",   "25 löv  |  35 seb  |  0.18s CD  |  2.2s tölt", ref y);
            WeaponRow("Sniper",   "$500",   " 5 löv  | 120 seb  |  1.20s CD  |  3.0s tölt", ref y);
            y += 6;
            BulletRow("Fegyvert körök között megtartod.", ref y);
            BulletRow("Fegyverbolt minden 3. kör után nyílik meg.", ref y);

            // BARRIKÁD
            SectionTitle("BARRIKÁD", ref y);
            BulletRow("B gombbal helyezed le a katona előtt.", ref y);
            BulletRow("800 HP. A zombik megállnak és ütnék a barrikádot.", ref y);
            BulletRow("Repedések jelzik a sérülést. A golyók átmennek rajta.", ref y);
            BulletRow("Barrikád töltetek fejlesztésekből szerezhető.", ref y);

            // FEJLESZTÉSEK
            SectionTitle("FEJLESZTÉSEK (minden 3. körben)", ref y);
            BulletRow("Minden alkalommal 1-et választasz 3 véletlenszerű közül.", ref y);
            BulletRow("Példák: extra barrikád töltetek, max HP növelés,", ref y);
            BulletRow("életlopás találatnál, gyorsabb töltés, sebzés szorzó.", ref y);

            // META BOLT
            SectionTitle("GAME OVER  /  META BOLT", ref y);
            BulletRow("Halál után a session arany hozzáadódik a permanens összeghez.", ref y);
            BulletRow("+5% Sebzés szintenként  (ár: $30, +$15/szint)", ref y);
            BulletRow("+10 Max HP szintenként  (ár: $25, +$10/szint)", ref y);
            BulletRow("+5% Sebesség szintenként  (ár: $35, +$20/szint)", ref y);
            BulletRow("A bónuszok minden jövőbeli futásra megmaradnak.", ref y);

            // TIPPEK
            SectionTitle("TIPPEK", ref y);
            BulletRow("Barrikádot töltés előtt helyezz le biztonságos újratöltéshez.", ref y);
            BulletRow("Mindig fejre célozz Tankokkal és a Bossnál.", ref y);
            BulletRow("Spórolj a 3. körig Riflre vagy Sniperre.", ref y);
            BulletRow("A Bosst azonnal fókuszáld - sebzése hamar levisz.", ref y);
            BulletRow("Meta boltban először a Permanens Sebzésbe fektess.", ref y);

            y += 20;
        }
    }
}
