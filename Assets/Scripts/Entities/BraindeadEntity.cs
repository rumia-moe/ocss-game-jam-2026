using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BraindeadEntity : Entity
{

    private NavMeshAgent agent;

    public float wanderRange = 3f;

    private Vector2 GetNewDestination()
    {

        Vector2 target = Random.insideUnitCircle * wanderRange + (Vector2) transform.position;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, wanderRange, NavMesh.AllAreas))
            return hit.position;
        

        return transform.position;

    }

    protected override void Start()
    {

        base.Start();

        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.SetDestination(GetNewDestination());

    }

    protected override void Update()
    {

        base.Update();

        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            return;
            
        agent.SetDestination(GetNewDestination());

    }

}
