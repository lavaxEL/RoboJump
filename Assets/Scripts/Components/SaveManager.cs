using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;


public static class GlobalData
{
    public static bool _first_in_game = true;
    public static bool _audio_object_is_loaded = false;

    #region Сохраняемые данные

    public static bool _sound = true;
    public static bool _music = true;

    public static int _coins = 0;
    public static int _best_score = 0;

    public static bool[] _characters = new bool[] { true,  false, false };

    public static int _num_of_games_without_ads = 0;

    public static int _current_character_ind = 0;

    #endregion

    public static void SaveData()
    {
        SaveData data = new SaveData()
        {
            _characters = _characters,
            _music = _music,
            _coins = _coins,
            _current_character_ind  = _current_character_ind,
            _sound = _sound,
            _best_score= _best_score,
            _num_of_games_without_ads = _num_of_games_without_ads,
        };
        SaveManager.Save("data", data);
    }
    public static void LoadData()
    {
        SaveData data = SaveManager.Load<SaveData>("data");

        _sound = data._sound;
        _coins = data._coins;
        _characters = data._characters;
        _music = data._music;
        _current_character_ind= data._current_character_ind;
        _best_score= data._best_score;
        _num_of_games_without_ads = data._num_of_games_without_ads;
    }
}

public static class SaveManager
{
    public static void Save<T>(string key, T saveData)
    {
        string jsonString = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(key, jsonString);
    }
    public static T Load<T>(string key) where T : new()
    {
        if (PlayerPrefs.HasKey(key))
        {
            string jsonString = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(jsonString);
        }
        else
            return new T();
    }
}

[System.Serializable]
public class SaveData
{
    public bool _sound = true;
    public bool _music = true;

    public int _coins = 0;
    public int _best_score = 0;

    public bool[] _characters = new bool[] { true, false, false };

    public  int _current_character_ind = 0;

    public int _num_of_games_without_ads = 0;
}

