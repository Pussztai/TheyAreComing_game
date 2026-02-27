using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class SoldierPlayer {
        private Texture2D spriteSheet;
        private int frameWidth  = 50;
        private int frameHeight = 50;
        private int currentFrame = 0;
        private int totalFrames  = 4;
        private float frameTimer = 0f;
        private float frameSpeed = 0.15f;

        public float X { get; set; } = 400;
        public float Y { get; set; } = 300;

        public int Money     { get; set; } = 50;
        public int Kills     { get; set; } = 0;
        public int Health    { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public float Speed { get; set; } = 220f;

        public List<Bullet> Bullets { get; private set; } = new();
        private float shootCooldown = 0f;
        public float ShootCooldownTime { get; set; } = 0.35f;
        public float BulletDamage      { get; set; } = 20f;
        public float Lifesteal         { get; set; } = 0f;

        public List<string> ActiveUpgrades { get; private set; } = new();

        private Vector2 aimDirection = new Vector2(1, 0);

        public int Ammo    { get; set; } = 30;
        public int MaxAmmo { get; set; } = 30;
        private float reloadTime  = 2f;
        private float reloadTimer = 0f;
        public bool IsReloading { get; private set; } = false;

        private float damageFlashTimer = 0f;

        public SoldierPlayer(string spriteSheetPath, int metaMaxHP = 0, float metaDmg = 0f, float metaSpd = 0f) {
            spriteSheet = Raylib.LoadTexture(spriteSheetPath);
            MaxHealth += metaMaxHP;
            Health     = MaxHealth;
            BulletDamage *= (1f + metaDmg);
            Speed        *= (1f + metaSpd);
            X = 400; Y = 300;
        }

        public void Update(float deltaTime) {
            // Újratöltés
            if (IsReloading) {
                reloadTimer -= deltaTime;
                if (reloadTimer <= 0) { Ammo = MaxAmmo; IsReloading = false; }
            }
            if (Raylib.IsKeyPressed(KeyboardKey.R) && !IsReloading && Ammo < MaxAmmo) StartReload();
            if (Ammo <= 0 && !IsReloading) StartReload();

            bool isMoving = false;
            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    { Y -= Speed * deltaTime; isMoving = true; }
            if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  { Y += Speed * deltaTime; isMoving = true; }
            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  { X -= Speed * deltaTime; isMoving = true; }
            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) { X += Speed * deltaTime; isMoving = true; }

            X = Math.Clamp(X, 50, 750);
            Y = Math.Clamp(Y, 50, 550);

            if (isMoving) {
                frameTimer += deltaTime;
                if (frameTimer >= frameSpeed) { currentFrame = (currentFrame + 1) % totalFrames; frameTimer = 0f; }
            } else {
                currentFrame = 0;
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            float dirX = mousePos.X - X, dirY = mousePos.Y - Y;
            float length = MathF.Sqrt(dirX * dirX + dirY * dirY);
            if (length > 0) { aimDirection.X = dirX / length; aimDirection.Y = dirY / length; }

            shootCooldown -= deltaTime;
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && shootCooldown <= 0 && !IsReloading && Ammo > 0) {
                Shoot();
                shootCooldown = ShootCooldownTime;
                Ammo--;
            }

            foreach (var b in Bullets) b.Update(deltaTime);
            Bullets.RemoveAll(b => !b.IsActive);
            damageFlashTimer -= deltaTime;
        }

        private void Shoot() {
            float bx = X + aimDirection.X * 25;
            float by = Y + aimDirection.Y * 25;
            Bullets.Add(new Bullet(bx, by, aimDirection.X, aimDirection.Y));
        }

        private void StartReload() {
            IsReloading = true;
            reloadTimer = reloadTime;
        }

        public void AddMoney(int amount) => Money += amount;
        public void AddKill()            => Kills++;

        public void TakeDamage(int damage) {
            Health = Math.Max(0, Health - damage);
            damageFlashTimer = 0.2f;
        }

        public void Heal(float amount) {
            Health = Math.Min(MaxHealth, Health + (int)amount);
        }

        public void Draw() {
            Rectangle source = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            Rectangle dest   = new Rectangle(X, Y, frameWidth * 2, frameHeight * 2);
            Color tint = damageFlashTimer > 0 ? new Color(255, 100, 100, 255) : Color.White;

            Raylib.DrawTexturePro(spriteSheet, source, dest,
                new Vector2(frameWidth, frameHeight), 0f, tint);

            float bw = 80f, bx = X - 40, by2 = Y - 60;
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
        }

        public void DrawTopBar(int wave, int zombieCount) {
            Raylib.DrawRectangle(0, 0, 800, 38, new Color(0, 0, 0, 215));
            Raylib.DrawRectangle(0, 38, 800, 2, new Color(75, 75, 85, 255));

            DrawSection(5,   4, 90,  30, "WAVE");
            Raylib.DrawText($"{wave}", 10, 19, 18, Color.White);

            DrawSection(103, 4, 135, 30, "HP");
            float hpPct = (float)Health / MaxHealth;
            Color hpC = hpPct > 0.5f ? new Color(50,200,50,255) : hpPct > 0.25f ? Color.Orange : Color.Red;
            Raylib.DrawRectangle(108, 21, 122, 11, new Color(25,25,25,255));
            Raylib.DrawRectangle(108, 21, (int)(122 * hpPct), 11, hpC);
            Raylib.DrawText($"{Health}/{MaxHealth}", 110, 22, 10, Color.White);

            DrawSection(246, 4, 135, 30, "AMMO");
            if (IsReloading) {
                Raylib.DrawText("RELOADING...", 251, 19, 13, Color.SkyBlue);
            } else {
                float aPct = (float)Ammo / MaxAmmo;
                Raylib.DrawRectangle(251, 21, 122, 11, new Color(25,25,25,255));
                Raylib.DrawRectangle(251, 21, (int)(122 * aPct), 11, aPct > 0.3f ? Color.Yellow : Color.Red);
                Raylib.DrawText($"{Ammo}/{MaxAmmo}", 253, 22, 10, Color.White);
            }

            DrawSection(389, 4, 95,  30, "KILLS");
            Raylib.DrawText($"{Kills}", 394, 19, 18, Color.Red);

            DrawSection(492, 4, 105, 30, "MONEY");
            Raylib.DrawText($"${Money}", 497, 19, 18, Color.Gold);

            DrawSection(605, 4, 105, 30, "ENEMIES");
            Raylib.DrawText($"{zombieCount}", 610, 19, 18, new Color(255, 80, 80, 255));

            if (ActiveUpgrades.Count > 0) {
                int ux = 718, uy = 4;
                Raylib.DrawRectangle(ux - 2, uy, 82, 34, new Color(28,28,40,255));
                Raylib.DrawText("UPGRADES", ux, uy + 2, 9, new Color(140,140,160,255));
                string upStr = ActiveUpgrades.Count <= 3
                    ? string.Join(", ", ActiveUpgrades.TakeLast(3))
                    : $"+{ActiveUpgrades.Count} buffs";
                Raylib.DrawText(upStr.Length > 12 ? upStr[..12] : upStr, ux, uy + 15, 9, Color.LightGray);
            }

            Raylib.DrawText($"{Raylib.GetFPS()} FPS", 10, 585, 12, Color.DarkGreen);
        }

        private void DrawSection(int x, int y, int w, int h, string label) {
            Raylib.DrawRectangle(x, y, w, h, new Color(32, 32, 44, 255));
            Raylib.DrawRectangleLinesEx(new Rectangle(x, y, w, h), 1, new Color(75, 75, 95, 255));
            Raylib.DrawText(label, x + 5, y + 3, 10, new Color(145, 145, 165, 255));
        }

        public void Unload() => Raylib.UnloadTexture(spriteSheet);
    }
}
