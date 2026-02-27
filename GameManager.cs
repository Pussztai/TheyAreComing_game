using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class GameManager {
        private GameState state = GameState.MainMenu;

        private MainMenu   mainMenu  = new();
        private GameMenu   gameMenu  = new();

        private SoldierPlayer? player;
        private WaveManager?   waves;

        private UpgradeSystem upgradeSystem = new();
        private MetaShop      metaShop      = new();

        private float rewardTimer       = 0f;
        private const float RewardDuration = 3f;
        private int   lastRewardGold    = 0;
        private Rectangle continueBtn;

        private int sessionGold  = 0;
        private int sessionKills = 0;

        private string? headshotMsg   = null;
        private float   headshotTimer = 0f;
        private float   headshotX, headshotY;

        private float damageCooldown = 0f;
        private const float DamageCooldownTime = 0.7f;

        private const float HeadshotDamage = 10f;
        private const float BodyshotDamage = 5f;

        public GameManager() {
            continueBtn = new Rectangle(300, 370, 200, 50);
        }

        public void Update(float dt) {
            switch (state) {
                case GameState.MainMenu:
                    if (mainMenu.Update())
                        state = GameState.GameMenu;
                    break;

                case GameState.GameMenu:
                    if (gameMenu.Update())
                        StartNewRun();
                    break;

                case GameState.Playing:
                    UpdatePlaying(dt);
                    break;

                case GameState.WaveReward:
                    rewardTimer -= dt;
                    Vector2 mp1 = Raylib.GetMousePosition();
                    bool clickContinue = Raylib.CheckCollisionPointRec(mp1, continueBtn)
                                        && Raylib.IsMouseButtonPressed(MouseButton.Left);
                    if (rewardTimer <= 0 || clickContinue) {
                        waves!.StartNextWave();
                        state = GameState.Playing;
                    }
                    break;

                case GameState.UpgradeSelect:
                    var chosen = upgradeSystem.Update(player!);
                    if (chosen != null && upgradeSystem.ContinueClicked) {
                        upgradeSystem.ApplyUpgrade(chosen, player!);
                        rewardTimer = RewardDuration;
                        state = GameState.WaveReward;
                    }
                    break;

                case GameState.GameOver:
                    metaShop.AddSessionGold(sessionGold);
                    bool playAgain = metaShop.Update();
                    if (playAgain) StartNewRun();
                    if (metaShop.IsMainMenuClicked()) {
                        metaShop.ResetSession();
                        state = GameState.MainMenu;
                    }
                    break;
            }
        }

        public void Draw() {
            switch (state) {
                case GameState.MainMenu:      mainMenu.Draw();                                                    break;
                case GameState.GameMenu:      gameMenu.Draw();                                                    break;
                case GameState.Playing:       DrawGame();                                                         break;
                case GameState.WaveReward:    DrawGame(); DrawRewardOverlay();                                    break;
                case GameState.UpgradeSelect: DrawGame(); upgradeSystem.Draw(player!);                                   break;
                case GameState.GameOver:      metaShop.Draw(waves?.CurrentWave ?? 0, sessionKills, sessionGold); break;
            }
        }

        private void UpdatePlaying(float dt) {
            if (player == null || waves == null) return;

            player.Update(dt);
            waves.Update(dt);

            foreach (var bullet in player.Bullets.ToList()) {
                if (!bullet.IsActive) continue;
                foreach (var zombie in waves.Zombies.ToList()) {
                    if (!zombie.IsAlive) continue;
                    if (zombie.CheckCollisionWithBullet(bullet.X, bullet.Y, out bool isHeadshot)) {
                        float dmg = (isHeadshot ? HeadshotDamage : BodyshotDamage)
                                    * (player.BulletDamage / 20f);
                        zombie.TakeDamage(dmg);
                        bullet.IsActive = false;

                        if (player.Lifesteal > 0)
                            player.Heal(dmg * player.Lifesteal);

                        if (isHeadshot) {
                            headshotMsg   = "HEADSHOT!";
                            headshotX     = zombie.X;
                            headshotY     = zombie.Y - 40;
                            headshotTimer = 0.8f;
                        }

                        if (!zombie.IsAlive) {
                            player.AddMoney(zombie.Reward);
                            player.AddKill();
                            sessionGold  += zombie.Reward;
                            sessionKills++;
                        }
                        break;
                    }
                }
            }

            damageCooldown -= dt;
            if (damageCooldown <= 0) {
                foreach (var zombie in waves.Zombies) {
                    if (zombie.CheckCollisionWithPlayer(player)) {
                        damageCooldown = DamageCooldownTime;
                        break;
                    }
                }
            }

            headshotTimer -= dt;

            if (player.Health <= 0) {
                state = GameState.GameOver;
                return;
            }

            if (waves.WaveCleared) {
                int bonus = waves.CurrentWave * 25;
                player.AddMoney(bonus);
                sessionGold   += bonus;
                lastRewardGold = bonus;

                if (waves.CurrentWave % 3 == 0) {
                    upgradeSystem.GenerateOptions();
                    state = GameState.UpgradeSelect;
                } else {
                    rewardTimer = RewardDuration;
                    state = GameState.WaveReward;
                }
            }
        }

        private void DrawGame() {
            Raylib.ClearBackground(new Color(22, 22, 30, 255));

            // Háttér grid
            for (int x = 0; x < 800; x += 60)
                Raylib.DrawLine(x, 40, x, 600, new Color(30, 30, 40, 255));
            for (int y = 40; y < 600; y += 60)
                Raylib.DrawLine(0, y, 800, y, new Color(30, 30, 40, 255));

            waves?.Draw();
            player?.Draw();

            if (headshotTimer > 0 && headshotMsg != null) {
                byte a = (byte)(255 * (headshotTimer / 0.8f));
                Raylib.DrawText(headshotMsg, (int)headshotX - 40, (int)headshotY, 20,
                    new Color((byte)255, (byte)80, (byte)0, a));
            }

            player?.DrawTopBar(waves?.CurrentWave ?? 0, waves?.Zombies.Count(z => z.IsAlive) ?? 0);

            Raylib.DrawRectangle(0, 587, 800, 13, new Color(0, 0, 0, 170));
            Raylib.DrawText("WASD-Move  |  LMB-Shoot  |  R-Reload",
                400 - Raylib.MeasureText("WASD-Move  |  LMB-Shoot  |  R-Reload", 11) / 2,
                589, 11, new Color(160, 160, 160, 255));
        }

        private void DrawRewardOverlay() {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 165));

            string wc = $"WAVE {waves?.CurrentWave ?? 0} CLEARED!";
            Raylib.DrawText(wc, 400 - Raylib.MeasureText(wc, 44) / 2, 155, 44, Color.Gold);

            string reward = $"Wave Bonus: +${lastRewardGold}";
            Raylib.DrawText(reward, 400 - Raylib.MeasureText(reward, 28) / 2, 218, 28, Color.Green);

            string total = $"Total Money: ${player?.Money ?? 0}";
            Raylib.DrawText(total, 400 - Raylib.MeasureText(total, 22) / 2, 260, 22, Color.Gold);

            if (rewardTimer > 0) {
                string timer = $"Next wave in {(int)rewardTimer + 1}s...";
                Raylib.DrawText(timer, 400 - Raylib.MeasureText(timer, 17) / 2, 310, 17, Color.LightGray);
            }

            Vector2 mp = Raylib.GetMousePosition();
            bool over = Raylib.CheckCollisionPointRec(mp, continueBtn);
            Raylib.DrawRectangleRec(continueBtn, over ? new Color(50, 200, 50, 255) : new Color(30, 140, 30, 255));
            Raylib.DrawRectangleLinesEx(continueBtn, 2, Color.Green);
            string ct = "CONTINUE";
            Raylib.DrawText(ct,
                (int)continueBtn.X + (int)continueBtn.Width / 2 - Raylib.MeasureText(ct, 22) / 2,
                (int)continueBtn.Y + 14, 22, Color.White);
        }

        private void StartNewRun() {
            player?.Unload();
            waves?.Unload();

            player = new SoldierPlayer(
                "soldier_pistol_spritesheet.png",
                metaShop.PermanentMaxHPBonus,
                metaShop.PermanentDamageBonus,
                metaShop.PermanentSpeedBonus
            );
            waves        = new WaveManager(player);
            sessionGold  = 0;
            sessionKills = 0;
            metaShop.ResetSession();
            waves.StartNextWave();
            state = GameState.Playing;
        }

        public void Unload() {
            player?.Unload();
            waves?.Unload();
        }
    }
}
