using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RefreshZOrider))]
public class Survivor : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public void Run()
    {
        GetComponent<Animator>().SetTrigger("Run");
    }
}
