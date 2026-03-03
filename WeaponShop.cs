using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public class WeaponShop {

        private Rectangle[] btnWeapon = new Rectangle[5];
        private Rectangle btnClose;
        private bool[] hovered = new bool[5];

        public bool IsClosed { get; private set; } = false;

        private Texture2D?[] weaponTextures = new Texture2D?[5];
        private bool[] weaponLoaded = new bool[5];

        private static readonly (string file, bool blackBg)[] WeaponImages = {
            ("pistolpixelart.png",  true),
            ("smgpixelart.png",     true),
            ("shotgunpixelart.png", false),
            ("m4pixelart.png",      true),
            ("sniperpixelart.png",  true),
        };

        public WeaponShop() {
            int bw = 140, bh = 90, gap = 12;
            int total = WeaponCatalog.All.Count;
            int startX = (800 - (total * bw + (total - 1) * gap)) / 2;
            int startY = 240;
            for (int i = 0; i < total; i++)
                btnWeapon[i] = new Rectangle(startX + i * (bw + gap), startY, bw, bh);

            btnClose = new Rectangle(300, 490, 200, 48);

            for (int i = 0; i < WeaponImages.Length; i++) {
                var (file, blackBg) = WeaponImages[i];
                string[] paths = {
                    file,
                    "./" + file,
                    System.IO.Path.Combine(AppContext.BaseDirectory, file),
                };
                foreach (var p in paths) {
                    if (!System.IO.File.Exists(p)) continue;
                    try {
                        Image img = Raylib.LoadImage(p);
                        if (img.Width == 0 || img.Height == 0) continue;
                        if (blackBg) {
                            Raylib.ImageColorReplace(ref img, new Color(0, 0, 0, 255), new Color(0, 0, 0, 0));
                            Raylib.ImageColorReplace(ref img, new Color(1, 1, 1, 255), new Color(0, 0, 0, 0));
                            Raylib.ImageColorReplace(ref img, new Color(2, 2, 2, 255), new Color(0, 0, 0, 0));
                        } else {
                            Raylib.ImageColorReplace(ref img, new Color(255, 255, 255, 255), new Color(0, 0, 0, 0));
                            Raylib.ImageColorReplace(ref img, new Color(254, 254, 254, 255), new Color(0, 0, 0, 0));
                            Raylib.ImageColorReplace(ref img, new Color(253, 253, 253, 255), new Color(0, 0, 0, 0));
                        }
                        var tex = Raylib.LoadTextureFromImage(img);
                        Raylib.UnloadImage(img);
                        if (tex.Id != 0) {
                            weaponTextures[i] = tex;
                            weaponLoaded[i] = true;
                            break;
                        }
                    } catch { }
                }
            }
        }

        public void Open() { IsClosed = false; }

        public WeaponType? Update(SoldierPlayer player) {
            if (IsClosed) return null;
            Vector2 mp = Raylib.GetMousePosition();

            for (int i = 0; i < WeaponCatalog.All.Count; i++) {
                hovered[i] = Raylib.CheckCollisionPointRec(mp, btnWeapon[i]);
                if (hovered[i] && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                    var def = WeaponCatalog.All[i];
                    bool alreadyOwned = player.CurrentWeapon == def.Type;
                    bool canAfford = player.Money >= def.Cost;
                    if (canAfford && !alreadyOwned) {
                        if (!player.IsPlayground) player.Money -= def.Cost;
                        player.EquipWeapon(def);
                        IsClosed = true;
                        return def.Type;
                    } else if (alreadyOwned) {
                        IsClosed = true;
                        return null;
                    }
                }
            }

            if (Raylib.CheckCollisionPointRec(mp, btnClose) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                IsClosed = true;

            return null;
        }

        public void Draw(SoldierPlayer player) {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 195));

            string title = "WEAPON SHOP";
            Raylib.DrawText(title, 400 - Raylib.MeasureText(title, 38) / 2, 100, 38, Color.Gold);

            string moneyDisplay = player.IsPlayground ? "∞" : $"${player.Money}";
            string sub = $"Money: {moneyDisplay}   —   click a weapon to purchase";
            Raylib.DrawText(sub, 400 - Raylib.MeasureText(sub, 14) / 2, 148, 14, Color.LightGray);

            string curr = $"Current weapon:  {WeaponCatalog.Get(player.CurrentWeapon).Name}";
            Raylib.DrawText(curr, 400 - Raylib.MeasureText(curr, 16) / 2, 170, 16,
                new Color(144, 238, 144, 255));

            Vector2 mp = Raylib.GetMousePosition();

            for (int i = 0; i < WeaponCatalog.All.Count; i++) {
                var def = WeaponCatalog.All[i];
                var b = btnWeapon[i];
                bool owned = player.CurrentWeapon == def.Type;
                bool afford = player.IsPlayground || player.Money >= def.Cost;
                bool hover = Raylib.CheckCollisionPointRec(mp, b);

                Color bg = owned
                    ? new Color(20, 70, 20, 255)
                    : (!afford ? new Color(40, 28, 28, 255)
                               : hover ? new Color(55, 55, 78, 255)
                                       : new Color(30, 30, 48, 255));

                Color border = owned ? Color.Green
                             : !afford ? new Color(100, 60, 60, 255)
                             : hover ? Color.Gold
                                       : new Color(80, 80, 110, 255);

                Raylib.DrawRectangleRec(b, bg);
                Raylib.DrawRectangleLinesEx(b, 2, border);

                Color tint = owned ? new Color(144, 238, 144, 255) :
                             afford ? Color.White :
                                      new Color(120, 80, 80, 255);

                if (weaponLoaded[i] && weaponTextures[i].HasValue)
                    DrawWeaponImage(weaponTextures[i]!.Value, b, tint);
                else
                    DrawWeaponIconFallback(def.Type, (int)b.X + 8, (int)b.Y + 8,
                        owned ? Color.Green : afford ? Color.SkyBlue : new Color(120, 80, 80, 255));

                Color nameCol = owned ? new Color(144, 238, 144, 255) : afford ? Color.White : new Color(150, 100, 100, 255);
                int nw = Raylib.MeasureText(def.Name, 14);
                Raylib.DrawText(def.Name, (int)b.X + (int)b.Width / 2 - nw / 2,
                    (int)b.Y + 46, 14, nameCol);

                string costStr = owned ? "✓ OWNED" : (def.Cost == 0 ? "FREE" : player.IsPlayground ? "∞" : $"${def.Cost}");
                Color costCol = owned ? new Color(144, 238, 144, 255)
                               : def.Cost == 0 ? Color.Gold
                               : afford ? Color.Yellow : new Color(200, 100, 100, 255);
                int cw = Raylib.MeasureText(costStr, 13);
                Raylib.DrawText(costStr, (int)b.X + (int)b.Width / 2 - cw / 2,
                    (int)b.Y + 65, 13, costCol);

                if (hover) DrawTooltip(def, (int)b.X + (int)b.Width / 2, (int)b.Y - 5);
            }

            bool overClose = Raylib.CheckCollisionPointRec(mp, btnClose);
            Raylib.DrawRectangleRec(btnClose, overClose ? new Color(80, 50, 50, 255) : new Color(45, 35, 35, 255));
            Raylib.DrawRectangleLinesEx(btnClose, 2, overClose ? Color.Red : new Color(120, 80, 80, 255));
            string ct = "CLOSE (keep current weapon)";
            Raylib.DrawText(ct, (int)btnClose.X + (int)btnClose.Width / 2 - Raylib.MeasureText(ct, 13) / 2,
                (int)btnClose.Y + 17, 13, Color.LightGray);
        }

        private static void DrawWeaponImage(Texture2D tex, Rectangle b, Color tint) {
            float iconAreaX = b.X + 4, iconAreaY = b.Y + 4;
            float iconAreaW = b.Width - 8, iconAreaH = 38f;
            float texW = tex.Width, texH = tex.Height;
            float scale = MathF.Min(iconAreaW / texW, iconAreaH / texH);
            float drawW = texW * scale, drawH = texH * scale;
            float drawX = iconAreaX + (iconAreaW - drawW) / 2f;
            float drawY = iconAreaY + (iconAreaH - drawH) / 2f;
            Raylib.DrawTexturePro(tex,
                new Rectangle(0, 0, texW, texH),
                new Rectangle(drawX, drawY, drawW, drawH),
                Vector2.Zero, 0f, tint);
        }

        private static void DrawWeaponIconFallback(WeaponType t, int x, int y, Color c) {
            switch (t) {
                case WeaponType.Pistol:
                    Raylib.DrawRectangle(x + 12, y + 12, 22, 6, c);
                    Raylib.DrawRectangle(x + 14, y + 18, 10, 14, c);
                    Raylib.DrawRectangle(x + 18, y + 22, 2, 6, c);
                    break;
                case WeaponType.SMG:
                    Raylib.DrawRectangle(x + 6, y + 12, 32, 6, c);
                    Raylib.DrawRectangle(x + 14, y + 18, 18, 10, c);
                    Raylib.DrawRectangle(x + 20, y + 28, 8, 12, c);
                    break;
                case WeaponType.Shotgun:
                    Raylib.DrawRectangle(x + 4, y + 10, 36, 5, c);
                    Raylib.DrawRectangle(x + 4, y + 16, 36, 5, c);
                    Raylib.DrawRectangle(x + 18, y + 21, 12, 12, c);
                    break;
                case WeaponType.Rifle:
                    Raylib.DrawRectangle(x + 2, y + 13, 44, 5, c);
                    Raylib.DrawRectangle(x + 10, y + 18, 26, 8, c);
                    Raylib.DrawRectangle(x + 18, y + 26, 10, 10, c);
                    Raylib.DrawRectangle(x + 22, y + 10, 6, 4, c);
                    break;
                case WeaponType.Sniper:
                    Raylib.DrawRectangle(x, y + 14, 50, 4, c);
                    Raylib.DrawRectangle(x + 8, y + 18, 28, 7, c);
                    Raylib.DrawRectangle(x + 16, y + 25, 8, 10, c);
                    Raylib.DrawRectangle(x + 20, y + 8, 14, 7, c);
                    Raylib.DrawCircleLines(x + 27, y + 12, 5, c);
                    break;
            }
        }

        private static void DrawTooltip(WeaponDefinition def, int cx, int topY) {
            string[] lines = {
                def.Description,
                $"Damage: {def.BulletDamage:F0}  |  Cooldown: {def.ShootCooldown:F2}s",
                $"Ammo: {def.MaxAmmo}  |  Reload: {def.ReloadTime:F1}s" +
                    (def.PelletCount > 1 ? $"  |  {def.PelletCount}x pellets" : ""),
            };
            int w = 220, lineH = 16, pad = 8;
            int h = lines.Length * lineH + pad * 2;
            int tx = cx - w / 2, ty = topY - h - 6;
            if (ty < 5) ty = topY + 100;
            Raylib.DrawRectangle(tx, ty, w, h, new Color(15, 15, 30, 240));
            Raylib.DrawRectangleLinesEx(new Rectangle(tx, ty, w, h), 1, Color.Gold);
            for (int i = 0; i < lines.Length; i++)
                Raylib.DrawText(lines[i], tx + pad, ty + pad + i * lineH, 11,
                    i == 0 ? Color.Gold : Color.LightGray);
        }

        public void Unload() {
            for (int i = 0; i < weaponTextures.Length; i++)
                if (weaponLoaded[i] && weaponTextures[i].HasValue)
                    Raylib.UnloadTexture(weaponTextures[i]!.Value);
        }
    }
}