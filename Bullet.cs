using Raylib_cs;

namespace TheyAreComing {
    public class Bullet {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public bool IsActive { get; set; } = true;

        private const float Speed = 600f;
        private const float Radius = 3f;

        public Bullet(float startX, float startY, float directionX, float directionY) {
            X = startX;
            Y = startY;
            float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
            if (length > 0) {
                VelocityX = (directionX / length) * Speed;
                VelocityY = (directionY / length) * Speed;
            }
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
            Raylib.DrawCircle((int)X, (int)Y, Radius, Color.Yellow);
            Raylib.DrawCircle((int)X, (int)Y, Radius - 1, Color.White);
        }
    }
}
