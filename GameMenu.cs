using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public enum GameMenuResult { None, Campaign, Playground }
    public enum DifficultyResult { None, Normal, Hard }

    public class GameMenu {
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 600;

        private Rectangle newCampaignButton;
        private Rectangle playgroundButton;
        private Rectangle rankingButton;
        private Rectangle shopButton;
        private Rectangle howToPlayButton;

        private Rectangle normalBtn;
        private Rectangle hardBtn;
        private Rectangle backBtn;

        public bool HowToPlayClicked { get; private set; } = false;

        public GameMenu() {
            newCampaignButton = new Rectangle(ScreenWidth / 2 - 210, ScreenHeight / 2 - 50, 200, 50);
            playgroundButton = new Rectangle(ScreenWidth / 2 + 10, ScreenHeight / 2 - 50, 200, 50);
            rankingButton = new Rectangle(ScreenWidth / 2 - 200, ScreenHeight / 2 + 70, 120, 40);
            shopButton = new Rectangle(ScreenWidth / 2 - 60, ScreenHeight / 2 + 70, 120, 40);
            howToPlayButton = new Rectangle(ScreenWidth / 2 + 80, ScreenHeight / 2 + 70, 120, 40);

            normalBtn = new Rectangle(ScreenWidth / 2 - 220, ScreenHeight / 2 - 60, 200, 120);
            hardBtn = new Rectangle(ScreenWidth / 2 + 20, ScreenHeight / 2 - 60, 200, 120);
            backBtn = new Rectangle(ScreenWidth / 2 - 60, ScreenHeight / 2 + 100, 120, 36);
        }

        public GameMenuResult Update() {
            HowToPlayClicked = false;
            Vector2 mousePos = Raylib.GetMousePosition();

            if (Raylib.CheckCollisionPointRec(mousePos, shopButton)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                OpenUrl("https://github.com/Pussztai");

            if (Raylib.CheckCollisionPointRec(mousePos, howToPlayButton)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                OpenUrl("https://discord.gg/");

            if (Raylib.CheckCollisionPointRec(mousePos, rankingButton)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                HowToPlayClicked = true;

            if (Raylib.CheckCollisionPointRec(mousePos, newCampaignButton)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                return GameMenuResult.Campaign;

            if (Raylib.CheckCollisionPointRec(mousePos, playgroundButton)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                return GameMenuResult.Playground;

            return GameMenuResult.None;
        }

        public DifficultyResult UpdateDifficulty() {
            Vector2 mp = Raylib.GetMousePosition();

            if (Raylib.CheckCollisionPointRec(mp, normalBtn)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                return DifficultyResult.Normal;

            if (Raylib.CheckCollisionPointRec(mp, hardBtn)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                return DifficultyResult.Hard;

            return DifficultyResult.None;
        }

        public bool UpdateDifficultyBack() {
            Vector2 mp = Raylib.GetMousePosition();
            return Raylib.CheckCollisionPointRec(mp, backBtn)
                   && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        private void OpenUrl(string url) {
            try {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Windows))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Linux))
                    System.Diagnostics.Process.Start("xdg-open", url);
                else
                    System.Diagnostics.Process.Start("open", url);
            } catch { }
        }

        public void Draw() {
            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.ClearBackground(Color.Black);

            string title = "They Are Coming";
            int titleWidth = Raylib.MeasureText(title, 50);
            Raylib.DrawText(title, ScreenWidth / 2 - titleWidth / 2, 100, 50, Color.Red);

            bool isOverNewCampaign = Raylib.CheckCollisionPointRec(mousePos, newCampaignButton);
            bool isOverPlayground = Raylib.CheckCollisionPointRec(mousePos, playgroundButton);
            bool isOverRanking = Raylib.CheckCollisionPointRec(mousePos, rankingButton);
            bool isOverShop = Raylib.CheckCollisionPointRec(mousePos, shopButton);
            bool isOverHowToPlay = Raylib.CheckCollisionPointRec(mousePos, howToPlayButton);

            Raylib.DrawRectangleRounded(newCampaignButton, 0.2f, 10,
                isOverNewCampaign ? new Color(200, 30, 30, 255) : new Color(220, 50, 50, 255));
            DrawCenteredText("Campaign", newCampaignButton, 20);

            Raylib.DrawRectangleRounded(playgroundButton, 0.2f, 10,
                isOverPlayground ? new Color(20, 140, 200, 255) : new Color(30, 160, 220, 255));
            DrawCenteredText("Playground", playgroundButton, 20);
            string pgSub = "∞ infinite money";
            int subW = Raylib.MeasureText(pgSub, 11);
            Raylib.DrawText(pgSub,
                (int)playgroundButton.X + (int)playgroundButton.Width / 2 - subW / 2,
                (int)playgroundButton.Y + (int)playgroundButton.Height + 4,
                11, new Color(100, 200, 255, 200));

            Color grey = new Color(80, 80, 80, 255);
            Raylib.DrawRectangleRounded(rankingButton, 0.2f, 10, isOverRanking ? Color.DarkGray : grey);
            DrawCenteredText("How To Play", rankingButton, 18);
            Raylib.DrawRectangleRounded(shopButton, 0.2f, 10, isOverShop ? Color.DarkGray : grey);
            DrawCenteredText("Github", shopButton, 18);
            Raylib.DrawRectangleRounded(howToPlayButton, 0.2f, 10, isOverHowToPlay ? Color.DarkGray : grey);
            DrawCenteredText("Discord", howToPlayButton, 18);
        }

        public void DrawDifficulty() {
            Vector2 mp = Raylib.GetMousePosition();
            Raylib.ClearBackground(Color.Black);

            string title = "SELECT DIFFICULTY";
            Raylib.DrawText(title, ScreenWidth / 2 - Raylib.MeasureText(title, 34) / 2, 110, 34, Color.Red);

            // ── NORMAL button ─────────────────────────────────────────────
            bool overNormal = Raylib.CheckCollisionPointRec(mp, normalBtn);
            Color normalBg = overNormal ? new Color(200, 30, 30, 255) : new Color(220, 50, 50, 255);
            Raylib.DrawRectangleRounded(normalBtn, 0.15f, 10, normalBg);
            Raylib.DrawRectangleLinesEx(normalBtn, 2, new Color(255, 100, 100, 255));

            string normalTitle = "NORMAL";
            Raylib.DrawText(normalTitle,
                (int)normalBtn.X + (int)normalBtn.Width / 2 - Raylib.MeasureText(normalTitle, 28) / 2,
                (int)normalBtn.Y + 18, 28, Color.White);

            string normalSub = "Survive 7 waves";
            Raylib.DrawText(normalSub,
                (int)normalBtn.X + (int)normalBtn.Width / 2 - Raylib.MeasureText(normalSub, 13) / 2,
                (int)normalBtn.Y + 56, 13, new Color(255, 220, 220, 255));

            string normalDesc = "Recommended for beginners";
            Raylib.DrawText(normalDesc,
                (int)normalBtn.X + (int)normalBtn.Width / 2 - Raylib.MeasureText(normalDesc, 12) / 2,
                (int)normalBtn.Y + 78, 12, new Color(255, 200, 200, 200));

            // ── HARD button ───────────────────────────────────────────────
            bool overHard = Raylib.CheckCollisionPointRec(mp, hardBtn);
            Color hardBg = overHard ? new Color(160, 10, 10, 255) : new Color(120, 8, 8, 255);
            Raylib.DrawRectangleRounded(hardBtn, 0.15f, 10, hardBg);
            Raylib.DrawRectangleLinesEx(hardBtn, 2, new Color(220, 30, 30, 255));

            Raylib.DrawRectangleRounded(
                new Rectangle(hardBtn.X + 4, hardBtn.Y + 4, hardBtn.Width - 8, hardBtn.Height - 8),
                0.12f, 8, new Color(80, 0, 0, 120));

            string hardTitle = "HARD";
            Raylib.DrawText(hardTitle,
                (int)hardBtn.X + (int)hardBtn.Width / 2 - Raylib.MeasureText(hardTitle, 28) / 2,
                (int)hardBtn.Y + 18, 28, new Color(255, 80, 80, 255));

            string hardSub = "Survive 15 waves";
            Raylib.DrawText(hardSub,
                (int)hardBtn.X + (int)hardBtn.Width / 2 - Raylib.MeasureText(hardSub, 13) / 2,
                (int)hardBtn.Y + 56, 13, new Color(220, 100, 100, 255));

            string hardDesc = "For experienced players only!";
            Raylib.DrawText(hardDesc,
                (int)hardBtn.X + (int)hardBtn.Width / 2 - Raylib.MeasureText(hardDesc, 12) / 2,
                (int)hardBtn.Y + 78, 12, new Color(200, 80, 80, 200));

            // ── Back button ───────────────────────────────────────────────
            bool overBack = Raylib.CheckCollisionPointRec(mp, backBtn);
            Raylib.DrawRectangleRounded(backBtn, 0.2f, 8,
                overBack ? new Color(60, 60, 60, 255) : new Color(35, 35, 35, 255));
            Raylib.DrawRectangleLinesEx(backBtn, 1, new Color(90, 90, 90, 255));
            DrawCenteredText("< Back", backBtn, 16);
        }

        private void DrawCenteredText(string text, Rectangle btn, int fontSize) {
            int textWidth = Raylib.MeasureText(text, fontSize);
            Raylib.DrawText(text,
                (int)btn.X + (int)btn.Width / 2 - textWidth / 2,
                (int)btn.Y + (int)btn.Height / 2 - fontSize / 2,
                fontSize, Color.White);
        }
    }
}