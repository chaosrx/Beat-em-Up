using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RefreshZOrider))]
public class Ammunition : MonoBehaviour
{
    [SerializeField]
    private int _ammoAmount;
    public int ammoAmount { get { return _ammoAmount; } }
}
