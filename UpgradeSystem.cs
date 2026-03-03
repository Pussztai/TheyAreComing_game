using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public enum UpgradeType {
        Damage,
        FireRate,
        MaxHP,
        MoveSpeed,
        MaxAmmo,
        Lifesteal,
        MedKit
    }

    public class Upgrade {
        public UpgradeType Type;
        public string Name;
        public string Description;
        public int Cost;

        public Upgrade(UpgradeType type, string name, string desc, int cost = 0) {
            Type = type; Name = name; Description = desc; Cost = cost;
        }
    }

    public class UpgradeSystem {
        private static readonly List<Upgrade> FreeUpgrades = new() {
            new(UpgradeType.Damage,    "+10% Damage",     "Bullet damage +10%"),
            new(UpgradeType.FireRate,  "+15% Fire Rate",  "Shoot faster"),
            new(UpgradeType.MaxHP,     "+20 Max HP",      "Increase max health"),
            new(UpgradeType.MoveSpeed, "+10% Move Speed", "Move faster"),
            new(UpgradeType.MaxAmmo,   "+10 Max Ammo",    "More bullets per clip"),
            new(UpgradeType.Lifesteal, "5% Lifesteal",    "Heal 5% of damage dealt")
        };

        private List<Upgrade> currentOptions = new();
        private Rectangle[] buttons = new Rectangle[3];
        private bool[] hovered = new bool[3];
        private int selectedIndex = -1;

        private Rectangle continueBtn;
        private Rectangle medKitBtn;
        private Rectangle barricadeBtn;
        private bool medKitPurchased = false;
        private int barrPurchaseCount = 0;

        public bool ContinueClicked { get; private set; } = false;
        private Upgrade? pendingUpgrade = null;

        public int BarricadeCount { get; private set; } = 0;

        public UpgradeSystem() {
            continueBtn = new Rectangle(300, 510, 200, 50);
            medKitBtn = new Rectangle(170, 415, 210, 55);
            barricadeBtn = new Rectangle(420, 415, 210, 55);
        }

        public void GenerateOptions() {
            currentOptions = FreeUpgrades.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();

            int bw = 210, bh = 120, gap = 20;
            int startX = (800 - (3 * bw + 2 * gap)) / 2;
            int startY = 200;
            for (int i = 0; i < 3; i++)
                buttons[i] = new Rectangle(startX + i * (bw + gap), startY, bw, bh);

            selectedIndex = -1;
            medKitPurchased = false;
            barrPurchaseCount = 0;
            ContinueClicked = false;
            pendingUpgrade = null;
        }

        public Upgrade? Update(SoldierPlayer player) {
            ContinueClicked = false;
            Vector2 mp = Raylib.GetMousePosition();

            for (int i = 0; i < 3; i++) {
                hovered[i] = Raylib.CheckCollisionPointRec(mp, buttons[i]);
                if (hovered[i] && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                    selectedIndex = i;
                    pendingUpgrade = currentOptions[i];
                }
            }

            bool medHover = Raylib.CheckCollisionPointRec(mp, medKitBtn);
            if (medHover && Raylib.IsMouseButtonPressed(MouseButton.Left)
                && !medKitPurchased && player.Money >= 100) {
                player.Money -= 100;
                player.Health = player.MaxHealth;
                medKitPurchased = true;
            }

            bool barrHover = Raylib.CheckCollisionPointRec(mp, barricadeBtn);
            if (barrHover && Raylib.IsMouseButtonPressed(MouseButton.Left)
                && barrPurchaseCount < 3 && player.Money >= 100) {
                player.Money -= 100;
                BarricadeCount += 1;
                barrPurchaseCount++;
            }

            if (selectedIndex >= 0) {
                bool contHover = Raylib.CheckCollisionPointRec(mp, continueBtn);
                if (contHover && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                    ContinueClicked = true;
                    return pendingUpgrade;
                }
            }

            return null;
        }

        public void ApplyUpgrade(Upgrade up, SoldierPlayer player) {
            switch (up.Type) {
                case UpgradeType.Damage:
                    player.BulletDamage *= 1.1f;
                    player.ActiveUpgrades.Add("+10% DMG");
                    break;
                case UpgradeType.FireRate:
                    player.ShootCooldownTime = Math.Max(0.1f, player.ShootCooldownTime * 0.85f);
                    player.ActiveUpgrades.Add("+15% Fire Rate");
                    break;
                case UpgradeType.MaxHP:
                    player.MaxHealth += 20;
                    player.Health = Math.Min(player.Health + 20, player.MaxHealth);
                    player.ActiveUpgrades.Add("+20 Max HP");
                    break;
                case UpgradeType.MoveSpeed:
                    player.Speed *= 1.1f;
                    player.ActiveUpgrades.Add("+10% Speed");
                    break;
                case UpgradeType.MaxAmmo:
                    player.MaxAmmo += 10;
                    player.ActiveUpgrades.Add("+10 Max Ammo");
                    break;
                case UpgradeType.Lifesteal:
                    player.Lifesteal += 0.05f;
                    player.ActiveUpgrades.Add("5% Lifesteal");
                    break;
            }
        }

        public bool UseBarricade() {
            if (BarricadeCount <= 0) return false;
            BarricadeCount--;
            return true;
        }

        public void Draw(SoldierPlayer player) {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 220));

            Raylib.DrawRectangle(0, 130, 800, 58, new Color(15, 15, 25, 240));
            Raylib.DrawRectangle(0, 130, 800, 2, new Color(180, 140, 0, 200));
            Raylib.DrawRectangle(0, 186, 800, 2, new Color(60, 60, 80, 180));

            string title = "UPGRADE  SCREEN";
            Raylib.DrawText(title, 400 - Raylib.MeasureText(title, 30) / 2, 142, 30, Color.Gold);

            string sub = "Choose an upgrade, then press Continue";
            Raylib.DrawText(sub, 400 - Raylib.MeasureText(sub, 12) / 2, 174, 12, new Color(140, 140, 160, 255));

            for (int i = 0; i < 3; i++) {
                var b = buttons[i];
                bool sel = selectedIndex == i;
                bool hov = hovered[i];

                Raylib.DrawRectangle((int)b.X + 4, (int)b.Y + 4, (int)b.Width, (int)b.Height, new Color(0, 0, 0, 100));

                Color bg = sel ? new Color(18, 55, 22, 255) :
                               hov ? new Color(30, 45, 35, 255) : new Color(22, 22, 36, 255);
                Color border = sel ? new Color(80, 220, 90, 255) :
                               hov ? new Color(70, 130, 75, 255) : new Color(55, 55, 75, 255);

                Raylib.DrawRectangleRec(b, bg);

                Color topStripe = sel ? new Color(80, 220, 90, 200) :
                                  hov ? new Color(60, 110, 65, 180) : new Color(70, 70, 100, 120);
                Raylib.DrawRectangle((int)b.X, (int)b.Y, (int)b.Width, 4, topStripe);
                Raylib.DrawRectangleLinesEx(b, 1, border);

                var up = currentOptions[i];

                string emoji = up.Type switch {
                    UpgradeType.Damage => "DMG",
                    UpgradeType.FireRate => "SPD",
                    UpgradeType.MaxHP => " HP",
                    UpgradeType.MoveSpeed => "MOV",
                    UpgradeType.MaxAmmo => "AMO",
                    UpgradeType.Lifesteal => "LST",
                    _ => "???"
                };
                Color emojiCol = sel ? new Color(100, 240, 110, 255) :
                                 hov ? new Color(80, 180, 90, 255) : new Color(100, 100, 140, 255);
                Raylib.DrawText(emoji, (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(emoji, 22) / 2, (int)b.Y + 14, 22, emojiCol);

                Raylib.DrawRectangle((int)b.X + 12, (int)b.Y + 42, (int)b.Width - 24, 1, new Color(70, 70, 90, 200));

                int nw = Raylib.MeasureText(up.Name, 15);
                Raylib.DrawText(up.Name, (int)b.X + (int)b.Width / 2 - nw / 2, (int)b.Y + 52, 15,
                    sel ? new Color(144, 238, 144, 255) : Color.White);

                int dw = Raylib.MeasureText(up.Description, 12);
                Raylib.DrawText(up.Description, (int)b.X + (int)b.Width / 2 - dw / 2, (int)b.Y + 74, 12,
                    new Color(170, 170, 185, 255));

                if (sel) {
                    Raylib.DrawRectangle((int)b.X + 8, (int)b.Y + (int)b.Height - 22, (int)b.Width - 16, 16, new Color(30, 100, 35, 200));
                    string badge = "✓  SELECTED";
                    Raylib.DrawText(badge, (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(badge, 11) / 2,
                        (int)b.Y + (int)b.Height - 20, 11, new Color(144, 238, 144, 255));
                } else if (hov) {
                    Raylib.DrawRectangle((int)b.X + 8, (int)b.Y + (int)b.Height - 22, (int)b.Width - 16, 16, new Color(40, 55, 45, 180));
                    string badge = "CLICK TO SELECT";
                    Raylib.DrawText(badge, (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(badge, 10) / 2,
                        (int)b.Y + (int)b.Height - 20, 10, new Color(160, 200, 165, 255));
                }
            }

            Vector2 mp2 = Raylib.GetMousePosition();

            Raylib.DrawRectangle(0, 398, 800, 18, new Color(10, 10, 18, 220));
            string shopTitle = "━━  SHOP  ━━";
            Raylib.DrawText(shopTitle, 400 - Raylib.MeasureText(shopTitle, 12) / 2, 402, 12, new Color(120, 120, 150, 200));

            DrawShopBtn(medKitBtn, mp2, medKitPurchased, player.Money >= 100,
                medKitPurchased ? "✓  FIRST AID USED" : "FIRST AID KIT",
                medKitPurchased ? "Full HP restored" : $"Restore full HP  —  $100",
                null,
                new Color(55, 15, 15, 255), new Color(80, 22, 22, 255),
                new Color(150, 45, 45, 255), new Color(220, 70, 70, 255));

            bool barrMaxed = barrPurchaseCount >= 3;
            bool canBarr = player.Money >= 100 && !barrMaxed;
            string barrTitle = barrMaxed ? "✓  MAX BARRICADES" : $"BARRICADE  —  $100";
            string barrLine2 = $"Owned: {BarricadeCount}   Can buy: {3 - barrPurchaseCount}x more";
            DrawShopBtn(barricadeBtn, mp2, barrMaxed, canBarr,
                barrTitle, barrLine2, null,
                new Color(28, 38, 14, 255), new Color(45, 60, 20, 255),
                new Color(90, 120, 40, 255), new Color(140, 185, 55, 255));

            bool showContinue = selectedIndex >= 0;
            bool contHover2 = Raylib.CheckCollisionPointRec(mp2, continueBtn);

            if (showContinue) {
                Raylib.DrawRectangle((int)continueBtn.X + 3, (int)continueBtn.Y + 3,
                    (int)continueBtn.Width, (int)continueBtn.Height, new Color(0, 0, 0, 120));
                Raylib.DrawRectangleRec(continueBtn,
                    contHover2 ? new Color(40, 175, 50, 255) : new Color(25, 120, 35, 255));
                Raylib.DrawRectangle((int)continueBtn.X, (int)continueBtn.Y, (int)continueBtn.Width, 3,
                    new Color(80, 230, 90, 180));
                Raylib.DrawRectangleLinesEx(continueBtn, 1, new Color(60, 200, 70, 200));
                string ct = "CONTINUE  →";
                Raylib.DrawText(ct,
                    (int)continueBtn.X + (int)continueBtn.Width / 2 - Raylib.MeasureText(ct, 20) / 2,
                    (int)continueBtn.Y + 15, 20, Color.White);
            } else {
                Raylib.DrawRectangleRec(continueBtn, new Color(20, 20, 32, 255));
                Raylib.DrawRectangleLinesEx(continueBtn, 1, new Color(50, 50, 65, 255));
                string ct = "— select an upgrade —";
                Raylib.DrawText(ct,
                    (int)continueBtn.X + (int)continueBtn.Width / 2 - Raylib.MeasureText(ct, 12) / 2,
                    (int)continueBtn.Y + 19, 12, new Color(75, 75, 92, 255));
            }

            Raylib.DrawRectangle(0, 574, 800, 26, new Color(0, 0, 0, 180));
            string moneyStr = player.IsPlayground ? "∞" : $"$  {player.Money}";
            Color moneyCol = player.IsPlayground ? Color.SkyBlue : Color.Gold;
            Raylib.DrawText(moneyStr, 400 - Raylib.MeasureText(moneyStr, 18) / 2, 579, 18, moneyCol);
        }

        private void DrawShopBtn(Rectangle b, Vector2 mp, bool purchased, bool canAfford,
                                  string titleText, string line2, string? line3,
                                  Color bgNorm, Color bgHover, Color borderNorm, Color borderHover) {
            bool hover = Raylib.CheckCollisionPointRec(mp, b);

            Color bg = purchased ? new Color(15, 45, 18, 255) :
                       !canAfford ? new Color(30, 18, 18, 255) :
                       hover ? bgHover : bgNorm;
            Color border = purchased ? new Color(55, 150, 60, 255) :
                           !canAfford ? new Color(80, 45, 45, 255) :
                           hover ? borderHover : borderNorm;

            Raylib.DrawRectangle((int)b.X + 3, (int)b.Y + 3, (int)b.Width, (int)b.Height, new Color(0, 0, 0, 80));
            Raylib.DrawRectangleRec(b, bg);

            Color stripe = purchased ? new Color(55, 150, 60, 180) :
                           !canAfford ? new Color(80, 45, 45, 100) :
                           hover ? borderHover : new Color(borderNorm.R, borderNorm.G, borderNorm.B, (byte)120);
            Raylib.DrawRectangle((int)b.X, (int)b.Y, (int)b.Width, 3, stripe);
            Raylib.DrawRectangleLinesEx(b, 1, border);

            Color titleCol = purchased ? new Color(130, 220, 135, 255) :
                             canAfford ? Color.White :
                                         new Color(130, 85, 85, 255);
            Color line2Col = purchased ? new Color(100, 170, 105, 255) :
                             canAfford ? new Color(175, 175, 190, 255) :
                                         new Color(110, 75, 75, 255);

            Raylib.DrawText(titleText,
                (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(titleText, 14) / 2,
                (int)b.Y + 9, 14, titleCol);

            Raylib.DrawRectangle((int)b.X + 10, (int)b.Y + 28, (int)b.Width - 20, 1, new Color(60, 60, 80, 160));

            Raylib.DrawText(line2,
                (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(line2, 11) / 2,
                (int)b.Y + 33, 11, line2Col);

            if (line3 != null) {
                Raylib.DrawText(line3,
                    (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(line3, 10) / 2,
                    (int)b.Y + 48, 10, new Color(160, 185, 110, 255));
            }

            if (hover && canAfford && !purchased)
                Raylib.DrawRectangle((int)b.X + (int)b.Width - 8, (int)b.Y + 4, 4, 4, borderHover);
        }
    }
}