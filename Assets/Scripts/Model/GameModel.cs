using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    public static string[] _character_names = new string[] { "Man", "Cube",  "Dino" };
    public static int[] _character_cost = new int[] { 0, 200,  500 };

    public static float _level_delta_speed = 0.175f;
}
