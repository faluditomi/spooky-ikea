using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerDetection : MonoBehaviour
{
    private GuardStateMachine myStateMachine;

    private PlayerStateMachine playerStateMachine;

    [Tooltip("The layers that guard can't see through.")]
    [SerializeField] private LayerMask[] obstacleMasks;

    [Tooltip("The object that the FOV mesh can be assigned to.")]
    [SerializeField] private MeshFilter viewMeshFilter;

    // Add: reference to the MeshRenderer used to render the view cone
    [Tooltip("Optional: MeshRenderer for the FOV mesh. If null, will be taken from the MeshFilter's GameObject.")]
    [SerializeField] private MeshRenderer viewMeshRenderer;

    // Add: color for the view cone (use a material/shader that supports color & transparency)
    [Tooltip("Color of the view cone. Ensure the Material uses a shader that supports _Color (e.g. Standard with Rendering Mode = Transparent).")]
    [SerializeField] private Color viewColor = new Color(1f, 0f, 0f, 0.35f);

    private Mesh viewMesh;

    private Collider myCollider;

    [Tooltip("Increasing this number makes the FOV cone smoother at the price of a some performance.")]
    [SerializeField] private float meshResolution = 1f;
    [Tooltip("The distance from which the guard can spot the player. Represented visaully in the scene.")]
    [SerializeField] private float viewRadius = 10f;
    [Tooltip("The angle at which the guard sees in front of itself. Represented visually in the scene by a red circle.")]
    [SerializeField] [Range(0,360)] private float viewAngle = 90;
    [Tooltip("The distance from which the guard can hear the player sprint. Represented visaully in the scene by a blue circle.")]
    [SerializeField] private float hearingRunningRadius = 15f;
    [Tooltip("The distance from which the guard can hear the player sneak. Represented visaully in the scene by a green circle.")]
    [SerializeField] private float hearingSneakingRadius = 5f;

    private void Awake()
    {
        myStateMachine = GetComponent<GuardStateMachine>();

        myCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        playerStateMachine = myStateMachine.GetPlayer().GetComponent<PlayerStateMachine>();

        viewMesh = new Mesh();

        viewMesh.name = "Field of View";

        viewMeshFilter.mesh = viewMesh;

        // Ensure we have a MeshRenderer reference
        if (viewMeshRenderer == null && viewMeshFilter != null)
        {
            viewMeshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
        }

        // Apply the configured color to the material instance (creates an instance)
        if (viewMeshRenderer != null && viewMeshRenderer.material != null)
        {
            viewMeshRenderer.material.color = viewColor;
        }
    }
    //private void LateUpdate()
    //{
    //    DrawFieldOfViewCone();
    //}

    public bool IsPlayerInSight()
    {
        Vector3 vectorToPlayer = (myStateMachine.GetPlayer().position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, myStateMachine.GetPlayer().position);

        if (Vector3.Angle(transform.forward, vectorToPlayer) < viewAngle / 2f && distanceToPlayer < viewRadius)
        {
            foreach (LayerMask mask in obstacleMasks)
            {
                if (Physics.Raycast(transform.position, vectorToPlayer, distanceToPlayer, mask))
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }
    
    public bool IsPlayerMakingNoiseRun()
    {
        if(!playerStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Sprinting))
        {
            return false;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, myStateMachine.GetPlayer().position);

        return distanceToPlayer < hearingRunningRadius;
    }
    
    public bool IsPlayerMakingNoiseSneak()
    {
        if(!playerStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Sneaking))
        {
            return false;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, myStateMachine.GetPlayer().position);

        return distanceToPlayer < hearingSneakingRadius;
    }


    //Returns the vector that is at a certain angle from the guard.
    public Vector3 VectorFromAngle(float angleInDegrees, bool isAngleGlobal)
    {
        if(!isAngleGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void DrawFieldOfViewCone()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);

        //The angle between two vertices.
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();

        //Casting a number of rays according to the mesh resolution.
        for(int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;

            ViewCastInfo newViewCast = ViewCast(angle);

            viewPoints.Add(newViewCast.point);
        }

        //The number of points we have in our mesh.
        int vertexCount = viewPoints.Count + 1;

        //The number of points we shoot rays to.
        Vector3[] vertices = new Vector3[vertexCount];

        //The number of trianges that make up our mesh.
        int[] triangles = new int[(vertexCount - 2) * 3];

        //The starting point has to be in local space, so it's relative to the guard.
        vertices[0] = Vector3.zero;

        //-1 because we already set the first vertex in the previous line.
        for(int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            //Setting up the array of triangles for the mesh in the form that Unity requires.
            if(i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
            
                triangles[i * 3 + 1] = i + 1;

                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;

        viewMesh.triangles = triangles;

        viewMesh.RecalculateNormals();
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = VectorFromAngle(globalAngle, true);
        RaycastHit hit;
        Vector3 feetPos = new Vector3(transform.position.x, transform.position.y - myCollider.bounds.extents.y + 0.2f, transform.position.z);

        foreach (LayerMask mask in obstacleMasks)
        {
            if(Physics.Raycast(feetPos, direction, out hit, viewRadius, mask))
            {
                Vector3 adjustedHitPoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                return new ViewCastInfo(true, adjustedHitPoint, hit.distance, globalAngle);
            }
        }

        Vector3 endPoint = new Vector3(
            feetPos.x + direction.x * viewRadius,
            transform.position.y,
            feetPos.z + direction.z * viewRadius
        );
        return new ViewCastInfo(false, endPoint, viewRadius, globalAngle);
    }

    public float GetViewRadius()
    {
        return viewRadius;
    }

    public float GetViewAngle()
    {
        return viewAngle;
    }

    public float GetHearingRadiusRun()
    {
        return hearingRunningRadius;
    }

    public float GetHearingRadiusSneak()
    {
        return hearingSneakingRadius;
    }

    //A struct to hold the necessary info about the vertices of our mesh.
    public struct ViewCastInfo
    {
        public bool hit;
        
        public Vector3 point;

        public float distance;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;

            this.point = point;

            this.distance = distance;

            this.angle = angle;
        }
    }

    // Add: runtime API to change the view cone color
    public void SetViewColor(Color color)
    {
        viewColor = color;

        if (viewMeshRenderer == null && viewMeshFilter != null)
        {
            viewMeshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
        }

        if (viewMeshRenderer != null && viewMeshRenderer.sharedMaterial != null)
        {
            viewMeshRenderer.sharedMaterial.color = viewColor;
        }
    }

    // Add: apply color when component is enabled at runtime
    private void OnEnable()
    {
        SetViewColor(viewColor);
    }

    // Add: apply color immediately in the editor when the value changes in the Inspector
    private void OnValidate()
    {
        // Avoid calling during domain reloads when serialization may be incomplete.
        if (!Application.isPlaying)
        {
            // Ensure references exist before trying to set color
            if (viewMeshFilter != null && viewMeshRenderer == null)
            {
                viewMeshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
            }
        }
        SetViewColor(viewColor);
    }
}
