using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class SoldierPlayer {
        private Texture2D spriteSheet;
        private int frameWidth  = 168;   // playerImg_default_clean.png: 840 / 5 frames
        private int frameHeight = 295;
        private int currentFrame = 0;
        private int totalFrames  = 5;
        private float frameTimer = 0f;
        private float frameSpeed = 0.22f;
        private bool isMoving = false;

        public float X { get; set; } = 200;
        public float Y { get; set; } = 460;

        public int Money     { get; set; } = 50;
        public int Kills     { get; set; } = 0;
        public int Health    { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public float Speed { get; set; } = 150f;

        public List<Bullet> Bullets { get; private set; } = new();
        private float shootCooldown = 0f;
        public float ShootCooldownTime { get; set; } = 0.35f;
        public float BulletDamage      { get; set; } = 20f;
        public float Lifesteal         { get; set; } = 0f;

        public WeaponType CurrentWeapon { get; private set; } = WeaponType.Pistol;
        private int    pelletCount = 1;
        private float  spread      = 0f;

        public List<string> ActiveUpgrades { get; private set; } = new();
        private Vector2 aimDirection = new Vector2(1, 0);

        public int   Ammo    { get; set; }
        public int   MaxAmmo { get; set; }
        private float reloadTime  = 1.5f;
        private float reloadTimer = 0f;
        public bool IsReloading { get; private set; } = false;

        private float damageFlashTimer = 0f;

        // ── Mozgási határok (barna terület) ───────────────────────────────
        public const float AreaMinX = 30f;
        public const float AreaMaxX = 770f;
        public const float AreaMinY = 355f;
        public const float AreaMaxY = 565f;

        private MuzzleFlash muzzleFlash;

        // Megjelenítési méret – 168*0.36≈60px széles, 295*0.36≈106px magas
        private const float RenderScale = 0.36f;

        private static string ResolvePath(string filename) {
            string[] candidates = {
                filename,
                System.IO.Path.Combine(AppContext.BaseDirectory, filename),
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename),
            };
            foreach (var p in candidates)
                if (System.IO.File.Exists(p)) return p;
            return filename;
        }

        public SoldierPlayer(string spriteSheetPath, int metaMaxHP = 0, float metaDmg = 0f, float metaSpd = 0f) {
            spriteSheet = Raylib.LoadTexture(ResolvePath(spriteSheetPath));
            MaxHealth += metaMaxHP;
            Health     = MaxHealth;
            BulletDamage *= (1f + metaDmg);
            Speed        *= (1f + metaSpd);
            X = 200; Y = 460;
            muzzleFlash = new MuzzleFlash("GunFire.png");
            var pistol = WeaponCatalog.Get(WeaponType.Pistol);
            MaxAmmo = pistol.MaxAmmo; Ammo = MaxAmmo; reloadTime = pistol.ReloadTime;
        }

        public void EquipWeapon(WeaponDefinition def) {
            CurrentWeapon = def.Type; ShootCooldownTime = def.ShootCooldown;
            BulletDamage = def.BulletDamage; MaxAmmo = def.MaxAmmo; Ammo = MaxAmmo;
            reloadTime = def.ReloadTime; pelletCount = def.PelletCount; spread = def.Spread;
            IsReloading = false; reloadTimer = 0f;
        }

        public void Update(float deltaTime) {
            if (IsReloading) {
                reloadTimer -= deltaTime;
                if (reloadTimer <= 0) { Ammo = MaxAmmo; IsReloading = false; }
            }
            if (Raylib.IsKeyPressed(KeyboardKey.R) && !IsReloading && Ammo < MaxAmmo) StartReload();
            if (Ammo <= 0 && !IsReloading) StartReload();

            float prevX = X, prevY = Y;
            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    Y -= Speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  Y += Speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  X -= Speed * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) X += Speed * deltaTime;

            X = Math.Clamp(X, AreaMinX, AreaMaxX);
            Y = Math.Clamp(Y, AreaMinY, AreaMaxY);

            isMoving = MathF.Abs(X - prevX) > 0.01f || MathF.Abs(Y - prevY) > 0.01f;
            if (isMoving) {
                frameTimer += deltaTime;
                if (frameTimer >= frameSpeed) {
                    // Frame 1-4 között ciklus mozgáskor
                    currentFrame = (currentFrame < 1) ? 1 : currentFrame;
                    currentFrame++;
                    if (currentFrame >= totalFrames) currentFrame = 1;
                    frameTimer = 0f;
                }
            } else {
                // Álló helyzetben mindig az első frame
                currentFrame = 0;
                frameTimer = 0f;
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            float dirX = mousePos.X - X, dirY = mousePos.Y - Y;
            float length = MathF.Sqrt(dirX * dirX + dirY * dirY);
            if (length > 0) { aimDirection.X = dirX / length; aimDirection.Y = dirY / length; }

            shootCooldown -= deltaTime;
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && shootCooldown <= 0 && !IsReloading && Ammo > 0) {
                Shoot(); shootCooldown = ShootCooldownTime; Ammo--;
            }

            foreach (var b in Bullets) b.Update(deltaTime);
            Bullets.RemoveAll(b => !b.IsActive);
            damageFlashTimer -= deltaTime;
            muzzleFlash.Update(deltaTime);
        }

        private void Shoot() {
            float aimAngle = MathF.Atan2(aimDirection.Y, aimDirection.X);
            const float fwd = 46f, side = 8f;
            float cosA = MathF.Cos(aimAngle), sinA = MathF.Sin(aimAngle);
            muzzleFlash.Trigger(X + cosA*fwd - sinA*side, Y + sinA*fwd + cosA*side,
                                aimAngle, GetWeaponFlashScale());

            if (pelletCount <= 1) {
                Bullets.Add(new Bullet(X + aimDirection.X*25, Y + aimDirection.Y*25,
                                       aimDirection.X, aimDirection.Y));
            } else {
                float baseAngle = MathF.Atan2(aimDirection.Y, aimDirection.X);
                float hs = spread / 2f;
                for (int i = 0; i < pelletCount; i++) {
                    float a = baseAngle - hs + spread * i / (pelletCount - 1);
                    Bullets.Add(new Bullet(X + MathF.Cos(a)*20, Y + MathF.Sin(a)*20,
                                           MathF.Cos(a), MathF.Sin(a)));
                }
            }
        }

        private float GetWeaponFlashScale() => CurrentWeapon switch {
            WeaponType.Pistol  => 1.0f, WeaponType.SMG => 0.8f,
            WeaponType.Shotgun => 1.8f, WeaponType.Rifle => 1.3f,
            WeaponType.Sniper  => 2.0f, _ => 1.0f,
        };

        private void StartReload() { IsReloading = true; reloadTimer = reloadTime; }
        public void AddMoney(int amount) => Money += amount;
        public void AddKill() => Kills++;

        public void TakeDamage(int damage) {
            Health = Math.Max(0, Health - damage);
            damageFlashTimer = 0.2f;
        }
        public void Heal(float amount) => Health = Math.Min(MaxHealth, Health + (int)amount);

        public void Draw() {
            Rectangle source = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            float drawW = frameWidth  * RenderScale;
            float drawH = frameHeight * RenderScale;
            Rectangle dest   = new Rectangle(X, Y, drawW, drawH);
            Color tint = damageFlashTimer > 0 ? new Color(255, 100, 100, 255) : Color.White;
            Raylib.DrawTexturePro(spriteSheet, source, dest, new Vector2(drawW / 2f, drawH / 2f), 0f, tint);

            float bw = 80f, bx = X - 40, by2 = Y - (frameHeight * RenderScale / 2f) - 16;
            Raylib.DrawRectangle((int)bx, (int)by2, (int)bw, 8, Color.DarkGray);
            float hpPct = (float)Health / MaxHealth;
            Color hpCol = hpPct > 0.5f ? Color.Green : hpPct > 0.25f ? Color.Orange : Color.Red;
            Raylib.DrawRectangle((int)bx, (int)by2, (int)(bw * hpPct), 8, hpCol);

            float aby = by2 + 12;
            Raylib.DrawRectangle((int)bx, (int)aby, (int)bw, 4, Color.DarkGray);
            if (!IsReloading) {
                float aPct = (float)Ammo / MaxAmmo;
                Raylib.DrawRectangle((int)bx, (int)aby, (int)(bw * aPct), 4, aPct > 0.3f ? Color.Yellow : Color.Red);
            } else {
                float rPct = 1f - (reloadTimer / reloadTime);
                Raylib.DrawRectangle((int)bx, (int)aby, (int)(bw * rPct), 4, Color.SkyBlue);
            }

            Vector2 mp = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mp.X, (int)mp.Y, 10, Color.Red);
            Raylib.DrawLine((int)mp.X - 12, (int)mp.Y, (int)mp.X + 12, (int)mp.Y, Color.Red);
            Raylib.DrawLine((int)mp.X, (int)mp.Y - 12, (int)mp.X, (int)mp.Y + 12, Color.Red);

            foreach (var b in Bullets) b.Draw();
            muzzleFlash.Draw();
        }

        /// <summary>Felső HUD sáv.</summary>
        public void DrawTopBar(int wave, int zombieCount) {
            Raylib.DrawRectangle(0, 0, 800, 38, new Color(0, 0, 0, 215));
            Raylib.DrawRectangle(0, 38, 800, 2, new Color(75, 75, 85, 255));

            DrawSection(5,   4, 68,  30, "WAVE");
            Raylib.DrawText($"{wave}", 10, 19, 18, Color.White);

            DrawSection(81,  4, 118, 30, "HP");
            float hpPct = (float)Health / MaxHealth;
            Color hpC = hpPct > 0.5f ? new Color(50,200,50,255) : hpPct > 0.25f ? Color.Orange : Color.Red;
            Raylib.DrawRectangle(86, 21, 106, 11, new Color(25,25,25,255));
            Raylib.DrawRectangle(86, 21, (int)(106 * hpPct), 11, hpC);
            Raylib.DrawText($"{Health}/{MaxHealth}", 88, 22, 10, Color.White);

            DrawSection(207, 4, 112, 30, "AMMO");
            if (IsReloading) {
                Raylib.DrawText("RELOAD...", 212, 19, 12, Color.SkyBlue);
            } else {
                float aPct = (float)Ammo / MaxAmmo;
                Raylib.DrawRectangle(212, 21, 100, 11, new Color(25,25,25,255));
                Raylib.DrawRectangle(212, 21, (int)(100 * aPct), 11, aPct > 0.3f ? Color.Yellow : Color.Red);
                Raylib.DrawText($"{Ammo}/{MaxAmmo}", 214, 22, 10, Color.White);
            }

            DrawSection(327, 4, 72, 30, "KILLS");
            Raylib.DrawText($"{Kills}", 332, 19, 18, Color.Red);

            DrawSection(407, 4, 85, 30, "MONEY");
            Raylib.DrawText($"${Money}", 412, 19, 15, Color.Gold);

            DrawSection(500, 4, 72, 30, "ENEMY");
            Raylib.DrawText($"{zombieCount}", 505, 19, 18, new Color(255, 80, 80, 255));

            DrawSection(580, 4, 64, 30, "GUN");
            string wname = WeaponCatalog.Get(CurrentWeapon).Name;
            Raylib.DrawText(wname.Length > 6 ? wname[..6] : wname, 585, 20, 10, Color.SkyBlue);

            //Raylib.DrawText($"{Raylib.GetFPS()} FPS", 10, 585, 12, Color.DarkGreen);
        }

        // Hotbar slot téglalap – használja mindkét metódus
        private static Rectangle BarricadeSlotRect() => new Rectangle(340, 559, 120, 36);

        /// <summary>Input lekérdezés (Update-ben hívd): true ha a slot-ra kattintottak.</summary>
        public bool DrawBottomHotbarInput(int barricadeCount) {
            if (barricadeCount <= 0) return false;
            Vector2 mp = Raylib.GetMousePosition();
            // Shoot gomb (LMB) ne számítson bele ha a hotbar fölött van
            bool overSlot = Raylib.CheckCollisionPointRec(mp, BarricadeSlotRect());
            return overSlot && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        /// <summary>Alsó hotbar – barrikád slot rajzolása (Draw-ban hívd).</summary>
        public void DrawBottomHotbar(int barricadeCount) {
            int barY = 555;
            Raylib.DrawRectangle(0, barY, 800, 45, new Color(0, 0, 0, 200));
            Raylib.DrawRectangle(0, barY, 800, 1, new Color(75, 75, 85, 255));

            Raylib.DrawText("HOTBAR", 8, barY + 4, 9, new Color(100, 100, 120, 200));

            Rectangle slotRect = BarricadeSlotRect();
            int slotX = (int)slotRect.X, slotY = (int)slotRect.Y;
            int slotW = (int)slotRect.Width, slotH = (int)slotRect.Height;

            bool hasBarr = barricadeCount > 0;
            Vector2 mp = Raylib.GetMousePosition();
            bool hovered = Raylib.CheckCollisionPointRec(mp, slotRect);

            Color slotBg = hasBarr
                ? (hovered ? new Color(70, 100, 30, 255) : new Color(40, 60, 18, 240))
                : new Color(22, 22, 22, 200);
            Color slotBorder = hasBarr
                ? (hovered ? new Color(180, 240, 80, 255) : new Color(110, 160, 45, 255))
                : new Color(50, 50, 50, 255);

            Raylib.DrawRectangle(slotX, slotY, slotW, slotH, slotBg);
            Raylib.DrawRectangleLinesEx(slotRect, 2, slotBorder);

            Raylib.DrawText("BARRICADE", slotX + 4, slotY + 2, 8,
                hasBarr ? new Color(160, 210, 70, 200) : new Color(60, 60, 60, 200));

            if (hasBarr) {
                // Forgított barrikád ikon (90°-os, tehát magasabb mint széles)
                int ix = slotX + 18, iy = slotY + 18;
                Color wood      = new Color(160, 100, 40, 255);
                Color woodDark  = new Color(90, 55, 18, 255);
                Color woodLight = new Color(210, 150, 70, 255);
                Color nail      = new Color(220, 215, 185, 255);
                // Ikon: 15 wide, 22 tall (90 fokkal fordított arány)
                Raylib.DrawRectangle(ix - 8, iy - 11, 15, 22, woodDark);
                // 3 függőleges deszka (vízszintes helyett)
                int colW = 15 / 3;
                for (int c = 0; c < 3; c++) {
                    Raylib.DrawRectangle(ix - 8 + c * colW + 1, iy - 10, colW - 1, 20, wood);
                    Raylib.DrawLine(ix - 8 + c * colW, iy - 11, ix - 8 + c * colW, iy + 11, woodDark);
                }
                // Vízszintes tartók (volt: függőleges oszlopok)
                foreach (int py in new[] { iy - 4, iy + 3 }) {
                    Raylib.DrawRectangle(ix - 8, py - 1, 15, 3, woodDark);
                    Raylib.DrawRectangle(ix - 7, py,     13, 1, woodLight);
                }
                // Szegecs sarkok
                Raylib.DrawRectangle(ix - 8, iy - 11, 3, 3, nail);
                Raylib.DrawRectangle(ix +  5, iy - 11, 3, 3, nail);
                Raylib.DrawRectangle(ix - 8, iy +  9, 3, 3, nail);
                Raylib.DrawRectangle(ix +  5, iy +  9, 3, 3, nail);
                Raylib.DrawRectangleLinesEx(new Rectangle(ix - 8, iy - 11, 15, 22), 1, new Color(55, 30, 8, 255));

                // Mennyiség
                Raylib.DrawText($"x{barricadeCount}", slotX + 38, slotY + 11, 18, new Color(200, 250, 90, 255));

                // Hint
                if (hovered)
                    Raylib.DrawText("klikk: lerak", slotX + 4, slotY + 26, 8, new Color(220, 255, 120, 200));
            } else {
                Raylib.DrawText("üres", slotX + slotW/2 - Raylib.MeasureText("üres", 12)/2,
                    slotY + 12, 12, new Color(50, 50, 50, 200));
            }

            Raylib.DrawText("[B]", slotX + slotW - 22, slotY + 2, 8,
                hasBarr ? new Color(140, 180, 60, 180) : new Color(50, 50, 50, 150));
        }

        private void DrawSection(int x, int y, int w, int h, string label) {
            Raylib.DrawRectangle(x, y, w, h, new Color(32, 32, 44, 255));
            Raylib.DrawRectangleLinesEx(new Rectangle(x, y, w, h), 1, new Color(75, 75, 95, 255));
            Raylib.DrawText(label, x + 3, y + 2, 9, new Color(145, 145, 165, 255));
        }

        public void Unload() {
            Raylib.UnloadTexture(spriteSheet);
            muzzleFlash.Unload();
        }
    }
}
