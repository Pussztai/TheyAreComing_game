using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public enum ZombieType  { Normal, Fast, Tank, Boss }
    public enum ZombieState { Walking, Crawling, Dead }

    public class Zombie {
        private static Texture2D? spriteSheet = null;
        private int frameWidth  = 50;
        private int frameHeight = 50;
        private int currentFrame = 0;
        private int totalFrames  = 4;

        private float frameTimer = 0f;
        private float frameSpeed = 0.2f;

        public float X         { get; set; }
        public float Y         { get; set; }
        public float Health    { get; set; }
        public float MaxHealth { get; set; }
        public bool  IsAlive   { get; set; } = true;
        public int   Reward    { get; private set; }
        public int   Damage    { get; private set; }

        private float speed;
        private float originalSpeed;
        public float Size  { get; private set; } = 25f;

        public ZombieType  Type  { get; private set; }
        public ZombieState State { get; private set; } = ZombieState.Walking;

        private SoldierPlayer? target = null;
        private Color zombieColor;
        public float Scale { get; private set; }

        private float headshotFlashTimer = 0f;

        public static void LoadSprite(string path) {
            if (spriteSheet == null || spriteSheet.Value.Id == 0)
                spriteSheet = Raylib.LoadTexture(path);
        }

        public static void UnloadSprite() {
            if (spriteSheet != null && spriteSheet.Value.Id != 0) {
                Raylib.UnloadTexture(spriteSheet.Value);
                spriteSheet = null;
            }
        }

        public Zombie(float startX, float startY, ZombieType type = ZombieType.Normal) {
            X = startX; Y = startY; Type = type;

            switch (type) {
                case ZombieType.Normal:
                    MaxHealth = 50f;  speed = 60f;  Damage = 10; Reward = 10;
                    zombieColor = new Color(100, 150, 90, 255);
                    Scale = 2f; Size = 25f;
                    break;
                case ZombieType.Fast:
                    MaxHealth = 30f;  speed = 100f; Damage = 8;  Reward = 15;
                    zombieColor = new Color(200, 150, 50, 255);
                    Scale = 1.8f; Size = 22f; frameSpeed = 0.12f;
                    break;
                case ZombieType.Tank:
                    MaxHealth = 150f; speed = 35f;  Damage = 20; Reward = 30;
                    zombieColor = new Color(80, 80, 100, 255);
                    Scale = 2.5f; Size = 35f; frameSpeed = 0.25f;
                    break;
                case ZombieType.Boss:
                    MaxHealth = 300f; speed = 50f;  Damage = 30; Reward = 100;
                    zombieColor = new Color(255, 100, 100, 255);
                    Scale = 3f; Size = 40f;
                    break;
            }

            Health = MaxHealth;
            originalSpeed = speed;
        }

        public void SetTarget(SoldierPlayer player) => target = player;

        public void Update(float deltaTime) {
            if (!IsAlive) return;

            float healthPercent = Health / MaxHealth;
            if (healthPercent <= 0.5f && State == ZombieState.Walking) {
                State  = ZombieState.Crawling;
                speed  = originalSpeed * 0.4f;
                Scale *= 0.7f;
                Damage = (int)(Damage * 0.6f);
            }

            if (target != null) {
                float dirX = target.X - X, dirY = target.Y - Y;
                float distance = MathF.Sqrt(dirX * dirX + dirY * dirY);
                if (distance > 5f) {
                    X += (dirX / distance) * speed * deltaTime;
                    Y += (dirY / distance) * speed * deltaTime;
                }
            }

            frameTimer += deltaTime;
            if (frameTimer >= frameSpeed) { currentFrame = (currentFrame + 1) % totalFrames; frameTimer = 0f; }

            headshotFlashTimer -= deltaTime;
        }

        public void TakeDamage(float damage) {
            Health -= damage;
            if (Health <= 0) { Health = 0; IsAlive = false; State = ZombieState.Dead; }
        }

        public bool CheckCollisionWithBullet(float bulletX, float bulletY, out bool isHeadshot) {
            isHeadshot = false;
            if (!IsAlive) return false;

            float dx = X - bulletX, dy = Y - bulletY;
            if (MathF.Sqrt(dx * dx + dy * dy) < Size) {
                float spriteHalfH = frameHeight * Scale / 2f;
                float headTop     = Y - spriteHalfH;
                float headBottom  = headTop + spriteHalfH * 0.8f;

                if (bulletY >= headTop && bulletY <= headBottom) {
                    isHeadshot = true;
                    headshotFlashTimer = 0.25f;
                }
                return true;
            }
            return false;
        }

        public bool CheckCollisionWithBullet(float bulletX, float bulletY) =>
            CheckCollisionWithBullet(bulletX, bulletY, out _);

        public bool CheckCollisionWithPlayer(SoldierPlayer player) {
            if (!IsAlive) return false;
            float dx = X - player.X, dy = Y - player.Y;
            return MathF.Sqrt(dx * dx + dy * dy) < (Size + 30);
        }

        public void Draw() {
            if (!IsAlive || spriteSheet == null || spriteSheet.Value.Id == 0) return;

            Rectangle source = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            float drawY = Y + (State == ZombieState.Crawling ? 15 : 0);
            Rectangle dest = new Rectangle(X, drawY, frameWidth * Scale, frameHeight * Scale);

            Color tint = zombieColor;
            if (headshotFlashTimer > 0) {
                tint = new Color((byte)255, (byte)220, (byte)0, (byte)255);
            } else if (State == ZombieState.Crawling) {
                tint = new Color(
                    (byte)((int)tint.R * 7 / 10),
                    (byte)((int)tint.G * 7 / 10),
                    (byte)((int)tint.B * 7 / 10),
                    (byte)255);
            }

            Raylib.DrawTexturePro(spriteSheet.Value, source, dest,
                new Vector2(frameWidth * Scale / 2, frameHeight * Scale / 2), 0f, tint);

            float barWidth = frameWidth * Scale;
            float barX = X - barWidth / 2;
            float barY = Y - frameHeight * Scale / 2 - 15;

            Raylib.DrawRectangle((int)barX, (int)barY, (int)barWidth, 6, Color.Red);
            float hp = Health / MaxHealth;
            Color hpColor = Type == ZombieType.Boss ? Color.Gold : (hp > 0.5f ? Color.Green : Color.Orange);
            Raylib.DrawRectangle((int)barX, (int)barY, (int)(barWidth * hp), 6, hpColor);

            string typeIcon = Type switch {
                ZombieType.Fast => "F", ZombieType.Tank => "T", ZombieType.Boss => "B", _ => ""
            };
            if (!string.IsNullOrEmpty(typeIcon))
                Raylib.DrawText(typeIcon, (int)X - 6, (int)barY - 18, 14, Color.White);
        }
    }
}
