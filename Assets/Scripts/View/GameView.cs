using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour, IGameView
{
    public event Action _start_game;
    public event Action _back_to_menu;
    public event Action _restart;
    public event Action _change_character;
    public event Action _check_sound;
    public event Action _pause;
    public event Action _resume;

    #region Переменные внешнего пользования

    public GameObject LogoBlock;
    public GameObject MenuBlock;
    public GameObject DarkBlock;
    public GameObject ShopBlock;
    public GameObject MusicSoundBlock;
    public GameObject GameBlock;
    public GameObject GameOverBlock;
    public GameObject CoinBlock;
    public GameObject PauseBlock;

    public TapButtonHandler PlayButton;
    public TapButtonHandler ShopButton;
    public TapButtonHandler SoundButton;
    public TapButtonHandler MusicButton;
    public TapButtonHandler BackToMenuShopButton;
    public TapButtonHandler PauseButton;
    public TapButtonHandler ResumeButton;
    public TapButtonHandler BackToMenuGameOverButton;
    public TapButtonHandler BackToMenuPauseButton;
    public TapButtonHandler RestartGameOverButton;

    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;
    public Sprite MusicOnSprite;
    public Sprite MusicOffSprite;

    public Text NumOfCoinsText;
    public Text NumOfCoinsTextBack;
    public GameObject CoinIcon;
    public Text ScoreText;
    public Text ScoreTextBack;

    public Text ScoreGameOverText;
    public Text ScoreGameOverTextBack;
    public Text BestScoreGameOverText;
    public Text BestScoreGameOverTextBack;

    public Image SoundImage;
    public Image MusicImage;

    public GameObject ManBlock;
    public GameObject CubeBlock;
    public GameObject DinoBlock;

    public AudioSource BuySound;
    public AudioSource TapSound;

    public static AudioSource BuySoundStatic;
    public static AudioSource TapSoundStatic;

    #endregion

    #region Переменные внутреннего пользования

    private List<ShopBlockElem> _shop_blocks;

    #endregion

    public class ShopBlockElem
    {
        public string _name;
        public int _ind;

        public event Action<ShopBlockElem> _buy;
        public event Action<ShopBlockElem> _select;

        public GameObject SelectedImage;
        public GameObject DarkImage;
        public GameObject GalkaImage;

        public TapButtonHandler BuyButton;
        public TapButtonHandler SelectButton;

        public Text CostText;
        public Text CostTextBack;

        public void AddHandlers()
        {
            BuyButton._tap += BuyButHandler;
            BuyButton._tap_sound += BuyButSoundHandler;

            SelectButton._tap += SelectButHandler;
            SelectButton._tap_sound += SelectButSoundHandler;
        }

        private void SelectButHandler() => _select(this);

        private void BuyButHandler() => _buy(this);


        private void SelectButSoundHandler()
        {  }
        private void BuyButSoundHandler()
        {  }
    }

    private void Awake()
    {
        if (!GlobalData._audio_object_is_loaded)
        {
            BuySoundStatic = BuySound;
            TapSoundStatic = TapSound;
        }
       
    }

    private void Start()
    {
        

        _shop_blocks = new List<ShopBlockElem>();
        AddCharacter("Man", 0, ManBlock);
        AddCharacter("Cube", 1, CubeBlock);
        AddCharacter("Dino", 2, DinoBlock);


        #region Подписки на события

        PlayButton._tap += PlayButtonHandler;
        PlayButton._tap_sound += ButtonTapSoundHandler;

        ShopButton._tap += ShopButtonHandler;
        ShopButton._tap_sound += ButtonTapSoundHandler;

        SoundButton._tap += SoundButtonHandler;
        SoundButton._tap_sound += ButtonTapSoundHandler;

        MusicButton._tap += MusicButtonHandler;
        MusicButton._tap_sound += ButtonTapSoundHandler;

        BackToMenuShopButton._tap += BackToMenuHandler;
        BackToMenuShopButton._tap_sound += ButtonTapSoundHandler;

        PauseButton._tap += PauseButtonHandler;
        PauseButton._tap_sound += ButtonTapSoundHandler;

        ResumeButton._tap += ResumeButtonHandler;
        ResumeButton._tap_sound += ButtonTapSoundHandler;

        BackToMenuPauseButton._tap += BackToMenuHandler;
        BackToMenuPauseButton._tap_sound += ButtonTapSoundHandler;

        BackToMenuGameOverButton._tap += BackToMenuHandler;
        BackToMenuGameOverButton._tap_sound += ButtonTapSoundHandler;

        RestartGameOverButton._tap += RestartGameOverButtonHandler;
        RestartGameOverButton._tap_sound += ButtonTapSoundHandler;

        #endregion
    }

   

    private void AddCharacter(string name, int ind, GameObject block)
    {
        ShopBlockElem elem = new ShopBlockElem()
        {
            _name = name,
            _ind = ind,
            SelectedImage = block.transform.Find("ImageSelected").gameObject,
            DarkImage = block.transform.Find("DarkImage").gameObject,
            GalkaImage = block.transform.Find("Selected").gameObject,
            BuyButton = block.transform.Find("BuyBut").gameObject.GetComponent<TapButtonHandler>(),
            SelectButton = block.transform.Find("Select").gameObject.GetComponent<TapButtonHandler>(),
            CostText = block.transform.Find("BuyBut").Find("Cost").gameObject.GetComponent<Text>(),
            CostTextBack = block.transform.Find("BuyBut").Find("CostBack").gameObject.GetComponent<Text>(),
        };

        elem.AddHandlers();

        elem._buy += CharacterBuyHandler;
        elem._select += CharacterSelectHandler;

        _shop_blocks.Add(elem);
    }

   
    private void UpdateShop()
    {
        for (int i=0; i<_shop_blocks.Count; i++)
        {
            int ind = _shop_blocks[i]._ind;
            if (!GlobalData._characters[ind]) // если не изучен
            {
                _shop_blocks[i].SelectButton.gameObject.SetActive(false);
                _shop_blocks[i].CostText.text = GameModel._character_cost[ind].ToString();
                _shop_blocks[i].CostTextBack.text = GameModel._character_cost[ind].ToString();
                _shop_blocks[i].BuyButton.gameObject.SetActive(true);
                _shop_blocks[i].DarkImage.gameObject.SetActive(false);
                _shop_blocks[i].SelectedImage.gameObject.SetActive(false);
                _shop_blocks[i].GalkaImage.gameObject.SetActive(false);
                _shop_blocks[i].BuyButton.Interactable = true;

                if (GlobalData._coins < GameModel._character_cost[ind]) // если не достаточно чтобы изучить
                {
                    _shop_blocks[i].DarkImage.gameObject.SetActive(true);
                    _shop_blocks[i].BuyButton.Interactable = false;
                }
            }
            else // если изучен
            {
                if (ind == GlobalData._current_character_ind) // если выбран
                {
                    _shop_blocks[i].SelectButton.gameObject.SetActive(false);
                    _shop_blocks[i].BuyButton.gameObject.SetActive(false);
                    _shop_blocks[i].DarkImage.gameObject.SetActive(false);
                    _shop_blocks[i].SelectedImage.gameObject.SetActive(true);
                    _shop_blocks[i].GalkaImage.gameObject.SetActive(true);
                }
                else // если не выбран
                {
                    _shop_blocks[i].SelectButton.gameObject.SetActive(true);
                    _shop_blocks[i].BuyButton.gameObject.SetActive(false);
                    _shop_blocks[i].DarkImage.gameObject.SetActive(false);
                    _shop_blocks[i].SelectedImage.gameObject.SetActive(false);
                    _shop_blocks[i].GalkaImage.gameObject.SetActive(false);
                }
            }
        }
    }


    #region Обработчики событий

    private void CharacterBuyHandler(ShopBlockElem elem)
    {
        GlobalData._coins -= GameModel._character_cost[elem._ind];
        UpdateCoins(GlobalData._coins, false);

        GlobalData._characters[elem._ind] = true;

        GlobalData.SaveData();

        UpdateShop();

        if (GlobalData._sound)
            BuySoundStatic.Play();
    }

    private void CharacterSelectHandler(ShopBlockElem elem)
    {
        GlobalData._current_character_ind = elem._ind;

        GlobalData.SaveData();

        _change_character();

        UpdateShop();

        ButtonTapSoundHandler();
    }

    private void PlayButtonHandler() => _start_game();

    private void ShopButtonHandler()
    {
        DisableAllBlock();

        UpdateShop();

        LogoBlock.SetActive(true);
        ShopBlock.SetActive(true);
        DarkBlock.SetActive(true);
        CoinBlock.SetActive(true);
    }

    private void ResumeButtonHandler()
    {
        GameScreenEnable();

        _resume();
    }

    private void PauseButtonHandler()
    {
        DisableAllBlock();

        PauseBlock.SetActive(true);
        DarkBlock.SetActive(true);
        CoinBlock.SetActive(true);
        LogoBlock.SetActive(true);

        _pause();
    }

    private void SoundButtonHandler()
    {
        if (GlobalData._sound)
            SoundImage.sprite = SoundOffSprite;
        else
            SoundImage.sprite = SoundOnSprite;

        GlobalData._sound = !GlobalData._sound;

        GlobalData.SaveData();

        _check_sound();
    }

    private void MusicButtonHandler()
    {
        if (GlobalData._music)
            MusicImage.sprite = MusicOffSprite;
        else
            MusicImage.sprite = MusicOnSprite;

        GlobalData._music = !GlobalData._music;

        GlobalData.SaveData();

        _check_sound();
    }

    private void BackToMenuHandler() => _back_to_menu();
    private void RestartGameOverButtonHandler() =>_restart();

    private void ButtonTapSoundHandler()
    {
        if (GlobalData._sound)
            TapSoundStatic.Play();
    }
    #endregion

    private void DisableAllBlock()
    {
        LogoBlock.SetActive(false);
        MenuBlock.SetActive(false);
        DarkBlock.SetActive(false);
        ShopBlock.SetActive(false); 
        MusicSoundBlock.SetActive(false);
        GameBlock.SetActive(false);
        GameOverBlock.SetActive(false);
        CoinBlock.SetActive(false);
        PauseBlock.SetActive(false);
    }

    public void StartScreenEnable()
    {
        DisableAllBlock();

        LogoBlock.SetActive(true);
        MenuBlock.SetActive(true);
        DarkBlock.SetActive(true);
        MusicSoundBlock.SetActive(true);
        CoinBlock.SetActive(true);

        if (GlobalData._music)
            MusicImage.sprite = MusicOnSprite;
        else
            MusicImage.sprite = MusicOffSprite;

        if (GlobalData._sound)
            SoundImage.sprite = SoundOnSprite;
        else
            SoundImage.sprite = SoundOffSprite;

    }

    public void GameScreenEnable()
    {
        DisableAllBlock();

        GameBlock.SetActive(true);
        CoinBlock.SetActive(true);
    }

    public void GameOverScreenEnable(int score)
    {
        ScoreGameOverText.text = "Score: " + score.ToString();
        ScoreGameOverTextBack.text = "Score: " + score.ToString();
        BestScoreGameOverText.text = "Best score: " + GlobalData._best_score.ToString();
        BestScoreGameOverTextBack.text="Best score: " +  GlobalData._best_score.ToString();

        DisableAllBlock();

        LogoBlock.SetActive(true);
        DarkBlock.SetActive(true);
        GameOverBlock.SetActive(true);
        CoinBlock.SetActive(true);
    }

    public void UpdateCoins(int coins, bool animation)
    {
        NumOfCoinsText.text = coins.ToString();
        NumOfCoinsTextBack.text=coins.ToString();

        if (animation)
        {
            CoinIcon.SetActive(false);
            CoinIcon.SetActive(true);
        }
    }

    public void UpdateScore(int score)
    {
        ScoreText.text = score.ToString();
        ScoreTextBack.text=score.ToString();
    }

    
}
