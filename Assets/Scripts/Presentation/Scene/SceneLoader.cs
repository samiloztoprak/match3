
public static class SceneLoader
{
    public const string MenuScene = "Menu";
    public const string GameScene = "Game";

    public static void LoadMenu() => Load(MenuScene);
    public static void LoadGame() => Load(GameScene);

    private static void Load(string sceneName)
    {
        LoadingScreen.LoadScene(sceneName);
    }
}