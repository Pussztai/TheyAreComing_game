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
        private bool[] hovered   = new bool[3];
        private int selectedIndex = -1;   // melyik upgrade van kiválasztva (-1 = egyik sem)

        private Rectangle continueBtn;
        private Rectangle medKitBtn;
        private bool medKitPurchased = false;

        public bool ContinueClicked  { get; private set; } = false;
        private Upgrade? pendingUpgrade = null;  // a kiválasztott, de még nem applied upgrade

        public UpgradeSystem() {
            continueBtn = new Rectangle(300, 510, 200, 50);
            medKitBtn   = new Rectangle(295, 415, 210, 55);
        }

        public void GenerateOptions() {
            currentOptions = FreeUpgrades.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();

            int bw = 210, bh = 120, gap = 20;
            int startX = (800 - (3 * bw + 2 * gap)) / 2;
            int startY = 200;
            for (int i = 0; i < 3; i++)
                buttons[i] = new Rectangle(startX + i * (bw + gap), startY, bw, bh);

            selectedIndex   = -1;
            medKitPurchased = false;
            ContinueClicked = false;
            pendingUpgrade  = null;
        }

        
        public Upgrade? Update(SoldierPlayer player) {
            ContinueClicked = false;
            Vector2 mp = Raylib.GetMousePosition();

            for (int i = 0; i < 3; i++) {
                hovered[i] = Raylib.CheckCollisionPointRec(mp, buttons[i]);
                if (hovered[i] && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                    selectedIndex  = i;
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

        public void Draw(SoldierPlayer player) {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 210));

            string title = "CHOOSE AN UPGRADE";
            Raylib.DrawText(title, 400 - Raylib.MeasureText(title, 34) / 2, 148, 34, Color.Gold);

            string sub = "— select one, then press Continue —";
            Raylib.DrawText(sub, 400 - Raylib.MeasureText(sub, 13) / 2, 188, 13, new Color(155, 155, 170, 255));

            for (int i = 0; i < 3; i++) {
                var b = buttons[i];
                bool sel = selectedIndex == i;
                Color bg     = sel ? new Color(30, 80, 30, 255) :
                               hovered[i] ? new Color(55, 110, 55, 255) : new Color(35, 35, 52, 255);
                Color border = sel ? Color.Green :
                               hovered[i] ? new Color(100, 180, 100, 255) : new Color(90, 90, 110, 255);

                Raylib.DrawRectangleRec(b, bg);
                Raylib.DrawRectangleLinesEx(b, 2, border);

                var up = currentOptions[i];
                int nw = Raylib.MeasureText(up.Name, 17);
                Raylib.DrawText(up.Name, (int)b.X + (int)b.Width / 2 - nw / 2, (int)b.Y + 18, 17,
                    sel ? new Color(144, 238, 144, 255) : Color.White);

                int dw = Raylib.MeasureText(up.Description, 13);
                Raylib.DrawText(up.Description, (int)b.X + (int)b.Width / 2 - dw / 2, (int)b.Y + 48, 13,
                    Color.LightGray);

                string badge = sel ? "✓ SELECTED" : (hovered[i] ? "CLICK TO SELECT" : "");
                if (badge != "") {
                    Raylib.DrawText(badge,
                        (int)b.X + (int)b.Width / 2 - Raylib.MeasureText(badge, 12) / 2,
                        (int)b.Y + (int)b.Height - 20, 12,
                        sel ? new Color(144, 238, 144, 255) : Color.White);
                }
            }

            Vector2 mp2 = Raylib.GetMousePosition();
            bool medHover  = Raylib.CheckCollisionPointRec(mp2, medKitBtn);
            bool canAfford = player.Money >= 100;

            Color medBg = medKitPurchased ? new Color(20, 60, 20, 255) :
                          !canAfford      ? new Color(40, 28, 28, 255) :
                          medHover        ? new Color(130, 30, 30, 255) :
                                            new Color(80, 20, 20, 255);
            Color medBorder = medKitPurchased ? new Color(60, 160, 60, 255) :
                              !canAfford      ? new Color(100, 60, 60, 255) :
                              medHover        ? Color.Red :
                                               new Color(180, 60, 60, 255);

            Raylib.DrawRectangleRec(medKitBtn, medBg);
            Raylib.DrawRectangleLinesEx(medKitBtn, 2, medBorder);

            string mkTitle = medKitPurchased ? "✓ MED KIT USED" : "MED KIT  — $100";
            Color mkColor  = medKitPurchased ? new Color(144, 238, 144, 255) :
                             canAfford       ? Color.White :
                                              new Color(160, 100, 100, 255);
            Raylib.DrawText(mkTitle,
                (int)medKitBtn.X + (int)medKitBtn.Width / 2 - Raylib.MeasureText(mkTitle, 17) / 2,
                (int)medKitBtn.Y + 8, 17, mkColor);

            string hpStatus = $"HP: {player.Health} / {player.MaxHealth}";
            Raylib.DrawText(hpStatus,
                (int)medKitBtn.X + (int)medKitBtn.Width / 2 - Raylib.MeasureText(hpStatus, 13) / 2,
                (int)medKitBtn.Y + 32, 13, Color.LightGray);

            if (!canAfford && !medKitPurchased) {
                string poor = $"(need ${100 - player.Money} more)";
                Raylib.DrawText(poor,
                    (int)medKitBtn.X + (int)medKitBtn.Width / 2 - Raylib.MeasureText(poor, 11) / 2,
                    (int)medKitBtn.Y + 50, 11, new Color(180, 100, 100, 255));
            }

            bool showContinue = selectedIndex >= 0;
            bool contHover2   = Raylib.CheckCollisionPointRec(mp2, continueBtn);

            if (showContinue) {
                Raylib.DrawRectangleRec(continueBtn,
                    contHover2 ? new Color(50, 200, 50, 255) : new Color(30, 140, 30, 255));
                Raylib.DrawRectangleLinesEx(continueBtn, 2, Color.Green);
                string ct = "CONTINUE  →";
                Raylib.DrawText(ct,
                    (int)continueBtn.X + (int)continueBtn.Width / 2 - Raylib.MeasureText(ct, 22) / 2,
                    (int)continueBtn.Y + 13, 22, Color.White);
            } else {
                Raylib.DrawRectangleRec(continueBtn, new Color(28, 28, 40, 255));
                Raylib.DrawRectangleLinesEx(continueBtn, 2, new Color(65, 65, 78, 255));
                string ct = "select upgrade first";
                Raylib.DrawText(ct,
                    (int)continueBtn.X + (int)continueBtn.Width / 2 - Raylib.MeasureText(ct, 13) / 2,
                    (int)continueBtn.Y + 18, 13, new Color(95, 95, 108, 255));
            }

            string moneyStr = $"Money: ${player.Money}";
            Raylib.DrawText(moneyStr,
                400 - Raylib.MeasureText(moneyStr, 19) / 2, 573, 19, Color.Gold);
        }
    }
}
