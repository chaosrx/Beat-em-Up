using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputHandler : MonoBehaviour
{
    private Hero _hero;
	
    void Start()
    {
        _hero = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
    }

	// Update is called once per frame
	void Update ()
    {
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        _hero.Move(h,v);
        if (CrossPlatformInputManager.GetButtonDown("Attack"))
        {
            _hero.Attack();
        }
        else if (CrossPlatformInputManager.GetButtonDown("Shoot"))
        {
            _hero.Shoot();
        }
    }
}
