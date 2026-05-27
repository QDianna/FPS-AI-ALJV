using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class QTableSaveData
{
    public float epsilon;
    
    public List<QTableEntry> entries = new();
}

[Serializable]
public class QTableEntry
{
    public int stateKey;
    public float[] qValues;
}



public enum AttackActionType
{
    AggressivePush,
    DefensiveRetreat,
    HoldPosition,
    StrafeLeft,
    StrafeRight
}

enum CombatAdvantage
{
    Losing,
    Even,
    Winning
}

enum DistanceState
{
    TooClose,
    Close,
    Medium,
    Far
}

enum PressureState
{
    Safe,
    UnderFire
}

struct RLState
{
    public CombatAdvantage advantage;

    public DistanceState distance;

    public PressureState pressure;


    public override int GetHashCode()
    {
        return (int)advantage * 100 +
               (int)distance * 10 +
               (int)pressure * 1;
    }
}


public class EnemyCombatAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimTarget;

    [SerializeField] private FirearmController firearm;

    [SerializeField] private EnemyAimAI aimer;

    [SerializeField] private EnemyMovementAI movement;

    [SerializeField] private EnemyHealth healthSystem;

    [Header("Vision")]
    [SerializeField] private float eyeHeight = 1.25f;

    [SerializeField] private float fov = 120f;
   
    [Header("Debug")]
    [SerializeField] private bool debugCombat = true;
    
    [Header("RL Parameters")]
    [SerializeField] private float learningRate = 0.2f;

    [SerializeField] private float discount = 0.9f;

    [SerializeField] private float epsilon = 0.9f;

    [SerializeField] private float epsilonDecay = 0.95f;

    [SerializeField] private float minEpsilon = 0.3f;
    
    [SerializeField] private float actionDuration = 0.9f;

    [SerializeField] private float underFireDuration = 1.8f;

    [Header("State Encoding Parameters")]
    [SerializeField] private float advantageThreshold = 20f;
    
    [SerializeField] private float tooCloseRange = 5f;

    [SerializeField] private float closeRange = 10f;

    [SerializeField] private float mediumRange = 15f;
    
    // reward-shaping metrics 
    
    private float oldDistanceToPlayer;
    
    private float newDistanceToPlayer;
    
    private float damageDealtThisStep;

    private float damageTakenThisStep;
    
    private int shotsThisStep;
    
    private int hitsThisStep;

    private float lastDamageTakenTime;

    // combat and decision parameters
    private bool isInCombat;

    private float actionCommitTimer;
    
    // Q table
    private Dictionary<int, float[]> qTable = new();
    
    [SerializeField] private bool loadExistingQTable;
    [SerializeField] private bool saveQTable = true;

    // RL
    private AttackActionType[] actions;
    
    private AttackActionType currentAction;

    private RLState previousState;

    private int previousActionIndex;

    private bool hasPreviousState;

    // log files
    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "qtable.json");
    
    private string logPath =>
        Path.Combine(Application.persistentDataPath, "rl_log.txt");

    
    
    void Start()
    {
        File.WriteAllText(logPath, "");
        
        actions =
            (AttackActionType[])
            Enum.GetValues(typeof(AttackActionType));

        if (loadExistingQTable)
        {
            LoadQTable();
        }
        else
            qTable.Clear();

        firearm.OnDamageDealt += HandleDamageDealt;
        firearm.OnShotsFired += HandleShotFired;
        healthSystem.OnDamageTaken += HandleDamageTaken;
        
    }

    void Update()
    {
        if (!isInCombat)
            return;

        Vector3 aimDirection =
            (aimTarget.position - aimer.aimRoot.position)
            .normalized;

        aimer.SetAimDirection(aimDirection);
        movement.SetLookDirection(aimDirection);
    }

    void OnApplicationQuit()
    {
        if (saveQTable)
            SaveQTable();
    }

    // ----------------------------------- BEHAVIOUR - COMBAT ----------------------------------- //

    public void EnterCombat()
    {
        damageDealtThisStep = 0f;
        damageTakenThisStep = 0f;
        shotsThisStep = 0;
        hitsThisStep = 0;   
        
        actionCommitTimer = 0f;
        
        isInCombat = true;

        hasPreviousState = false;
    }

    public void ExitCombat()
    {
        isInCombat = false;

        hasPreviousState = false;
        actionCommitTimer = 0f;
    }

    public void ResetCombat()
    {
        ExitCombat();

        ResetStepMetrics();
        
        epsilon *= epsilonDecay;
        epsilon =
            Mathf.Max(
                minEpsilon,
                epsilon
            );
    }
    
    void ResetStepMetrics()
    {
        damageDealtThisStep = 0f;
        damageTakenThisStep = 0f;
        lastDamageTakenTime = 0f;
        
        shotsThisStep = 0;
        hitsThisStep = 0;

        aimer.ResetStepMetrics();
    }

    public void TickCombat()
    {
        if (!isInCombat)
            return;

        if (actionCommitTimer > 0f)
        {
            // continue current decision
            actionCommitTimer -= Time.deltaTime;
            
            ExecuteAction(currentAction, false);
            
            return;
        }
        
        // new decision
        newDistanceToPlayer = Vector3.Distance(transform.position, aimTarget.position);
        
        RLState currentState = BuildRLState();

        float reward = 0f;
        if (hasPreviousState)
        {
            reward = CalculateReward(
                previousState,
                previousActionIndex,
                currentState
            );

            UpdateQ(
                previousState,
                previousActionIndex,
                currentState,
                reward
            );
        }
        
        // reset metrics 
        ResetStepMetrics();   
        
        AttackActionType action = DecideAction(currentState);

        currentAction = action;

        actionCommitTimer = actionDuration;
        
        oldDistanceToPlayer = Vector3.Distance(transform.position, aimTarget.position);

        ExecuteAction(action, true);

        previousState = currentState;

        previousActionIndex =
            Array.IndexOf(actions, action);

        hasPreviousState = true;
    }

    // ----------------------------------- RL ----------------------------------- //

    RLState BuildRLState()
    {
        return new RLState
        {
            advantage = GetCombatAdvantage(),

            distance =
                GetDistanceState(
                    GetDistanceToTarget()
                ),

            pressure = GetPressureState(),
        };
    }

    AttackActionType DecideAction(RLState state)
    {
        EnsureStateExists(state);

        List<AttackActionType> validActions = GetValidActions();

        if (Random.value < epsilon)
        {
            return validActions[
                Random.Range(0, validActions.Count)
            ];
        }

        return GetBestAction(
            state,
            validActions
        );
    }

    List<AttackActionType> GetValidActions()
    {
        List<AttackActionType> valid = new();

        if (movement.CanPushForward())
            valid.Add(AttackActionType.AggressivePush);

        if (movement.CanRetreat())
            valid.Add(AttackActionType.DefensiveRetreat);

        if (movement.CanStrafeLeft())
            valid.Add(AttackActionType.StrafeLeft);

        if (movement.CanStrafeRight())
            valid.Add(AttackActionType.StrafeRight);

        valid.Add(AttackActionType.HoldPosition);

        return valid;
    }

    AttackActionType GetBestAction(
        RLState state,
        List<AttackActionType> validActions)
    {
        int key = state.GetHashCode();

        float[] qValues = qTable[key];

        AttackActionType bestAction =
            validActions[0];

        float bestQ =
            qValues[
                Array.IndexOf(actions, bestAction)
            ];

        foreach (AttackActionType action in validActions)
        {
            int index =
                Array.IndexOf(actions, action);

            if (qValues[index] > bestQ)
            {
                bestQ = qValues[index];
                bestAction = action;
            }
        }

        return bestAction;
    }

    void EnsureStateExists(RLState state)
    {
        int key = state.GetHashCode();

        if (qTable.ContainsKey(key))
            return;

        qTable[key] = new float[actions.Length];

        for (int i = 0; i < actions.Length; i++)
        {
            qTable[key][i] =
                Random.Range(0f, 0.1f);
        }
    }

    void UpdateQ(
        RLState oldState,
        int actionIndex,
        RLState newState,
        float reward)
    {
        EnsureStateExists(oldState);
        EnsureStateExists(newState);

        int oldKey = oldState.GetHashCode();
        int newKey = newState.GetHashCode();

        float[] oldValues = qTable[oldKey];
        float[] newValues = qTable[newKey];

        float maxNext = Mathf.Max(newValues);
        
        oldValues[actionIndex] +=
            learningRate *
            (
                reward +
                discount * maxNext -
                oldValues[actionIndex]
            );
    }

    // ----------------------------------- REWARD ----------------------------------- //
    float CalculateAccuracyReward(RLState oldState, RLState newState)
    {
        if (shotsThisStep == 0)
            return -0.01f;

        float accuracy = (float)hitsThisStep / shotsThisStep;

        float reward = accuracy * 0.3f;

        bool repositioningMadeSense = false;

        float deltaDistance = oldDistanceToPlayer - newDistanceToPlayer;
        
        switch (oldState.advantage)
        {
            case CombatAdvantage.Winning:
            case CombatAdvantage.Even:

                repositioningMadeSense =
                    deltaDistance > 0.5f &&
                    newState.distance != DistanceState.TooClose;

                break;

            case CombatAdvantage.Losing:

                repositioningMadeSense =
                    deltaDistance < -0.5f;

                break;
        }

        float movementPenalty = aimer.AvgMovementPenalty * 0.2f;

        // movement spread este atenuat daca miscarea face o repozitionare buna

        if (repositioningMadeSense)
        {
            movementPenalty *= 0.15f;

            reward += 0.03f;
        }

        reward -= movementPenalty;

        return reward;
    }
    
    float CalculateReward(RLState oldState, int actionIndex, RLState newState)
    {
        float reward = 0f;

        // context-dependent damage weights
        float dealtWeight;
        float takenWeight;

        switch (oldState.advantage)
        {
            case CombatAdvantage.Winning:
                dealtWeight = 0.07f;
                takenWeight = 0.05f;
                break;
            case CombatAdvantage.Losing:
                dealtWeight = 0.05f;
                takenWeight = 0.07f;
                break;
            default:
                dealtWeight = 0.06f;
                takenWeight = 0.06f;
                break;
        }

        reward += damageDealtThisStep * dealtWeight;
        reward -= damageTakenThisStep * takenWeight;

        // accuracy
        reward += CalculateAccuracyReward(oldState, newState);

        if (newState.distance == DistanceState.TooClose)
            reward -= 0.8f;

        if (shotsThisStep > 0 && hitsThisStep == 0)
            reward -= 0.02f;
        
        // logging
        string logState =
            $"Episode:{GameManager.Instance.currentEpisode} | " +
            $"State:[advantage:{oldState.advantage}, " +
            $"distance:{oldState.distance}, " +
            $"pressure:{oldState.pressure}] | " +
            $"[ACTION]:{actions[actionIndex]}";

        string logReward =
            $"[REWARD] {reward:F2} | " +
            $"DMG Dealt:{damageDealtThisStep:F1} | " +
            $"DMG Taken:{damageTakenThisStep:F1} | " +
            $"Delta distance:{oldDistanceToPlayer - newDistanceToPlayer} | " +
            $"Shots Hit/Fired:{hitsThisStep}/{shotsThisStep} | " +
            $"AvgSpread:{aimer.AvgSpread:F3} | " +
            $"AvgMovePenalty:{aimer.AvgMovementPenalty:F3}";

        if (debugCombat)
        {
            Debug.Log(logState);
            Debug.Log(logReward);
        }

        File.AppendAllText(logPath, logState + Environment.NewLine);
        File.AppendAllText(logPath, logReward + Environment.NewLine);

        return reward;
    }

    // ----------------------------------- STATE CODING ----------------------------------- //

    DistanceState GetDistanceState(float distance)
    {
        if (distance <= tooCloseRange)
            return DistanceState.TooClose;

        if (distance <= closeRange)
            return DistanceState.Close;

        if (distance <= mediumRange)
            return DistanceState.Medium;

        return DistanceState.Far;
    }

    CombatAdvantage GetCombatAdvantage()
    {
        float advantage =
            healthSystem.health -
            PlayerController.Instance.health;

        if (advantage > advantageThreshold)
            return CombatAdvantage.Winning;

        if (advantage < -advantageThreshold)
            return CombatAdvantage.Losing;

        return CombatAdvantage.Even;
    }
    
    PressureState GetPressureState()
    {
        bool underFire =
            Time.time - lastDamageTakenTime < underFireDuration;

        if (underFire)
            return PressureState.UnderFire;

        return PressureState.Safe;
    }
    
    // ----------------------------------- SITUATION PERCEPTION ----------------------------------- //

    void HandleDamageDealt(float damage)
    {
        damageDealtThisStep += damage;
        
        hitsThisStep++;
    }

    void HandleDamageTaken(
        float damage,
        Vector3 vector3)
    {
        damageTakenThisStep += damage;

        lastDamageTakenTime = Time.time;
    }
    
    void HandleShotFired()
    {
        shotsThisStep++;
    }
    
    // ----------------------------------- ACTIONS ----------------------------------- //

    void ExecuteAction(AttackActionType action, bool startedNewAction)
    {
        Shoot();
        
        if (!startedNewAction)
            return;
        
        switch (action)
        {
            case AttackActionType.AggressivePush:
                movement.PushForward();
                break;
            
            case AttackActionType.DefensiveRetreat:
                movement.BackOff();
                break;

            case AttackActionType.StrafeLeft:
                movement.StrafeLeft();
                break;

            case AttackActionType.StrafeRight:
                movement.StrafeRight();
                break;
        }
    }

    void Shoot()
    {
        if (!firearm.CanFire())
            return;

        Vector3 shotDirection =
            aimer.GetShotDirection(
                aimTarget.position
            );

        bool hit =
            firearm.Fire(shotDirection);
    }
    
    
    
    
    // ----------------------------------- UTILS ----------------------------------- //
    
    public bool CanSeePlayer()
    {
        return IsInFOV() && HasLineOfSight();
    }

    bool IsInFOV()
    {
        Vector3 origin = GetEyePosition();

        Vector3 toTarget =
            (aimTarget.position - origin).normalized;

        float angle =
            Vector3.Angle(
                transform.forward,
                toTarget
            );

        return angle <= fov * 0.5f;
    }

    bool HasLineOfSight()
    {
        Vector3 origin = GetEyePosition();

        Vector3 direction =
            (aimTarget.position - origin).normalized;

        float distance =
            Vector3.Distance(origin, aimTarget.position);

        /*Debug.DrawRay(
            origin,
            direction * distance,
            Color.red,
            0.1f
        );*/
        
        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distance))
        {
            /*if (hit.transform.name != "PlayerFPS")
                Debug.Log($"LOS HIT: {hit.transform.name}");*/
            
            return hit.transform.root == aimTarget.root;
        }
        

        return false;
    }

    Vector3 GetEyePosition()
    {
        return transform.position +
               transform.up * eyeHeight;
    }

    float GetDistanceToTarget()
    {
        return Vector3.Distance(
            transform.position,
            aimTarget.position
        );
    }

    


    // ----------------------------------- Q TABLE ----------------------------------- //
    public void DebugQTable()
    {
        // Debug.Log("========== Q TABLE ==========");
        int count = 1;
        foreach (var entry in qTable)
        {
            int key = entry.Key;
            float[] values = entry.Value;

            CombatAdvantage advantage =
                (CombatAdvantage)(key / 100);

            DistanceState distance =
                (DistanceState)((key % 100) / 10);

            PressureState pressure =
                (PressureState)(key % 10);

            string log =
                $"State {count}:[Adv:{advantage}, " +
                $"Dist:{distance}, " +
                $"Pressure:{pressure}] || ";

            for (int i = 0; i < values.Length; i++)
            {
                log += $"{actions[i]}:{values[i]:F2} ";
            }

            if (debugCombat)
                Debug.Log(log);
            
            File.AppendAllText(logPath, log + Environment.NewLine);

            count++;
        }
    }
    public void SaveQTable()
    {
        QTableSaveData saveData = new();
            
        saveData.epsilon = Mathf.Max(minEpsilon, epsilon);
        
        foreach (var pair in qTable)
        {
            saveData.entries.Add(
                new QTableEntry
                {
                    stateKey = pair.Key,
                    qValues = pair.Value
                });
        }

        string json =
            JsonUtility.ToJson(saveData, true);

        File.WriteAllText(SavePath, json);

        Debug.Log($"[QTABLE] Saved {qTable.Count} states");
    }

    public void LoadQTable()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[QTABLE] No save found");
            return;
        }

        string json =
            File.ReadAllText(SavePath);

        QTableSaveData saveData =
            JsonUtility.FromJson<QTableSaveData>(json);

        qTable.Clear();

        epsilon = Mathf.Max(
            minEpsilon,
            saveData.epsilon
        );
        
        foreach (QTableEntry entry in saveData.entries)
        {
            if (entry.qValues == null ||
                entry.qValues.Length != actions.Length)
            {
                continue;
            }

            qTable[entry.stateKey] =
                entry.qValues;
        }

        Debug.Log($"[QTABLE] Loaded {qTable.Count} states");
        DebugQTable();
    }

    [ContextMenu("Delete QTable")]
    void DeleteQTable()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);

            Debug.Log("[QTABLE] Deleted");
        }
        else
        {
            Debug.Log("[QTABLE] File not found");
        }
    }
}


