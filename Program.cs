using Raylib_cs;

namespace TheyAreComing {
    class Program {
        static void Main(string[] args) {
            Raylib.InitWindow(800, 600, "They Are Coming");
            Raylib.SetTargetFPS(60);

            var game = new GameManager();

            while (!Raylib.WindowShouldClose()) {
                float dt = Raylib.GetFrameTime();
                game.Update(dt);
                Raylib.BeginDrawing();
                game.Draw();
                Raylib.EndDrawing();
            }

            game.Unload();
            Raylib.CloseWindow();
        }
    }
}
