using Raylib_cs;

namespace TheyAreComing {
    public class WaveManager {
        public int CurrentWave { get; private set; } = 0;
        public List<Zombie> Zombies { get; private set; } = new();
        public bool WaveCleared { get; private set; } = false;

        private SoldierPlayer player;

        private int toSpawn = 0;
        private float spawnTimer = 0f;
        private float spawnInterval = 1.2f;
        private bool waveActive = false;

        private float damageCooldown = 0f;
        private const float DamageCooldownTime = 0.8f;

        private float zombieHealthMultiplier = 1f;

        
        private const float ZoneMinY = SoldierPlayer.AreaMinY;
        private const float ZoneMaxY = SoldierPlayer.AreaMaxY;

        public WaveManager(SoldierPlayer player) {
            this.player = player;
            Zombie.LoadSprite("zombie_walking_spritesheet.png");
        }

        public void StartNextWave() {
            CurrentWave++;
            WaveCleared = false;
            waveActive  = true;

            zombieHealthMultiplier = 1f + (CurrentWave - 1) * 0.25f;

            int baseCount = CurrentWave switch {
                1 => 4,
                2 => 7,
                3 => 11,
                _ => (int)(12 * MathF.Pow(1.3f, CurrentWave - 3))
            };
            toSpawn = baseCount;

            spawnInterval = CurrentWave switch {
                1 => 2.5f,
                2 => 1.8f,
                3 => 1.3f,
                _ => Math.Max(0.3f, 1.2f - (CurrentWave - 3) * 0.08f)
            };
            spawnTimer = 0f;
            Zombies.Clear();

            Console.WriteLine($"Wave {CurrentWave} started — spawning {toSpawn} zombies");
        }

        public void Update(float dt) {
            foreach (var z in Zombies) z.Update(dt);
            Zombies.RemoveAll(z => !z.IsAlive);

            damageCooldown -= dt;
            if (damageCooldown <= 0) {
                foreach (var z in Zombies) {
                    if (z.CheckCollisionWithPlayer(player)) {
                        player.TakeDamage(z.Damage);
                        damageCooldown = DamageCooldownTime;
                        break;
                    }
                }
            }

            if (waveActive && toSpawn > 0) {
                spawnTimer += dt;
                if (spawnTimer >= spawnInterval) {
                    SpawnZombie();
                    toSpawn--;
                    spawnTimer = 0f;
                }
            }

            if (waveActive && toSpawn <= 0 && Zombies.Count == 0) {
                waveActive  = false;
                WaveCleared = true;
            }
        }

        private void SpawnZombie() {
            
            float sx = 1000f + Random.Shared.NextSingle() * 100f;
            float sy = Random.Shared.NextSingle() * (ZoneMaxY - ZoneMinY) + ZoneMinY;

            ZombieType type;
            int r = Random.Shared.Next(100);

            if (CurrentWave % 5 == 0 && Random.Shared.Next(4) == 0) {
                type = ZombieType.Boss;
            } else if (CurrentWave >= 3 && r < 15) {
                type = ZombieType.Tank;
            } else if (CurrentWave >= 2 && r < 45) {
                type = ZombieType.Fast;
            } else {
                type = ZombieType.Normal;
            }

            var zombie = new Zombie(sx, sy, type);
            zombie.Health    *= zombieHealthMultiplier;
            zombie.MaxHealth *= zombieHealthMultiplier;
            zombie.SetTarget(player);
            zombie.SetMovementZone(SoldierPlayer.AreaMinX, SoldierPlayer.AreaMaxX,
                                   ZoneMinY, ZoneMaxY);
            Zombies.Add(zombie);
        }

        public void Draw() {
            foreach (var z in Zombies) z.Draw();
        }

        public bool IsPlayerDead() => player.Health <= 0;

        public void Unload() => Zombie.UnloadSprite();
    }
}
