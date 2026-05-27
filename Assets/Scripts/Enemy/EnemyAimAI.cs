using UnityEngine;
using UnityEngine.AI;

public class EnemyAimAI : FirearmAimer
{
    [Header("References")]
    [SerializeField] private EnemyMovementAI movement;

    [SerializeField] private NavMeshAgent agent;

    [Header("Spread")]
    [SerializeField] private float baseSpread = 0.001f;

    [SerializeField]
    private float distanceSpreadMultiplier = 0.0085f;

    [SerializeField]
    private float movementSpreadMultiplier = 0.05f;
    
    public float AvgMovementPenalty =>
        movementPenaltySamples > 0
            ? movementPenaltyAccumulator /
              movementPenaltySamples
            : 0f;

    public float AvgSpread =>
        spreadSamples > 0
            ? spreadAccumulator / spreadSamples
            : 0f;

    private float movementPenaltyAccumulator;
    private float spreadAccumulator;

    private int movementPenaltySamples;
    private int spreadSamples;
    
    void Awake()
    {
        if (!movement)
            movement = GetComponentInParent<EnemyMovementAI>();

        if (!agent)
            agent = GetComponentInParent<NavMeshAgent>();
    }

    public Vector3 GetShotDirection(Vector3 targetPosition)
    {
        Vector3 currentAimDirection =
            GetAimDirection();

        float distance =
            Vector3.Distance(
                aimRoot.position,
                targetPosition
            );

        float spread =
            ComputeSpread(distance);

        Vector3 shotDirection = 
            ApplySpread(currentAimDirection, spread);
        
        return shotDirection;
    }


    private float ComputeSpread(float distance)
    {
        float spread = baseSpread;

        float distancePenalty =
            distance * distanceSpreadMultiplier;

        spread += distancePenalty;

        float movementPenalty = 0f;

        if (agent)
        {
            movementPenalty =
                agent.velocity.magnitude *
                movementSpreadMultiplier;

            spread += movementPenalty;
        }

        // ----------------------------------- ACCUMULATE ----------------------------------- //

        movementPenaltyAccumulator += movementPenalty;
        movementPenaltySamples++;

        spreadAccumulator += spread;
        spreadSamples++;

        return spread;
    }
    
    public void ResetStepMetrics()
    {
        movementPenaltyAccumulator = 0f;
        spreadAccumulator = 0f;

        movementPenaltySamples = 0;
        spreadSamples = 0;
    }
    
    private Vector3 ApplySpread(
        Vector3 direction,
        float spread)
    {
        float horizontal =
            Random.Range(-spread, spread);

        float vertical =
            Random.Range(-spread, spread);

        Vector3 spreadOffset =
            aimRoot.right * horizontal +
            aimRoot.up * vertical;

        return (direction + spreadOffset).normalized;
    }
    
}

