using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [SerializeField] private Transform player;
    [SerializeField] Transform[] points;
    
    [SerializeField] private float chaseRadius = 15f;
    [SerializeField] private float waypointTolerance = 0.5f;

    private int index;
    private bool isChasing;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // pornește din prima pe patrol
        if (points.Length > 0)
        {
            agent.SetDestination(points[0].position);
        }
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= chaseRadius)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void ChasePlayer()
    {
        isChasing = true;
        agent.SetDestination(player.position);
    }

    private void Patrol()
    {
        if (points.Length == 0)
            return;

        // dacă venea din chase, reia patrol corect
        if (isChasing)
        {
            isChasing = false;
            agent.SetDestination(points[index].position);
        }

        // a ajuns la waypoint
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            index = (index + 1) % points.Length;
            agent.SetDestination(points[index].position);
        }
    }

}