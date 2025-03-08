using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{

    #region Переменные внешнего пользования

    public Player _player;

    public Animator BackgroundAnimator;
    public Animator ForgegroundAnimator;
    public Animator GroundAnimator;

    public GameObject[] ObstaclePrefabs;
    public GameObject CoinPrefab;

    public GameObject Audio;

    public AudioSource Music;
    public AudioSource GetCoinSound;
    public AudioSource GameOverSound;

    public static AudioSource MusicStatic;
    public static AudioSource GetCoinSoundStatic;
    public static AudioSource GameOverSoundStatic;

    public static InterstitialAd InterstitialAdStatic;
    public  InterstitialAd InterstitialAd;

    #endregion

    #region Переменные внутреннего пользования

    private IGameView _game_view;
    private float _time_tap = 0;
    private bool _playing = false;
    private bool _was_jump = false;
    private bool _was_double_jump = false;
    private int _score = 0;
    private int _level_ind = 0;

    private Queue<Obstacle> _obstacles;
    private Queue<Coin> _coins;

    private float _left_time_limit = 1.3f;
    private float _right_time_limit = 1.8f;
    private float _time_coin = 0.4f;


    private float _world_speed_temp = 0;
    private float _player_speed_temp = 0;
    
    private readonly int[] _levels = new int[] {8, 16, 32,  64, 128, 256};

    private readonly float _spawn_pos_x = 40;
    private readonly float _spawn_pos_y = -5;
    private readonly float _spawn_pos_coin_y = -4.5f;

    private readonly float _world_start_speed_in_sec = 7.7143f;

    #endregion




    void Start()
    {
        _game_view = GameObject.Find("App").transform.Find("View").gameObject.GetComponent<IGameView>();

        Application.targetFrameRate = 120;

        GlobalData.LoadData();

        _player.EnableCurrentSkin(GameModel._character_names[GlobalData._current_character_ind]);
        _player.SetStartPosition();

        if (GlobalData._first_in_game)
        {
            _game_view.StartScreenEnable();
            GlobalData._first_in_game=false;
        }
        else
        {
            
            StartGame();
        }

        if (!GlobalData._audio_object_is_loaded)
        {
            InterstitialAdStatic = InterstitialAd;
            InterstitialAdStatic.LoadAd();

            GlobalData._audio_object_is_loaded = true;
            MusicStatic = Music;
            GetCoinSoundStatic = GetCoinSound;
            GameOverSoundStatic = GameOverSound;

            DontDestroyOnLoad(Audio);
            DontDestroyOnLoad(InterstitialAd);
        }
        else
        {
            Destroy(Audio);
            Destroy(InterstitialAd);
        }
            

        _game_view.UpdateCoins(GlobalData._coins, false);

        #region Подписки на события

        _game_view._start_game += StartGameHandler;
        _game_view._back_to_menu += BackToMenuHandler;
        _game_view._restart += StartGameHandler;
        _game_view._change_character += ChangeCharacterHandler;
        _game_view._check_sound += CheckSoundHandler;
        _game_view._pause += PauseHandler;
        _game_view._resume += ResumeHandler;

        _player._on_ground += PlayerOnGroundHandler;


        #endregion

        CheckSoundHandler();

    }



    private void StartGame()
    {
        _player._enable = true;
        _game_view.UpdateCoins(GlobalData._coins, false);
        _game_view.GameScreenEnable();

        BackgroundAnimator.speed = 1.7f;
        ForgegroundAnimator.speed = 1.7f;
        GroundAnimator.speed = 1.7f;
        _player._animator.speed = 1.3f;

        BackgroundAnimator.SetInteger("cntrl", 1);
        ForgegroundAnimator.SetInteger("cntrl", 1);
        GroundAnimator.SetInteger("cntrl", 1);


        _player.Run();
        StartCoroutine("GenerateObstacles");

        _playing = true;
    }

    private IEnumerator GenerateObstacles()
    {
        int lastIndObst = 0;
        int ind = UnityEngine.Random.Range(0, ObstaclePrefabs.Length);
        int coinP = 0;

        _coins = new Queue<Coin>();
        _obstacles= new Queue<Obstacle>();

        bool _was_pause = false;

        while (true)
        {
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(_left_time_limit, _right_time_limit));

            if (!_playing)
            {
                _was_pause= true;
                continue;
            }
            else if (_was_pause)
            {
                _was_pause = false;
                yield return new WaitForSeconds(_left_time_limit);
            }
                

            if (ind == lastIndObst)
            {
                ind++;
                if (ind == ObstaclePrefabs.Length)
                    ind = 0;
            }
            lastIndObst = ind;

            Obstacle ob = Instantiate(ObstaclePrefabs[ind], new Vector2(_spawn_pos_x, _spawn_pos_y), Quaternion.identity).gameObject.GetComponent<Obstacle>();
            ob._was_collision += WasObstacleCollisionHandler;
            ob._add_score += AddScoreHandler;
            ob.SetSpeed(_world_start_speed_in_sec * GroundAnimator.speed);
            _obstacles.Enqueue(ob);

            int coinRand = UnityEngine.Random.Range(0, 16);

            if (coinRand < coinP)
            {
                int r2 = UnityEngine.Random.Range(0, 5);
                if (r2 == 2) // спавним одну монету над препятствием
                {
                    if (ob._type == ObstacleType.Box)
                        SpawnCoin(new Vector2(_spawn_pos_x, _spawn_pos_y + UnityEngine.Random.Range(3f, 4.1f)));
                    else if (ob._type == ObstacleType.Lazer || ob._type == ObstacleType.EvilRobotDown)
                        SpawnCoin(new Vector2(_spawn_pos_x, _spawn_pos_y + UnityEngine.Random.Range(3.5f, 4.6f)));
                    else if (ob._type == ObstacleType.EvilRobotUpx2)
                        SpawnCoin(new Vector2(_spawn_pos_x, _spawn_pos_coin_y));

                    coinP = 0;
                }
                else
                {
                    int coinNum = 0;

                    int r1 = UnityEngine.Random.Range(0, 10); // решаем сколько монет будет заспавнено на земле
                    if (r1 < 2)
                        coinNum = 2;
                    else
                        coinNum = 1;

                    for (int c = 0; c < coinNum; c++)
                    {
                        yield return new WaitForSeconds(_time_coin);
                        if(!_playing)
                            continue;
                        SpawnCoin(new Vector2(_spawn_pos_x, _spawn_pos_coin_y));
                    }
                }

            }
            else coinP++;

        }

    }

   

    #region Обработчики событий

    private void StartGameHandler()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    private void CheckSoundHandler()
    {
        if (GlobalData._music)
            MusicStatic.volume = 0.6f;
        else
            MusicStatic.volume = 0;
    }

    private void BackToMenuHandler()
    {
        GlobalData._first_in_game = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ChangeCharacterHandler()
    {
        _player.EnableCurrentSkin(GameModel._character_names[GlobalData._current_character_ind]);
    }

    private void AddScoreHandler()
    {
        _score++;
        _game_view.UpdateScore(_score);

        if (_score > GlobalData._best_score)
        {
            GlobalData._best_score = _score;
            GlobalData.SaveData();
        }

        _obstacles.Dequeue();

        if (_level_ind < _levels.Length && _score >= _levels[_level_ind]) // усложнение игры
        {
            BackgroundAnimator.speed += GameModel._level_delta_speed;
            ForgegroundAnimator.speed += GameModel._level_delta_speed;
            GroundAnimator.speed += GameModel._level_delta_speed;
            _player._animator.speed += GameModel._level_delta_speed;



            foreach(Obstacle ob in _obstacles)
            {
                if (ob != null)
                    ob.SetSpeed(_world_start_speed_in_sec * GroundAnimator.speed);
            }


            foreach (Coin c in _coins)
            {
                if (c != null)
                    c.SetSpeed(_world_start_speed_in_sec * GroundAnimator.speed);
            }

            _left_time_limit -= 0.1f;
            _right_time_limit -= 0.1f;
            _time_coin -= 0.025f;

            _level_ind++;
        }
    }

    private void PlayerOnGroundHandler()
    {
        _was_jump = false;
        _was_double_jump=false;
    }

    private void ResumeHandler()
    {
        _playing = true;
        BackgroundAnimator.speed = _world_speed_temp;
        ForgegroundAnimator.speed = _world_speed_temp;
        GroundAnimator.speed = _world_speed_temp;

        _player._animator.speed = _player_speed_temp;
        _player.UnPause();

        foreach (Obstacle ob in _obstacles)
        {
            if (ob != null)
                ob._move=true;
        }


        foreach (Coin c in _coins)
        {
            if (c != null)
                c._move = true;
        }
    }

    private void PauseHandler()
    {
        _playing = false;

        _world_speed_temp = BackgroundAnimator.speed;
        _player_speed_temp = _player._animator.speed;

        BackgroundAnimator.speed = 0;
        ForgegroundAnimator.speed = 0;
        GroundAnimator.speed = 0;

        _player._animator.speed = 0;
        _player.Pause();

        foreach (Obstacle ob in _obstacles)
        {
            if (ob != null)
                ob._move = false;
        }


        foreach (Coin c in _coins)
        {
            if (c != null)
                c._move = false;
        }
    }


    private void WasObstacleCollisionHandler(ObstacleType type)
    {
        if (_playing)
        {
            _playing = false;

            BackgroundAnimator.speed = 0;
            ForgegroundAnimator.speed = 0;
            GroundAnimator.speed = 0;

            foreach (Obstacle ob in _obstacles)
            {
                if (ob != null)
                    ob._move = false;
            }


            foreach (Coin c in _coins)
            {
                if (c != null)
                    c._move = false;
            }

            _player.Dead();

            if (GlobalData._sound)
                GameOverSoundStatic.Play();

            _game_view.GameOverScreenEnable(_score);

            if (GlobalData._num_of_games_without_ads >= 6)
            {
                InterstitialAdStatic.ShowAd();
                GlobalData._num_of_games_without_ads = 0;
                GlobalData.SaveData();
            }
            else
            {
                GlobalData._num_of_games_without_ads++;
                GlobalData.SaveData();
                if (GlobalData._num_of_games_without_ads>=5)
                    InterstitialAdStatic.LoadAd();
            }
        }
        
    }

    private void WasCoinCollisionHandler()
    {
        GlobalData._coins++;
        AndroidAPI.Vibration(10);
        if (GlobalData._sound)
            GetCoinSoundStatic.Play();
        _game_view.UpdateCoins(GlobalData._coins, true);
        GlobalData.SaveData();
    }

    #endregion

    private void SpawnCoin(Vector2 pos)
    {
        Coin coin = Instantiate(CoinPrefab, pos, Quaternion.identity).gameObject.GetComponent<Coin>();
        coin._was_collision += WasCoinCollisionHandler;
        coin.SetSpeed(_world_start_speed_in_sec * GroundAnimator.speed);
        _coins.Enqueue(coin);
    }

   

    private void Update()
    {
        if (_playing) 
        {
            if (Input.GetMouseButtonDown(0) && !_was_jump && Input.mousePosition.y<Screen.height/1.3f)
            {
                _was_jump = true;
                _time_tap = Time.realtimeSinceStartup;
                _player.Jump();
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.y < Screen.height / 1.3f)
            {
                if (Time.realtimeSinceStartup - _time_tap>=0.2f && _was_jump && !_was_double_jump)
                {
                    _was_double_jump = true;
                    _player.DoubleJump();
                }
                if (!_was_jump && !_was_double_jump)
                {
                    _was_jump = true;
                    _time_tap = Time.realtimeSinceStartup;
                    _player.Jump();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                //_was_double_jump = true;
            }
            else if(Input.GetMouseButtonDown(0) && _was_jump && !_was_double_jump && Input.mousePosition.y < Screen.height / 1.3f)
            {
                _was_double_jump = true;
                _player.DoubleJump();
            }
        }
      
    }




}
