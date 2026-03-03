using Raylib_cs;

namespace TheyAreComing {
    public class Bullet {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public bool IsActive { get; set; } = true;

        private const float Speed = 1000f;
        private float radius;

        public Bullet(float startX, float startY, float directionX, float directionY, WeaponType weaponType = WeaponType.Pistol) {
            X = startX;
            Y = startY;
            float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
            if (length > 0) {
                VelocityX = (directionX / length) * Speed;
                VelocityY = (directionY / length) * Speed;
            }

            radius = weaponType switch {
                WeaponType.Rifle => 4.5f,
                WeaponType.Sniper => 5.5f,
                _ => 3f,
            };
        }

        public void Update(float deltaTime) {
            if (!IsActive) return;
            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
            if (X < -50 || X > 850 || Y < -50 || Y > 650)
                IsActive = false;
        }

        public void Draw() {
            if (!IsActive) return;
            Raylib.DrawCircle((int)X, (int)Y, radius, Color.Yellow);
            Raylib.DrawCircle((int)X, (int)Y, radius - 1, Color.White);
        }
    }
}