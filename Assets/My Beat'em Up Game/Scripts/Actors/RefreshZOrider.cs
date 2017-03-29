using UnityEngine;
using System.Collections;

public class RefreshZOrider : MonoBehaviour
{
    private SpriteRenderer _renderer;

    [SerializeField]
    private bool _isUpdateRuntime;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        RefreshZ();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isUpdateRuntime)
        {
            RefreshZ();
        }
    }

    void RefreshZ()
    {
        _renderer.sortingOrder = -(int)transform.position.y;
    }
}