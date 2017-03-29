using UnityEngine;
using System;
using System.Collections;

public enum Direction
{
    Right,
    Left
}

public struct AttackData
{
    public int damage;
    //public bool isSuperAttack;
    public Direction from;
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RefreshZOrider))]
public class Hero : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Animator _animator;
    private Rigidbody2D _rigibody2d;

    [SerializeField]
    private int _maxHealth;
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _hurtTime;//被攻击后的硬直时间
    [Header("普通攻击参数")]
    [SerializeField]
    private int _attackDamage;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private AudioClip _attackEffect;
    [Header("射击参数")]
    [SerializeField]
    private int _ammosAmount;//携带的子弹数量
    public int ammosAmount { get { return _ammosAmount; } }
    [SerializeField]
    private int _shootDamage;
    [SerializeField]
    private AudioClip _shootEffect;

    private int _health;
    public int health { get { return _health; } } 
    public bool IsDead { get { return _health <= 0; } }
    private Direction _facing;
    private float _remainHurtTime;

    public event Action OnDie;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _rigibody2d = GetComponent<Rigidbody2D>();
    }

	// Use this for initialization
	void Start ()
    {
        _health = _maxHealth;
        _facing = Direction.Right;
        _remainHurtTime = 0.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
    }

    void FaceTo(Direction direction)
    {
        switch(direction)
        {
            case Direction.Left:
                _renderer.flipX = true;
                break;
            case Direction.Right:
                _renderer.flipX = false;
                break;
        }
        _facing = direction;
    }

    void StopMoving()
    {
        _rigibody2d.velocity = Vector2.zero;
    }

    public void Move(float h, float v)
    {
        //移动
        if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            Vector2 speed = new Vector2(h, v);
            if (speed.magnitude > 1.0f)
            {
                speed = speed.normalized;
            }
            speed = speed * _moveSpeed;
            _rigibody2d.velocity = speed;
            if (h != 0.0f || v != 0.0f)
            {
                _animator.SetBool("IsMoving", true);
            }
            else
            {
                _animator.SetBool("IsMoving", false);
            }
            //转身
            if (h > 0.0f && _facing == Direction.Left)
            {
                FaceTo(Direction.Right);
            }
            else if (h < 0.0f && _facing == Direction.Right)
            {
                FaceTo(Direction.Left);
            }
        }
    }

    /*
     * 攻击和射击的判定：
     * 人物伤害判定区域为人物底端的一个触发器
     * 攻击：射线检测
     */
    public void Attack()
    {
        //如果被打则无法攻击
        if (_remainHurtTime > 0.0f)
        {
            return;
        }
        StopMoving();//攻击时停止移动
        //随机播放攻击动画
        int attackId = UnityEngine.Random.Range(1, 4);
        _animator.SetInteger("AttackId", attackId);
        _animator.SetTrigger("Attack");
        AudioManager.Instance.Play(_attackEffect);//播放攻击音效
        //攻击判定
        RaycastHit2D[] hitInfos = Physics2D.RaycastAll(transform.position, _facing == Direction.Right ? Vector2.right : Vector2.left, _attackRange, LayerMask.GetMask("Zombie", "Barrel"));
        foreach (RaycastHit2D hitInfo in hitInfos)
        {
            hitInfo.collider.gameObject.SendMessage("OnAttacked",
                new AttackData()
                {
                    damage = _attackDamage,
                    from = (hitInfo.collider.transform.position.x - transform.position.x > 0.0f) ? Direction.Left : Direction.Right
                });
        }
    }

    public void Shoot()
    {
        //被打则无法射击
        if (_remainHurtTime > 0.0f)
        {
            return;
        }
        //没有子弹则无法射击
        if (_ammosAmount == 0)
        {
            return;
        }
        StopMoving();//射击时停止移动
        _animator.SetTrigger("Shoot");//播放射击动画
        AudioManager.Instance.Play(_shootEffect);//播放射击音效
        _ammosAmount--;//更新子弹数
        HUDManager.Instance.UpdateAmmoAmountLabel();//更新UI
        //射击判定
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, (_facing == Direction.Right ? Vector2.right : Vector2.left), float.MaxValue, LayerMask.GetMask("Zombie","Barrel"));
        if(hitInfo)
        {
            hitInfo.collider.gameObject.SendMessage("OnAttacked",
                new AttackData()
                {
                    damage = _shootDamage,
                    from = (hitInfo.collider.transform.position.x - transform.position.x > 0.0f) ? Direction.Left : Direction.Right
                });
        }
    }

    void OnAttacked(object attackData)
    {
        AttackData attack = (AttackData)attackData;
        _health = Mathf.Max(0, _health - attack.damage);
        HUDManager.Instance.UpdateHealthBar();
        StopMoving();
        FaceTo(attack.from);
        if (_health == 0)
        {
            Die();
        }
        else
        {
            _animator.SetTrigger("Hurt");
            if (_remainHurtTime > 0.0f)
            {
                _remainHurtTime = _hurtTime;
            }
            else
            {
                StartCoroutine(Hurt());
            }
        }
    }

    IEnumerator Hurt()
    {
        _remainHurtTime = _hurtTime;
        while (_remainHurtTime > 0.0f)
        {
            _remainHurtTime -= Time.deltaTime;
            yield return null;
        }
    }

    void Die()
    {
        StopMoving();
        _animator.SetTrigger("Die");
        GetComponent<BoxCollider2D>().enabled = false;
        OnDie();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (_facing == Direction.Right ? Vector3.right : Vector3.left) * _attackRange);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag == "Ammo")
        {
            _ammosAmount += coll.gameObject.GetComponent<Ammunition>().ammoAmount;
            HUDManager.Instance.UpdateAmmoAmountLabel();
            Destroy(coll.gameObject);
        }
    }
}