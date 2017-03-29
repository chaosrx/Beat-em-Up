using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour
{
    private Transform _target;

    [SerializeField]
    private float _z;
    [SerializeField]
    private float _followSpeed = 20.0f;
    [SerializeField]
    private float _leftBorder;
    [SerializeField]
    private float _rightBorder;
    [SerializeField]
    private float _topBorder;
    [SerializeField]
    private float _bottomBorder;

    // Use this for initialization
    void Start ()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Follow();
    }

    void Follow()
    {
        if(_target == null)
        {
            return;
        }
        Vector3 position = new Vector3(Mathf.Clamp(_target.position.x, _leftBorder, _rightBorder), Mathf.Clamp(_target.position.y, _bottomBorder, _topBorder), _z);
        transform.position = Vector3.Lerp(transform.position, position, _followSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
