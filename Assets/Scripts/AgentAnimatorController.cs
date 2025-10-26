using UnityEngine;
using UnityEngine.AI;

public class AgentAnimatorController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Calculate movement speed (projected on ground plane)
        Vector3 velocity = agent.velocity;
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        // Update Animator parameter
        animator.SetFloat("Speed", speed);

        // // Optionally rotate smoothly to face movement direction
        // if (speed > 0.1f)
        // {
        //     transform.rotation = Quaternion.Slerp(
        //         transform.rotation,
        //         Quaternion.LookRotation(velocity.normalized),
        //         Time.deltaTime * 10f
        //     );
        // }
    }
}
