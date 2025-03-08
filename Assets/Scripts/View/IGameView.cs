using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameView
{
    public event Action _start_game;
    public event Action _back_to_menu;
    public event Action _restart;
    public event Action _change_character;
    public event Action _check_sound;
    public event Action _pause;
    public event Action _resume;

    public void StartScreenEnable();
    public void GameScreenEnable();
    public void GameOverScreenEnable(int score);

    public void UpdateCoins(int coins, bool animation);
    public void UpdateScore(int score);

}
