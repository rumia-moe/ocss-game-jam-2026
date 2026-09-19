using UnityEngine;

public class IKChain : MonoBehaviour
{
    [Header("Profile")]
    public FishProfile profile;

    [Header("Target")]
    public Transform target;
    public bool rootIsFixed = true;

    [HideInInspector] public Vector3[] points;
    [HideInInspector] public float[] heights;

    private float[] _segLengths;
    private Vector3 _smoothTip;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        if (profile == null)
            return;

        // ------------------------------------------------------------
        // IMPORTANT:
        // If the profile contains the original painter control points,
        // use THOSE as the IK chain points.
        //
        // Do not reconstruct the shape from profile.segments.
        // ------------------------------------------------------------

        if (profile.controlPoints != null &&
            profile.controlPoints.Length >= 2)
        {
            InitFromControlPoints();
        }
        else
        {
            // Backwards compatibility for old profiles.
            InitFromSegments();
        }

        if (points != null && points.Length > 0)
            _smoothTip = points[points.Length - 1];
    }

    void InitFromControlPoints()
    {
        Vector2[] saved = profile.controlPoints;

        int count = saved.Length;

        points = new Vector3[count];
        heights = new float[count];
        _segLengths = new float[count - 1];

        Vector3 origin = transform.position;

        // ------------------------------------------------------------
        // The painter stores:
        //
        // X = distance along fish
        // Y = dorsal height above spine
        //
        // Runtime IK points live on the SPINE, so Y is intentionally
        // zero here.
        // ------------------------------------------------------------

        float minX = saved[0].x;

        for (int i = 0; i < count; i++)
        {
            float x = saved[i].x - minX;

            points[i] = origin + Vector3.right * x;
        }

        // Segment lengths come from the actual saved control points.
        for (int i = 0; i < count - 1; i++)
        {
            _segLengths[i] =
                Vector3.Distance(points[i], points[i + 1]);

            _segLengths[i] = Mathf.Max(_segLengths[i], 0.001f);
        }

        // ------------------------------------------------------------
        // Heights come directly from the saved silhouette.
        //
        // FishProfile.controlPoints contains dorsal height.
        // The renderer expects full dorsal-to-ventral height.
        // ------------------------------------------------------------

        for (int i = 0; i < count; i++)
        {
            heights[i] = Mathf.Max(
                Mathf.Abs(saved[i].y) * 2f,
                0.001f
            );
        }
    }

    void InitFromSegments()
    {
        if (profile.segments == null ||
            profile.segments.Length == 0)
            return;

        int segmentCount = profile.segments.Length;

        // segments describe the spaces BETWEEN points.
        int pointCount = segmentCount + 1;

        points = new Vector3[pointCount];
        heights = new float[pointCount];
        _segLengths = new float[segmentCount];

        Vector3 origin = transform.position;

        points[0] = origin;

        for (int i = 0; i < segmentCount; i++)
        {
            float length = Mathf.Max(
                profile.segments[i].length,
                0.001f
            );

            _segLengths[i] = length;

            points[i + 1] =
                points[i] + Vector3.right * length;
        }

        for (int i = 0; i < pointCount; i++)
        {
            if (i < segmentCount)
            {
                heights[i] =
                    Mathf.Max(
                        profile.segments[i].height,
                        0.001f
                    );
            }
            else
            {
                // Last point uses the final segment height.
                heights[i] =
                    Mathf.Max(
                        profile.segments[segmentCount - 1].height,
                        0.001f
                    );
            }
        }
    }

    void Update()
    {
        if (target == null ||
            points == null ||
            points.Length < 2)
            return;

        _smoothTip = Vector3.Lerp(
            _smoothTip,
            target.position,
            profile.followSpeed
        );

        SolveFABRIK(_smoothTip);
    }

    void SolveFABRIK(Vector3 tipTarget)
    {
        if (points == null ||
            points.Length < 2 ||
            _segLengths == null)
            return;

        int last = points.Length - 1;

        // The original implementation mixed up root and tip.
        //
        // Here:
        // points[0]     = root
        // points[last]  = head/tip
        //
        // The target drives the LAST point.

        Vector3 fixedRoot = points[0];

        for (int iter = 0;
             iter < profile.solverIterations;
             iter++)
        {
            // --------------------------------------------------------
            // Forward pass:
            // put the tip on the target.
            // --------------------------------------------------------

            points[last] = tipTarget;

            for (int i = last - 1; i >= 0; i--)
            {
                Vector3 delta = points[i] - points[i + 1];

                Vector3 dir;

                if (delta.sqrMagnitude > 0.000001f)
                    dir = delta.normalized;
                else
                    dir = Vector3.left;

                points[i] =
                    points[i + 1] +
                    dir * _segLengths[i];
            }

            // --------------------------------------------------------
            // Backward pass:
            // restore fixed root.
            // --------------------------------------------------------

            if (rootIsFixed)
                points[0] = fixedRoot;

            for (int i = 1; i <= last; i++)
            {
                Vector3 delta = points[i] - points[i - 1];

                Vector3 dir;

                if (delta.sqrMagnitude > 0.000001f)
                    dir = delta.normalized;
                else
                    dir = Vector3.right;

                points[i] =
                    points[i - 1] +
                    dir * _segLengths[i - 1];
            }
        }
    }

    public Vector3 GetSegmentDirection(int i)
    {
        if (points == null ||
            i < 0 ||
            i >= points.Length - 1)
        {
            return Vector3.right;
        }

        Vector3 delta = points[i + 1] - points[i];

        if (delta.sqrMagnitude < 0.000001f)
            return Vector3.right;

        return delta.normalized;
    }

    public float GetHeight(int i)
    {
        if (heights == null ||
            heights.Length == 0)
            return 0f;

        i = Mathf.Clamp(i, 0, heights.Length - 1);

        return heights[i];
    }
}
