using System;

public class GameSpeedManager
{
    private static GameSpeedManager _instance;
    public static GameSpeedManager Instance => _instance ??= new GameSpeedManager();

    private float _gameSpeed = 1.0f; // 기본 속도
    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            _gameSpeed = value;
            OnGameSpeedChanged?.Invoke(_gameSpeed);
        }
    }

    public event Action<float> OnGameSpeedChanged;

    private GameSpeedManager() { }
}
