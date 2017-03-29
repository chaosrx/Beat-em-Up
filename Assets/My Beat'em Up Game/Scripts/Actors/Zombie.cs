using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ZombieStateEnum
{
    None,
    Patrol,
    Seek,
    Attack,
    Dead
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RefreshZOrider))]
public class Zombie : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidbody2d;

    [SerializeField]
    private int _maxHealth;
    public int maxHealth { get { return _maxHealth; } }
    [SerializeField]
    private float _hurtTime;//被攻击后的硬直时间
    [SerializeField]
    private float _moveSpeed;
    public float moveSpeed { get { return _moveSpeed; } }
    //[SerializeField]
    //private float _patrolDeltaTime;
    //public float patrolDeltaTime { get { return _patrolDeltaTime; } }
    //[SerializeField]
    //private float _minPatrolDistance;
    //public float minPatrolDistance { get { return _minPatrolDistance; } }
    //[SerializeField]
    //private float _maxPatrolDistance;
    //public float maxPatrolDistance { get { return _maxPatrolDistance; } }
    //[SerializeField]
    //private float _stopDistance;
    //public float stopDistance { get { return _stopDistance; } }
    //[SerializeField]
    //private float _minStayTime;
    //public float minStayTime { get { return _minStayTime; } }
    //[SerializeField]
    //private float _maxStayTime;
    //public float maxStayTime { get { return _maxStayTime; } }
    [SerializeField]
    private float _scanRange;
    public float scanRange { get { return _scanRange; } }
    [SerializeField]
    private int _damage;
    public int damage { get { return _damage; } }
    [SerializeField]
    private Vector2 _attackRange;
    public Vector2 attackRange { get { return _attackRange; } }
    [SerializeField]
    private float _attackCD;
    public float attackCD { get { return _attackCD; } }

    private int _health;
    private Direction _facing;
    private float _remainHurtTime;
    private Transform _target;
    public Transform target { get { return _target; } }

    public event Action OnDie;

    //四种状态：巡逻，追赶，攻击，死亡
    private Dictionary<ZombieStateEnum, ZombieState> _states;
    private ZombieState _currState = null;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2d = GetComponent<Rigidbody2D>();
    }

	// Use this for initialization
	void Start ()
    {
        _health = _maxHealth;
        _facing = Direction.Left;
        _remainHurtTime = 0.0f;
        _target = null;

        _states = new Dictionary<ZombieStateEnum, ZombieState>();


        _states.Add(ZombieStateEnum.Patrol, new ZombieIdleState());
        _states.Add(ZombieStateEnum.Seek, new ZombieSeekState());
        _states.Add(ZombieStateEnum.Attack, new ZombieAttackState());
        _states.Add(ZombieStateEnum.Dead, new ZombieDeadState());

        ChangeState(ZombieStateEnum.Patrol);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(_remainHurtTime >0.0f)
        {
            return;
        }

        ZombieStateEnum newState = _currState.Update(this);
        if(newState != ZombieStateEnum.None)
        {
            ChangeState(newState);
        }
	}

    void FaceTo(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                GetComponent<SpriteRenderer>().flipX = true;
                break;
            case Direction.Right:
                GetComponent<SpriteRenderer>().flipX = false;
                break;
        }
        _facing = direction;
    }

    void ChangeState(ZombieStateEnum newState)
    {
        if(_currState != null)
        {
            _currState.OnExit(this);
        }
        _currState = _states[newState];
        _currState.OnEnter(this);
    }

    void OnAttacked(object attackData)
    {
        AttackData attack = (AttackData)attackData;
        _health -= attack.damage;
        StopMoving();
        FaceTo(attack.from);
        if(_health<=0)
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
        ChangeState(ZombieStateEnum.Dead);
        OnDie();
    }

    public bool Scan()
    {
        Collider2D coll = Physics2D.OverlapCircle(transform.position, _scanRange, 1 << LayerMask.NameToLayer("Player"));
        if (coll != null)
        {
            _target = coll.transform;
            return true;
        }
        return false;
    }

    //攻击
    public void Attack()
    {
        FaceTo((target.position - transform.position).x > 0 ? Direction.Right : Direction.Left);//面向目标方向
        _animator.SetInteger("AttackId", UnityEngine.Random.Range(1, 3));
        _animator.SetTrigger("Attack");
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, _facing == Direction.Right ? Vector2.right : Vector2.left, _attackRange.x, 1 << LayerMask.NameToLayer("Player"));
        if (hitInfo)
        {
            hitInfo.collider.gameObject.SendMessage("OnAttacked", new AttackData() { damage = _damage, from = (hitInfo.collider.transform.position.x - transform.position.x > 0.0f) ? Direction.Left : Direction.Right });
        }
    }

    //朝指定方向移动
    public void Move(Vector2 direction)
    {
        FaceTo(direction.x > 0 ? Direction.Right : Direction.Left);
        _animator.SetBool("IsMoving", true);
        _rigidbody2d.velocity = direction.normalized * _moveSpeed;
    }

    //停止移动
    public void StopMoving()
    {
        _animator.SetBool("IsMoving", false);
        _rigidbody2d.velocity = Vector2.zero;
    }
}

