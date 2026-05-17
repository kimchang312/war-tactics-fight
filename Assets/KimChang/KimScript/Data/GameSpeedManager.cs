using System;
using UnityEngine;

public class GameSpeedManager
{
    private static GameSpeedManager _instance;
    public static GameSpeedManager Instance => _instance ??= new GameSpeedManager();

    private float _gameSpeed = 1f;

    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            float newValue = Mathf.Max(0.01f, value);

            if (Mathf.Approximately(_gameSpeed, newValue))
                return;

            _gameSpeed = newValue;
            OnGameSpeedChanged?.Invoke(_gameSpeed);
        }
    }

    public event Action<float> OnGameSpeedChanged;

    private GameSpeedManager() { }
}