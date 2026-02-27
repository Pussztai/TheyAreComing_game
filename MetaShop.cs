using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class MetaShop {
        public float PermanentDamageBonus = 0f;
        public int   PermanentMaxHPBonus  = 0;
        public float PermanentSpeedBonus  = 0f;

        private int damageLevel = 0;
        private int hpLevel     = 0;
        private int speedLevel  = 0;

        private Rectangle btnDamage;
        private Rectangle btnHP;
        private Rectangle btnSpeed;
        private Rectangle btnPlay;
        private Rectangle btnMenu;

        public int TotalGold = 0;
        private bool goldAddedThisSession = false;

        public MetaShop() {
            btnDamage = new Rectangle(90,  240, 185, 75);
            btnHP     = new Rectangle(308, 240, 185, 75);
            btnSpeed  = new Rectangle(526, 240, 185, 75);
            btnPlay   = new Rectangle(290, 430, 220, 55);
            btnMenu   = new Rectangle(290, 500, 220, 35);
        }

        public void AddSessionGold(int gold) {
            if (!goldAddedThisSession) {
                TotalGold += gold;
                goldAddedThisSession = true;
            }
        }

        public void ResetSession() {
            goldAddedThisSession = false;
        }

        public bool Update() {
            Vector2 mp = Raylib.GetMousePosition();

            if (Raylib.CheckCollisionPointRec(mp, btnDamage) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                int cost = 30 + damageLevel * 15;
                if (TotalGold >= cost) { TotalGold -= cost; damageLevel++; PermanentDamageBonus = damageLevel * 0.05f; }
            }
            if (Raylib.CheckCollisionPointRec(mp, btnHP) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                int cost = 25 + hpLevel * 10;
                if (TotalGold >= cost) { TotalGold -= cost; hpLevel++; PermanentMaxHPBonus = hpLevel * 10; }
            }
            if (Raylib.CheckCollisionPointRec(mp, btnSpeed) && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                int cost = 35 + speedLevel * 20;
                if (TotalGold >= cost) { TotalGold -= cost; speedLevel++; PermanentSpeedBonus = speedLevel * 0.05f; }
            }

            return Raylib.CheckCollisionPointRec(mp, btnPlay) && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        public bool IsMainMenuClicked() {
            Vector2 mp = Raylib.GetMousePosition();
            return Raylib.CheckCollisionPointRec(mp, btnMenu) && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        public void Draw(int lastWave, int kills, int sessionGold) {
            Raylib.ClearBackground(new Color(10, 10, 20, 255));

            string t = "GAME OVER";
            Raylib.DrawText(t, 400 - Raylib.MeasureText(t, 56) / 2, 45, 56, Color.Red);

            string stats = $"Wave {lastWave}  |  {kills} Kills  |  Session: ${sessionGold}";
            Raylib.DrawText(stats, 400 - Raylib.MeasureText(stats, 20) / 2, 115, 20, Color.LightGray);

            string goldTxt = $"Total Gold: ${TotalGold}";
            Raylib.DrawText(goldTxt, 400 - Raylib.MeasureText(goldTxt, 26) / 2, 150, 26, Color.Gold);

            string sub = "— PERMANENT UPGRADES —";
            Raylib.DrawText(sub, 400 - Raylib.MeasureText(sub, 18) / 2, 205, 18, new Color(160, 160, 190, 255));

            Vector2 mp = Raylib.GetMousePosition();
            DrawBtn(btnDamage, mp, "+5% Damage",
                $"Lv.{damageLevel}   Cost: ${30 + damageLevel * 15}",
                $"Active: +{(int)(PermanentDamageBonus * 100)}%", Color.Orange);
            DrawBtn(btnHP, mp, "+10 Max HP",
                $"Lv.{hpLevel}   Cost: ${25 + hpLevel * 10}",
                $"Active: +{PermanentMaxHPBonus} HP", Color.Green);
            DrawBtn(btnSpeed, mp, "+5% Speed",
                $"Lv.{speedLevel}   Cost: ${35 + speedLevel * 20}",
                $"Active: +{(int)(PermanentSpeedBonus * 100)}%", Color.SkyBlue);

            bool overPlay = Raylib.CheckCollisionPointRec(mp, btnPlay);
            Raylib.DrawRectangleRec(btnPlay, overPlay ? new Color(50, 190, 50, 255) : new Color(30, 130, 30, 255));
            Raylib.DrawRectangleLinesEx(btnPlay, 2, Color.Green);
            string pt = "PLAY AGAIN";
            Raylib.DrawText(pt, (int)btnPlay.X + (int)btnPlay.Width / 2 - Raylib.MeasureText(pt, 24) / 2,
                (int)btnPlay.Y + 15, 24, Color.White);

            bool overMenu = Raylib.CheckCollisionPointRec(mp, btnMenu);
            Raylib.DrawRectangleRec(btnMenu, overMenu ? new Color(60, 60, 80, 255) : new Color(35, 35, 50, 255));
            Raylib.DrawRectangleLinesEx(btnMenu, 1, Color.Gray);
            string mt = "MAIN MENU";
            Raylib.DrawText(mt, (int)btnMenu.X + (int)btnMenu.Width / 2 - Raylib.MeasureText(mt, 16) / 2,
                (int)btnMenu.Y + 10, 16, Color.LightGray);
        }

        private void DrawBtn(Rectangle b, Vector2 mp, string title, string cost, string active, Color accent) {
            bool over = Raylib.CheckCollisionPointRec(mp, b);
            Raylib.DrawRectangleRec(b, over ? new Color(50, 50, 70, 255) : new Color(28, 28, 42, 255));
            Raylib.DrawRectangleLinesEx(b, 2, over ? accent : new Color(70, 70, 90, 255));
            Raylib.DrawText(title,  (int)b.X + 10, (int)b.Y + 10, 16, accent);
            Raylib.DrawText(cost,   (int)b.X + 10, (int)b.Y + 33, 13, Color.LightGray);
            Raylib.DrawText(active, (int)b.X + 10, (int)b.Y + 52, 12, Color.Gray);
        }
    }
}
