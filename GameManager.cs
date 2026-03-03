using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class GameManager {
        private GameState state = GameState.MainMenu;

        private MainMenu mainMenu = new();
        private GameMenu gameMenu = new();

        private SoldierPlayer? player;
        private WaveManager? waves;

        private Texture2D background;
        private bool backgroundLoaded = false;

        private UpgradeSystem upgradeSystem = new();
        private MetaShop metaShop = new();
        private WeaponShop weaponShop = new();
        private HowToPlay howToPlay = new();

        private List<Barricade> barricades = new();

        private float rewardTimer = 0f;
        private const float RewardDuration = 3f;
        private int lastRewardGold = 0;
        private Rectangle continueBtn;
        private Rectangle victoryPlayAgainBtn;
        private Rectangle victoryMenuBtn;

        private int sessionGold = 0;
        private int sessionKills = 0;

        private string? headshotMsg = null;
        private float headshotTimer = 0f;
        private float headshotX, headshotY;

        private float damageCooldown = 0f;
        private const float DamageCooldownTime = 0.7f;

        private const float HeadshotDamage = 10f;
        private const float BodyshotDamage = 5f;

        private bool isPlayground = false;
        private bool isHard = false;
        private int waveLimit = 7;

        private Rectangle pgExitBtn = new Rectangle(762, 0, 38, 38);

        public GameManager() {
            continueBtn = new Rectangle(300, 370, 200, 50);
            victoryPlayAgainBtn = new Rectangle(290, 430, 220, 55);
            victoryMenuBtn = new Rectangle(290, 500, 220, 35);
            LoadBackground();
        }

        private void LoadBackground() {
            string[] paths = {
                "background.png",
                "./background.png",
                System.IO.Path.Combine(AppContext.BaseDirectory, "background.png"),
            };
            foreach (var p in paths) {
                if (System.IO.File.Exists(p)) {
                    try {
                        background = Raylib.LoadTexture(p);
                        if (background.Id != 0) { backgroundLoaded = true; return; }
                    } catch { }
                }
            }
        }

        public void Update(float dt) {
            switch (state) {
                case GameState.MainMenu:
                    if (mainMenu.Update()) state = GameState.GameMenu;
                    break;

                case GameState.GameMenu:
                    var menuResult = gameMenu.Update();
                    if (menuResult == GameMenuResult.Campaign)
                        state = GameState.DifficultySelect;
                    if (menuResult == GameMenuResult.Playground)
                        StartNewRun(playground: true, hard: false);
                    if (gameMenu.HowToPlayClicked) {
                        howToPlay.Open();
                        state = GameState.HowToPlay;
                    }
                    break;

                case GameState.DifficultySelect:
                    if (gameMenu.UpdateDifficultyBack())
                        state = GameState.GameMenu;
                    var diff = gameMenu.UpdateDifficulty();
                    if (diff == DifficultyResult.Normal) StartNewRun(playground: false, hard: false);
                    if (diff == DifficultyResult.Hard) StartNewRun(playground: false, hard: true);
                    break;

                case GameState.Playing:
                case GameState.Playground:
                    UpdatePlaying(dt);
                    break;

                case GameState.WaveReward:
                    rewardTimer -= dt;
                    Vector2 mp1 = Raylib.GetMousePosition();
                    bool clickContinue = Raylib.CheckCollisionPointRec(mp1, continueBtn)
                                        && Raylib.IsMouseButtonPressed(MouseButton.Left);
                    if (rewardTimer <= 0 || clickContinue) {
                        waves!.StartNextWave();
                        state = isPlayground ? GameState.Playground : GameState.Playing;
                    }
                    break;

                case GameState.WeaponShopState:
                    weaponShop.Update(player!);
                    if (weaponShop.IsClosed) {
                        rewardTimer = RewardDuration;
                        state = GameState.WaveReward;
                    }
                    break;

                case GameState.UpgradeSelect:
                    var chosen = upgradeSystem.Update(player!);
                    if (chosen != null && upgradeSystem.ContinueClicked) {
                        upgradeSystem.ApplyUpgrade(chosen, player!);
                        weaponShop.Open();
                        state = GameState.WeaponShopState;
                    }
                    break;

                case GameState.HowToPlay:
                    howToPlay.Update();
                    if (howToPlay.ExitClicked) state = GameState.GameMenu;
                    break;

                case GameState.Victory:
                    UpdateVictory();
                    break;

                case GameState.GameOver:
                    metaShop.AddSessionGold(sessionGold);
                    bool playAgain = metaShop.Update();
                    if (playAgain) state = GameState.DifficultySelect;
                    if (metaShop.IsMainMenuClicked()) {
                        metaShop.ResetSession();
                        state = GameState.MainMenu;
                    }
                    break;
            }
        }

        public void Draw() {
            switch (state) {
                case GameState.MainMenu: mainMenu.Draw(); break;
                case GameState.GameMenu: gameMenu.Draw(); break;
                case GameState.DifficultySelect: gameMenu.DrawDifficulty(); break;
                case GameState.HowToPlay: howToPlay.Draw(); break;
                case GameState.Playing: DrawGame(); break;
                case GameState.Playground: DrawGame(); DrawPlaygroundExitBtn(); break;
                case GameState.WaveReward: DrawGame(); DrawRewardOverlay(); break;
                case GameState.WeaponShopState: DrawGame(); weaponShop.Draw(player!); break;
                case GameState.UpgradeSelect: DrawGame(); upgradeSystem.Draw(player!); break;
                case GameState.Victory: DrawVictory(); break;
                case GameState.GameOver: metaShop.Draw(waves?.CurrentWave ?? 0, sessionKills, sessionGold); break;
            }
        }

        private void UpdateVictory() {
            Vector2 mp = Raylib.GetMousePosition();
            if (Raylib.CheckCollisionPointRec(mp, victoryPlayAgainBtn)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                state = GameState.DifficultySelect;
            if (Raylib.CheckCollisionPointRec(mp, victoryMenuBtn)
                && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                metaShop.ResetSession();
                state = GameState.MainMenu;
            }
        }

        private void DrawVictory() {
            Raylib.ClearBackground(new Color(10, 10, 20, 255));

            string t = "YOU WIN!";
            Raylib.DrawText(t, 400 - Raylib.MeasureText(t, 56) / 2, 45, 56, Color.Green);

            string diff = isHard
                ? "HARD mode – 15 waves completed!"
                : "NORMAL mode – 7 waves completed!";
            Raylib.DrawText(diff, 400 - Raylib.MeasureText(diff, 20) / 2, 115, 20,
                isHard ? new Color(255, 100, 100, 255) : new Color(100, 220, 100, 255));

            string stats = $"Wave {waves?.CurrentWave ?? 0}  |  {sessionKills} Kills  |  Session: ${sessionGold}";
            Raylib.DrawText(stats, 400 - Raylib.MeasureText(stats, 20) / 2, 148, 20, Color.LightGray);

            string goldTxt = $"Total Gold: ${metaShop.TotalGold}";
            Raylib.DrawText(goldTxt, 400 - Raylib.MeasureText(goldTxt, 26) / 2, 185, 26, Color.Gold);

            Raylib.DrawRectangle(100, 220, 600, 2, new Color(50, 180, 50, 200));

            string congrats = "Congratulations, you survived the horde!";
            Raylib.DrawText(congrats, 400 - Raylib.MeasureText(congrats, 18) / 2, 235, 18,
                new Color(180, 255, 180, 255));

            Vector2 mp = Raylib.GetMousePosition();

            bool overPlay = Raylib.CheckCollisionPointRec(mp, victoryPlayAgainBtn);
            Raylib.DrawRectangleRec(victoryPlayAgainBtn,
                overPlay ? new Color(50, 190, 50, 255) : new Color(30, 130, 30, 255));
            Raylib.DrawRectangleLinesEx(victoryPlayAgainBtn, 2, Color.Green);
            string pt = "PLAY AGAIN";
            Raylib.DrawText(pt,
                (int)victoryPlayAgainBtn.X + (int)victoryPlayAgainBtn.Width / 2 - Raylib.MeasureText(pt, 22) / 2,
                (int)victoryPlayAgainBtn.Y + 16, 22, Color.White);

            bool overMenu = Raylib.CheckCollisionPointRec(mp, victoryMenuBtn);
            Raylib.DrawRectangleRec(victoryMenuBtn,
                overMenu ? new Color(60, 60, 80, 255) : new Color(35, 35, 50, 255));
            Raylib.DrawRectangleLinesEx(victoryMenuBtn, 1, Color.Gray);
            string mt = "MAIN MENU";
            Raylib.DrawText(mt,
                (int)victoryMenuBtn.X + (int)victoryMenuBtn.Width / 2 - Raylib.MeasureText(mt, 16) / 2,
                (int)victoryMenuBtn.Y + 10, 16, Color.LightGray);
        }

        private void DrawPlaygroundExitBtn() {
            Vector2 mp = Raylib.GetMousePosition();
            bool over = Raylib.CheckCollisionPointRec(mp, pgExitBtn);
            Raylib.DrawRectangleRec(pgExitBtn, over
                ? new Color(200, 30, 30, 240)
                : new Color(140, 20, 20, 200));
            Raylib.DrawRectangle((int)pgExitBtn.X, 0, 1, 38, new Color(75, 75, 85, 255));
            int cx = (int)(pgExitBtn.X + pgExitBtn.Width / 2);
            int cy = (int)(pgExitBtn.Y + pgExitBtn.Height / 2);
            int half = 8;
            Raylib.DrawLineEx(new Vector2(cx - half, cy - half), new Vector2(cx + half, cy + half), 2.5f, Color.White);
            Raylib.DrawLineEx(new Vector2(cx + half, cy - half), new Vector2(cx - half, cy + half), 2.5f, Color.White);
        }

        private void PlaceBarricade() {
            if (player == null) return;
            if (upgradeSystem.BarricadeCount <= 0) return;
            if (!upgradeSystem.UseBarricade()) return;
            barricades.Add(new Barricade(player.X + 60, player.Y));
        }

        private void UpdatePlaying(float dt) {
            if (player == null || waves == null) return;

            if (isPlayground) {
                Vector2 mpx = Raylib.GetMousePosition();
                if (Raylib.CheckCollisionPointRec(mpx, pgExitBtn)
                    && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                    player.Unload(); waves.Unload(); barricades.Clear();
                    player = null; waves = null; isPlayground = false;
                    state = GameState.GameMenu;
                    return;
                }
            }

            bool hotbarClicked = player.DrawBottomHotbarInput(upgradeSystem.BarricadeCount);
            if ((Raylib.IsKeyPressed(KeyboardKey.B) || hotbarClicked) && upgradeSystem.BarricadeCount > 0)
                PlaceBarricade();

            player.Update(dt);
            waves.Update(dt);

            foreach (var barr in barricades) barr.Update(dt);
            foreach (var zombie in waves.Zombies) zombie.SetBarricades(barricades);

            foreach (var zombie in waves.Zombies) {
                if (!zombie.IsAlive) continue;
                var blocker = zombie.CurrentBlocker;
                if (blocker != null && blocker.IsAlive && zombie.CanHitBarricade())
                    blocker.TakeDamage(zombie.BarricadeDamage());
            }

            foreach (var bullet in player.Bullets.ToList()) {
                if (!bullet.IsActive) continue;
                foreach (var barr in barricades) {
                    if (!barr.IsAlive) continue;
                    if (barr.CheckBulletCollision(bullet.X, bullet.Y)) { bullet.IsActive = false; break; }
                }
            }

            barricades.RemoveAll(b => !b.IsAlive);

            foreach (var bullet in player.Bullets.ToList()) {
                if (!bullet.IsActive) continue;
                foreach (var zombie in waves.Zombies.ToList()) {
                    if (!zombie.IsAlive) continue;
                    if (zombie.CheckCollisionWithBullet(bullet.X, bullet.Y, out bool isHeadshot)) {
                        float dmg = (isHeadshot ? HeadshotDamage : BodyshotDamage)
                                    * (player.BulletDamage / 20f);
                        zombie.TakeDamage(dmg);
                        bullet.IsActive = false;
                        if (player.Lifesteal > 0) player.Heal(dmg * player.Lifesteal);
                        if (isHeadshot) {
                            headshotMsg = "HEADSHOT!"; headshotX = zombie.X;
                            headshotY = zombie.Y - 40; headshotTimer = 0.8f;
                        }
                        if (!zombie.IsAlive) {
                            if (!isPlayground) { player.AddMoney(zombie.Reward); sessionGold += zombie.Reward; }
                            player.AddKill(); sessionKills++;
                        }
                        break;
                    }
                }
            }

            damageCooldown -= dt;
            if (damageCooldown <= 0) {
                foreach (var zombie in waves.Zombies) {
                    if (zombie.CheckCollisionWithPlayer(player)) {
                        damageCooldown = DamageCooldownTime; break;
                    }
                }
            }

            headshotTimer -= dt;

            if (player.Health <= 0) {
                if (!isPlayground) { state = GameState.GameOver; } else { player.Health = player.MaxHealth; }
                return;
            }

            if (waves.WaveCleared) {
                int bonus = waves.CurrentWave * 25;
                if (!isPlayground) {
                    player.AddMoney(bonus);
                    sessionGold += bonus;
                }
                lastRewardGold = bonus;

                if (!isPlayground && waves.CurrentWave >= waveLimit) {
                    metaShop.AddSessionGold(sessionGold);
                    state = GameState.Victory;
                    return;
                }

                if (isPlayground) {
                    upgradeSystem.GenerateOptions();
                    state = GameState.UpgradeSelect;
                } else {
                    if (waves.CurrentWave % 3 == 0) {
                        upgradeSystem.GenerateOptions();
                        state = GameState.UpgradeSelect;
                    } else {
                        rewardTimer = RewardDuration;
                        state = GameState.WaveReward;
                    }
                }
            }
        }

        private void DrawGame() {
            Raylib.ClearBackground(new Color(22, 22, 30, 255));

            if (backgroundLoaded) {
                float scaleX = 800f / background.Width;
                float scaleY = 600f / background.Height;
                float scale = MathF.Max(scaleX, scaleY);
                float drawW = background.Width * scale;
                float drawH = background.Height * scale;
                float offX = (800f - drawW) / 2f;
                float offY = (600f - drawH) / 2f;
                Raylib.DrawTexturePro(background,
                    new Rectangle(0, 0, background.Width, background.Height),
                    new Rectangle(offX, offY, drawW, drawH),
                    Vector2.Zero, 0f, Color.White);
                Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 80));
            } else {
                for (int x = 0; x < 800; x += 60)
                    Raylib.DrawLine(x, 40, x, 600, new Color(30, 30, 40, 255));
                for (int y = 40; y < 600; y += 60)
                    Raylib.DrawLine(0, y, 800, y, new Color(30, 30, 40, 255));
            }

            foreach (var b in barricades) b.Draw();
            waves?.Draw();
            player?.Draw();

            if (headshotTimer > 0 && headshotMsg != null) {
                byte a = (byte)(255 * (headshotTimer / 0.8f));
                Raylib.DrawText(headshotMsg, (int)headshotX - 40, (int)headshotY, 20,
                    new Color((byte)255, (byte)80, (byte)0, a));
            }

            player?.DrawTopBar(waves?.CurrentWave ?? 0,
                               waves?.Zombies.Count(z => z.IsAlive) ?? 0,
                               isPlayground);

            if (player != null)
                player.DrawBottomHotbar(upgradeSystem.BarricadeCount);

            if (!isPlayground) {
                string limTxt = $"GOAL: {waves?.CurrentWave ?? 0}/{waveLimit}";
                Color limCol = isHard ? new Color(255, 80, 80, 220) : new Color(255, 200, 80, 220);
                Raylib.DrawText(limTxt, 800 - Raylib.MeasureText(limTxt, 11) - 44, 2, 11, limCol);
            }
        }

        private void DrawRewardOverlay() {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 165));

            string wc = $"WAVE {waves?.CurrentWave ?? 0} CLEARED!";
            Raylib.DrawText(wc, 400 - Raylib.MeasureText(wc, 44) / 2, 155, 44, Color.Gold);

            if (!isPlayground) {
                string reward = $"Wave Bonus: +${lastRewardGold}";
                Raylib.DrawText(reward, 400 - Raylib.MeasureText(reward, 28) / 2, 218, 28, Color.Green);
                string total = $"Total Money: ${player?.Money ?? 0}";
                Raylib.DrawText(total, 400 - Raylib.MeasureText(total, 22) / 2, 260, 22, Color.Gold);
                int remaining = waveLimit - (waves?.CurrentWave ?? 0);
                string remTxt = remaining > 0
                    ? $"{remaining} wave(s) left until victory!"
                    : "Next wave is the last one!";
                Raylib.DrawText(remTxt, 400 - Raylib.MeasureText(remTxt, 16) / 2, 292, 16,
                    isHard ? new Color(255, 120, 120, 220) : new Color(120, 220, 120, 220));
            } else {
                string pgInfo = "PLAYGROUND MODE – Infinite money";
                Raylib.DrawText(pgInfo, 400 - Raylib.MeasureText(pgInfo, 22) / 2, 218, 22, Color.SkyBlue);
            }

            if (rewardTimer > 0) {
                string timer = $"Next wave in {(int)rewardTimer + 1}s...";
                Raylib.DrawText(timer, 400 - Raylib.MeasureText(timer, 17) / 2, 320, 17, Color.LightGray);
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

        private void StartNewRun(bool playground, bool hard) {
            player?.Unload();
            waves?.Unload();
            barricades.Clear();
            isPlayground = playground;
            isHard = hard;
            waveLimit = hard ? 15 : 7;

            player = new SoldierPlayer(
                "playerImg_default_clean.png",
                metaShop.PermanentMaxHPBonus,
                metaShop.PermanentDamageBonus,
                metaShop.PermanentSpeedBonus,
                playground
            );
            waves = new WaveManager(player);
            sessionGold = 0;
            sessionKills = 0;
            metaShop.ResetSession();
            upgradeSystem = new UpgradeSystem();
            waves.StartNextWave();
            state = playground ? GameState.Playground : GameState.Playing;
        }

        public void Unload() {
            player?.Unload();
            waves?.Unload();
            if (backgroundLoaded) Raylib.UnloadTexture(background);
        }
    }
}