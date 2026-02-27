using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {
    public class MainMenu {
        private const int ScreenWidth  = 800;
        private const int ScreenHeight = 600;
        private Rectangle startButton;

        public MainMenu() {
            startButton = new Rectangle(ScreenWidth / 2 - 100, ScreenHeight / 2 + 50, 200, 60);
        }

        public bool Update() {
            Vector2 mousePos = Raylib.GetMousePosition();
            return Raylib.CheckCollisionPointRec(mousePos, startButton)
                   && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        public void Draw() {
            Raylib.ClearBackground(Color.Black);

            string title = "THEY ARE COMING";
            int titleWidth = Raylib.MeasureText(title, 60);
            Raylib.DrawText(title, ScreenWidth / 2 - titleWidth / 2, 150, 60, Color.Red);

            Vector2 mousePos = Raylib.GetMousePosition();
            bool isMouseOverStart = Raylib.CheckCollisionPointRec(mousePos, startButton);
            Color buttonColor = isMouseOverStart ? Color.DarkGray : Color.Gray;

            Raylib.DrawRectangleRec(startButton, buttonColor);
            Raylib.DrawRectangleLinesEx(startButton, 3, Color.White);

            string buttonText = "START";
            int textWidth = Raylib.MeasureText(buttonText, 30);
            Raylib.DrawText(buttonText,
                (int)startButton.X + (int)startButton.Width  / 2 - textWidth / 2,
                (int)startButton.Y + 15,
                30, Color.White);
        }
    }
}
