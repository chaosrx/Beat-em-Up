using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RefreshZOrider))]
public class GasolineCan : MonoBehaviour
{
    [SerializeField]
    private int _durability;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _range;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnAttacked(object attackData)
    {
        AttackData attack = (AttackData)attackData;
        _durability -= attack.damage;
        if (_durability <= 0)
        {
            Explode();
        }
    }

    void Explode()
    {
        GetComponent<Animator>().SetTrigger("Explode");
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, _range, LayerMask.GetMask("Zombie", "Player"));
        foreach(Collider2D coll in colls)
        {
            coll.gameObject.SendMessage("OnAttacked",
                new AttackData()
                {
                    damage = _damage,
                    from = (coll.transform.position.x - transform.position.x > 0.0f) ? Direction.Left : Direction.Right
                });
        }
    }

    void Destroy()
    {
        Destroy(this.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}
