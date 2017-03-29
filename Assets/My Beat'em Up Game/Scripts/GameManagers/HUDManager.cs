using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private static HUDManager _instance;
    public static HUDManager Instance { get { return _instance; } }

    private Hero _player;

    [SerializeField]
    private GameObject _HUDPanel;
    //Life Bar
    private Slider _lifeBar;
    //Ammo Amount Label
    private Text _ammoText;

    void Awake()
    {
        _instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        _lifeBar = _HUDPanel.transform.Find("LifeBar").GetComponent<Slider>();
        _ammoText = _HUDPanel.transform.Find("AmmoText").GetComponent<Text>();
        UpdateAmmoAmountLabel();
    }

    public void UpdateHealthBar()
    {
        _lifeBar.value = _player.health;
    }

    public void UpdateAmmoAmountLabel()
    {
        _ammoText.text = _player.ammosAmount.ToString();
    }
}