public abstract class ZombieState
{
    public abstract ZombieStateEnum Update(Zombie zombie);
    public virtual void OnEnter(Zombie zombie) { }
    public virtual void OnExit(Zombie zombie) { }
}

public class ZombieIdleState : ZombieState
{
    public override ZombieStateEnum Update(Zombie zombie)
    {
        if(zombie.Scan())
        {
            return ZombieStateEnum.Seek;
        }

        return ZombieStateEnum.None;
    }
}

public class ZombieSeekState : ZombieState
{
    public override ZombieStateEnum Update(Zombie zombie)
    {
        //目标死亡进入Idle状态
        if(zombie.target.GetComponent<Hero>().IsDead)
        {
            return ZombieStateEnum.Patrol;
        }
        //进入攻击范围的一半时进入攻击状态
        float x = zombie.target.position.x - zombie.transform.position.x;
        float y = zombie.target.position.y - zombie.transform.position.y;
        if (Mathf.Abs(x) <= zombie.attackRange.x / 2.0f && Mathf.Abs(y) <= zombie.attackRange.y / 2.0f)
        {
            return ZombieStateEnum.Attack;
        }
        //否则不断朝目标移动
        Vector2 direction = Vector2.zero;
        if (Mathf.Abs(x) > zombie.attackRange.x / 2.0f)
        {
            direction.x = (x > 0.0f ? 1.0f : -1.0f);
        }
        if (Mathf.Abs(y) > zombie.attackRange.y / 2.0f)
        {
            direction.y = (y > 0.0f ? 1.0f : -1.0f);
        }
        zombie.Move(direction);

        return ZombieStateEnum.None;
    }
}

public class ZombieAttackState : ZombieState
{
    private float _time;

    public override ZombieStateEnum Update(Zombie zombie)
    {
        //目标死亡进入Idle状态
        if (zombie.target.GetComponent<Hero>().IsDead)
        {
            return ZombieStateEnum.Patrol;
        }
        //如果目标超出攻击范围则切换成追逐状态
        float x = zombie.target.position.x - zombie.transform.position.x;
        float y = zombie.target.position.y - zombie.transform.position.y;
        if (Mathf.Abs(x) > zombie.attackRange.x || Mathf.Abs(y) > zombie.attackRange.y)
        {
            return ZombieStateEnum.Seek;
        }

        if (_time<=0.0f)
        {
            zombie.Attack();
            _time = zombie.attackCD;
        }
        else
        {
            _time -= Time.deltaTime;
        }

        return ZombieStateEnum.None;
    }

    public override void OnEnter(Zombie zombie)
    {
        _time = zombie.attackCD;
        zombie.StopMoving();//进入攻击状态时停止移动
    }
}

public class ZombieDeadState : ZombieState
{
    public override ZombieStateEnum Update(Zombie zombie)
    {
        return ZombieStateEnum.None;
    }

    public override void OnEnter(Zombie zombie)
    {
        zombie.StopMoving();//死亡时理所当然地停止移动
        zombie.GetComponent<Animator>().SetTrigger("Die");//播放死亡动画
        zombie.GetComponent<BoxCollider2D>().enabled = false;//关闭碰撞体积
    }
}
