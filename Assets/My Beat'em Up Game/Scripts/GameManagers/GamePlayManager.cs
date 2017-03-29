using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuPanel;
    private Text _menuTitle;
    private Button _button;

    private Hero _player;
    private List<Zombie> _zombies;
    private int _aliveZombiesAmount;
    private bool _isPlaying;//是否通关或者失败了
    private bool _isPausing;

    private static int _currLevelIndex;

    void Awake()
    {
        GetComponent<LevelManager>().LoadLevel(_currLevelIndex);
    }

    // Use this for initialization
    void Start()
    {
        _menuTitle = _menuPanel.transform.Find("Text").GetComponent<Text>();
        _button = _menuPanel.transform.Find("Button").GetComponent<Button>();

        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        _player.OnDie += () => { if (_isPlaying) { OnLose(); } };

        _zombies = new List<Zombie>();
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject z in zombies)
        {
            Zombie zombie = z.GetComponent<Zombie>();
            _zombies.Add(zombie);
            zombie.OnDie += () =>
            {
                if (_isPlaying)
                {
                    _aliveZombiesAmount--;
                    if (_aliveZombiesAmount == 0)
                    {
                        OnWin();
                    }
                }
            };
        }
        _aliveZombiesAmount = _zombies.Count;

        _isPlaying = true;
        _isPausing = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(_isPausing)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }
    }

    void OnWin()
    {
        _menuTitle.text = "通关";
        if (_currLevelIndex == GetComponent<LevelManager>().levelCount)
        {
            _button.gameObject.SetActive(false);
        }
        else
        {
            _button.transform.Find("Text").GetComponent<Text>().text = "下一关";
            _button.onClick.AddListener(() =>
            {
                LoadLevel(_currLevelIndex + 1);
            });
        }
        _menuPanel.SetActive(true);
    }

    void OnLose()
    {
        _menuTitle.text = "死亡";
        _button.transform.Find("Text").GetComponent<Text>().text = "再试一次";
        _button.onClick.AddListener(() =>
        {
            LoadLevel(_currLevelIndex);
        });
        _menuPanel.SetActive(true);
    }

    void OnPause()
    {
        _isPausing = true;
        Time.timeScale = 0;
        _menuTitle.text = "暂停";
        _button.transform.Find("Text").GetComponent<Text>().text = "继续游戏";
        _button.onClick.AddListener(OnResume);
        _menuPanel.SetActive(true);
    }

    void OnResume()
    {
        _isPausing = false;
        Time.timeScale = 1;
        _button.onClick.RemoveAllListeners();
        _menuPanel.SetActive(false);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public static void LoadLevel(int levelIndex)
    {
        _currLevelIndex = levelIndex;
        SceneManager.LoadScene(1);
    }
}
