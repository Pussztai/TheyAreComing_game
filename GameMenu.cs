using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class GameMenu {
        private const int ScreenWidth  = 800;
        private const int ScreenHeight = 600;

        private Rectangle newCampaignButton;
        private Rectangle playgroundButton;
        private Rectangle rankingButton;
        private Rectangle shopButton;
        private Rectangle howToPlayButton;

        public GameMenu() {
            newCampaignButton = new Rectangle(ScreenWidth / 2 - 210, ScreenHeight / 2 - 50, 200, 50);
            playgroundButton  = new Rectangle(ScreenWidth / 2 + 10,  ScreenHeight / 2 - 50, 200, 50);
            rankingButton     = new Rectangle(ScreenWidth / 2 - 200, ScreenHeight / 2 + 70, 120, 40);
            shopButton        = new Rectangle(ScreenWidth / 2 - 60,  ScreenHeight / 2 + 70, 120, 40);
            howToPlayButton   = new Rectangle(ScreenWidth / 2 + 80,  ScreenHeight / 2 + 70, 120, 40);
        }

        public bool Update() {
            Vector2 mousePos = Raylib.GetMousePosition();
            return Raylib.CheckCollisionPointRec(mousePos, newCampaignButton)
                   && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        public void Draw() {
            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.ClearBackground(Color.Black);

            string title = "They Are Coming";
            int titleWidth = Raylib.MeasureText(title, 50);
            Raylib.DrawText(title, ScreenWidth / 2 - titleWidth / 2, 100, 50, Color.Red);

            bool isOverNewCampaign = Raylib.CheckCollisionPointRec(mousePos, newCampaignButton);
            bool isOverPlayground  = Raylib.CheckCollisionPointRec(mousePos, playgroundButton);
            bool isOverRanking     = Raylib.CheckCollisionPointRec(mousePos, rankingButton);
            bool isOverShop        = Raylib.CheckCollisionPointRec(mousePos, shopButton);
            bool isOverHowToPlay   = Raylib.CheckCollisionPointRec(mousePos, howToPlayButton);

            Raylib.DrawRectangleRounded(newCampaignButton, 0.2f, 10,
                isOverNewCampaign ? new Color(200, 30, 30, 255) : new Color(220, 50, 50, 255));
            DrawCenteredText("Campaign", newCampaignButton, 20);

            Raylib.DrawRectangleRounded(playgroundButton, 0.2f, 10,
                isOverPlayground ? new Color(200, 120, 20, 255) : new Color(220, 140, 30, 255));
            DrawCenteredText("Playground", playgroundButton, 20);

            Color grey = new Color(80, 80, 80, 255);
            Raylib.DrawRectangleRounded(rankingButton,   0.2f, 10, isOverRanking   ? Color.DarkGray : grey);
            DrawCenteredText("How To Play", rankingButton, 18);
            Raylib.DrawRectangleRounded(shopButton,      0.2f, 10, isOverShop      ? Color.DarkGray : grey);
            DrawCenteredText("Github", shopButton, 18);
            Raylib.DrawRectangleRounded(howToPlayButton, 0.2f, 10, isOverHowToPlay ? Color.DarkGray : grey);
            DrawCenteredText("Discord", howToPlayButton, 18);
        }

        private void DrawCenteredText(string text, Rectangle btn, int fontSize) {
            int textWidth = Raylib.MeasureText(text, fontSize);
            Raylib.DrawText(text,
                (int)btn.X + (int)btn.Width  / 2 - textWidth / 2,
                (int)btn.Y + (int)btn.Height / 2 - fontSize / 2,
                fontSize, Color.White);
        }
    }
}
