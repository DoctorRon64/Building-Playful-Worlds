using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum StateEnum { Idle, Patrol, Attack }
    public StateEnum currentState;
    public float ViewDistance = 5;
	public float CooldownTimer;
	public float CooldownStartTime;

	[Header("NavMeshAgent")]
    public NavMeshAgent NavMeshAgent;
	[SerializeField] private PatrolPoints[] PatrolPoints;
    public int patrolIndex = 0;

	[Header("Reference Object")]
	public GameObject Player;
	private Animator anim;

	[Header("Combat")]
	public float Health;
	public bool AttackingPlayer;

	private void Awake()
	{
		NavMeshAgent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
	}

	private void Start()
	{
        if (PatrolPoints.Length != 0)
		{
            NavMeshAgent.SetDestination(PatrolPoints[patrolIndex].transform.position);
		}
	}

	private void Update()
	{
		CheckState();
		IfHealthIsToLow();
	}
	private void CheckState()
	{
        switch (currentState)
		{
			case StateEnum.Idle: IdleBehaviour(); break;
			case StateEnum.Patrol: PatrolBehaviour(); break;
			case StateEnum.Attack: AttackBehaviour(); break;
   		}
	}

	private void IdleBehaviour()
	{
		AttackingPlayer = false;
        anim.SetInteger("EnemyState", 0);
		if (Vector3.Distance(transform.position, Player.transform.position) < ViewDistance)
		{
			currentState = StateEnum.Attack;
		}

		CooldownTimer = CooldownStartTime;
		CooldownTimer -= 1000 * Time.deltaTime;
		if (CooldownTimer <= 0)
		{
			NavMeshAgent.SetDestination(PatrolPoints[patrolIndex].transform.position);
			currentState = StateEnum.Patrol;
		}
	}

	private void PatrolBehaviour()
	{
		AttackingPlayer = false;
		anim.SetInteger("EnemyState", 1);
		if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
		{
			patrolIndex++;
			if (patrolIndex < PatrolPoints.Length)
			{
				currentState = StateEnum.Idle;
			}

			if (patrolIndex >= PatrolPoints.Length)
			{
				patrolIndex = -1;
			}
		}

		if (Vector3.Distance(transform.position, Player.transform.position) < ViewDistance)
		{
			currentState = StateEnum.Attack;
		}
	}

	private void AttackBehaviour()
	{
        AttackingPlayer = true;
		anim.SetInteger("EnemyState", 2);
		NavMeshAgent.SetDestination(Player.transform.position);
		NavMeshAgent.transform.rotation = Quaternion.LookRotation((Player.transform.position - transform.position).normalized);

		if (Vector3.Distance(transform.position, Player.transform.position) > ViewDistance*2)
		{
			currentState = StateEnum.Patrol;
		}
	}

	private void OnTriggerExit(Collider collision)
	{
		if (collision.CompareTag("Sword"))
		{
			if (Player.GetComponent<PlayerMovement>().AttackingEnemy == true) 
			{
				Health -= Player.GetComponent<PlayerMovement>().AttackDamage;
			}
		}
	}
	private void IfHealthIsToLow()
	{
		if (Health < 0)
		{
			Destroy(gameObject);
		}
	}
}
