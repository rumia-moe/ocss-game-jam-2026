using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(IKChain))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class IKChainRenderer : MonoBehaviour
{
    [Header("Appearance")]
    public Material bodyMaterial;
    public bool generateUVs = true;

    [Header("Smoothing")]
    [Range(1, 10)]
    public int subdivisionsPerSegment = 4;

    [Header("Gap Squeezing")]
    public bool enableSqueezing = true;
    public LayerMask wallLayers;

    [Range(0f, 1f)]
    public float minHeightScale = 0.3f;

    public float squeezeSmoothSpeed = 8f;

    private IKChain _chain;
    private Mesh _mesh;
    private float[] _squeezedHeights;

    private readonly List<Vector3> _verts = new();
    private readonly List<Vector2> _uvs = new();
    private readonly List<int> _tris = new();

    // ================================================================
    // Unity
    // ================================================================

    void Awake()
    {
        _chain = GetComponent<IKChain>();

        _mesh = new Mesh
        {
            name = "FishBody"
        };

        _mesh.MarkDynamic();

        GetComponent<MeshFilter>().mesh = _mesh;

        if (bodyMaterial != null)
            GetComponent<MeshRenderer>().material = bodyMaterial;
    }

    void LateUpdate()
    {
        if (_chain.points == null ||
            _chain.points.Length < 2)
            return;

        RebuildMesh();
    }

    // ================================================================
    // Catmull-Rom position
    //
    // Used ONLY for the XY spine position.
    // ================================================================

    static Vector3 CRPos(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3,
        float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result =
            0.5f *
            (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );

        // Absolutely flat 2D.
        result.z = 0f;

        return result;
    }

    // ================================================================
    // Catmull-Rom tangent
    //
    // Used only to determine the 2D direction of the fish.
    // ================================================================

    static Vector3 CRTan(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3,
        float t)
    {
        float t2 = t * t;

        Vector3 result =
            0.5f *
            (
                (-p0 + p2) +
                2f *
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
                3f *
                (-p0 + 3f * p1 - 3f * p2 + p3) * t2
            );

        // Strictly XY.
        result.z = 0f;

        if (result.sqrMagnitude < 0.000001f)
            result = Vector3.right;

        return result.normalized;
    }

    // ================================================================
    // Spine
    // ================================================================

    struct SpineRing
    {
        public Vector3 pos;

        // Forward along the fish.
        public Vector3 tan;

        // 90-degree perpendicular in the XY plane.
        public Vector3 perp;

        public float height;
        public float arcU;
        public float segT;
    }

    SpineRing[] BuildSpine()
    {
        Vector3[] pts = _chain.points;
        float[] hts = _chain.heights;

        int sourceCount = pts.Length;
        int segments = sourceCount - 1;

        int sub = Mathf.Max(1,subdivisionsPerSegment);

        int sampleCount = segments * sub + 1;

        SpineRing[] spine =
            new SpineRing[sampleCount];

        int index = 0;

        for (int seg = 0; seg < segments; seg++)
        {
            int i0 = Mathf.Max(seg - 1,0);

            int i1 = seg;

            int i2 = seg + 1;

            int i3 = Mathf.Min(seg + 2,sourceCount - 1);

            for (int s = 0; s < sub; s++)
            {
                float t =
                    s / (float)sub;

                Vector3 pos =
                    Vector3.Lerp(
                        pts[i1],
                        pts[i2],
                        t
                    );

                pos.z = 0f;


                Vector3 tan =
                CRTan(
                    pts[i0],
                    pts[i1],
                    pts[i2],
                    pts[i3],
                    t
                );

            Vector3 perp =
                new Vector3(
                    -tan.y,
                    tan.x,
                    0f
                );

            if (index > 0)
            {
                Vector3 previousPerp =
                    spine[index - 1].perp;

                // Keep the perpendicular from suddenly
                // rotating across a sharp bend.
                if (Vector3.Dot(previousPerp, perp) < 0f)
                    perp = -perp;
            }

                // IMPORTANT:
                // Do not Catmull-Rom the height.
                //
                // This prevents the taper from overshooting
                // when the fish turns.
                float height =
                    Mathf.Lerp(
                        hts[i1],
                        hts[i2],
                        t
                    );

                spine[index++] =
                    new SpineRing
                    {
                        pos = pos,
                        tan = tan,
                        perp = perp,
                        height =
                            Mathf.Max(
                                height,
                                0.001f
                            ),
                        segT =
                            (seg + t) /
                            segments
                    };
            }
        }

        // ============================================================
        // Exact final control point.
        // ============================================================

        Vector3 lastTan =
            pts[sourceCount - 1] -
            pts[sourceCount - 2];

        lastTan.z = 0f;

        if (lastTan.sqrMagnitude < 0.000001f)
            lastTan = Vector3.right;

        lastTan.Normalize();

        Vector3 lastPerp =
            new Vector3(
                -lastTan.y,
                lastTan.x,
                0f
            );

        spine[index] =
            new SpineRing
            {
                pos =
                    new Vector3(
                        pts[sourceCount - 1].x,
                        pts[sourceCount - 1].y,
                        0f
                    ),

                tan = lastTan,

                perp = lastPerp,

                height =
                    Mathf.Max(
                        hts[sourceCount - 1],
                        0.001f
                    ),

                segT = 1f
            };

        // ============================================================
        // Arc length.
        // ============================================================

        float arc = 0f;

        spine[0].arcU = 0f;

        for (int i = 1; i < spine.Length; i++)
        {
            arc +=
                Vector3.Distance(
                    spine[i - 1].pos,
                    spine[i].pos
                );

            spine[i].arcU = arc;
        }

        float totalArc =
            Mathf.Max(
                arc,
                0.0001f
            );

        for (int i = 0; i < spine.Length; i++)
        {
            spine[i].arcU /=
                totalArc;
        }

        return spine;
    }

    // ================================================================
    // Squeeze
    //
    // Squeeze across the fish's LOCAL 2D perpendicular,
    // not world up/down.
    // ================================================================

    void ApplySqueeze(SpineRing[] spine)
    {
        if (_squeezedHeights == null ||
            _squeezedHeights.Length != spine.Length)
        {
            _squeezedHeights =
                new float[spine.Length];

            for (int i = 0; i < spine.Length; i++)
            {
                _squeezedHeights[i] =
                    spine[i].height;
            }
        }

        for (int i = 0; i < spine.Length; i++)
        {
            float desired =
                spine[i].height;

            if (desired > 0.001f)
            {
                Vector3 perp =
                    spine[i].perp;

                float angle =
                    Mathf.Atan2(
                        spine[i].tan.y,
                        spine[i].tan.x
                    ) *
                    Mathf.Rad2Deg;

                // The overlap box is oriented along
                // the fish's local frame.
                if (Physics2D.OverlapBox(
                    spine[i].pos,
                    new Vector2(
                        0.05f,
                        desired
                    ),
                    angle,
                    wallLayers))
                {
                    float halfHeight =
                        desired * 0.5f;

                    RaycastHit2D left =
                        Physics2D.Raycast(
                            spine[i].pos,
                            perp,
                            halfHeight + 0.5f,
                            wallLayers
                        );

                    RaycastHit2D right =
                        Physics2D.Raycast(
                            spine[i].pos,
                            -perp,
                            halfHeight + 0.5f,
                            wallLayers
                        );

                    float leftDistance =
                        left.collider
                            ? left.distance
                            : halfHeight + 0.5f;

                    float rightDistance =
                        right.collider
                            ? right.distance
                            : halfHeight + 0.5f;

                    desired =
                        Mathf.Max(
                            leftDistance +
                            rightDistance,

                            spine[i].height *
                            minHeightScale
                        );
                }
            }

            _squeezedHeights[i] =
                Mathf.Lerp(
                    _squeezedHeights[i],
                    desired,
                    Time.deltaTime *
                    squeezeSmoothSpeed
                );

            spine[i].height =
                _squeezedHeights[i];
        }
    }

    // ================================================================
    // Body
    //
    // Every vertex is explicitly forced onto Z = 0.
    // ================================================================

    void AddBodyStrip(SpineRing[] spine)
    {
        int baseIndex =
            _verts.Count;

        for (int i = 0; i < spine.Length; i++)
        {
            float halfHeight =
                spine[i].height * 0.5f;

            Vector3 lower =
                spine[i].pos -
                spine[i].perp *
                halfHeight;

            Vector3 upper =
                spine[i].pos +
                spine[i].perp *
                halfHeight;

            // Absolute guarantee of a flat 2D mesh.
            lower.z = 0f;
            upper.z = 0f;

            _verts.Add(lower);
            _verts.Add(upper);

            if (generateUVs)
            {
                _uvs.Add(
                    new Vector2(
                        spine[i].arcU,
                        0f
                    )
                );

                _uvs.Add(
                    new Vector2(
                        spine[i].arcU,
                        1f
                    )
                );
            }
        }

        // Connect EVERY pair of adjacent rings.
        for (int i = 0;
             i < spine.Length - 1;
             i++)
        {
            int bl =
                baseIndex +
                i * 2;

            int br =
                bl + 1;

            int tl =
                baseIndex +
                (i + 1) * 2;

            int tr =
                tl + 1;

            _tris.Add(bl);
            _tris.Add(tl);
            _tris.Add(br);

            _tris.Add(br);
            _tris.Add(tl);
            _tris.Add(tr);
        }
    }

    // ================================================================
    // Fins
    //
    // Fins are also kept completely flat in XY.
    // ================================================================

    void AddFin(
        SpineRing ring,
        FishFin fin)
    {
        Vector3 outDir =
            fin.axis == FinAxis.Dorsal
                ? ring.perp
                : -ring.perp;

        float bodyEdge =
            ring.height * 0.5f;

        Vector3 root =
            ring.pos +
            outDir * bodyEdge;

        // Fin offset is interpreted in 2D.
        root +=
            new Vector3(
                fin.offset.x,
                fin.offset.y,
                0f
            );

        root.z = 0f;

        Vector3 forward =
            ring.tan;

        Vector3 v0 =
            root;

        Vector3 v1 =
            root +
            forward *
            fin.size.x;

        Vector3 v2 =
            root +
            outDir *
            fin.size.y;

        Vector3 v3 =
            root +
            forward *
            fin.size.x +
            outDir *
            fin.size.y;

        v0.z = 0f;
        v1.z = 0f;
        v2.z = 0f;
        v3.z = 0f;

        int b =
            _verts.Count;

        _verts.Add(v0);
        _verts.Add(v1);
        _verts.Add(v2);
        _verts.Add(v3);

        if (generateUVs)
        {
            _uvs.Add(
                new Vector2(0f, 0f)
            );

            _uvs.Add(
                new Vector2(1f, 0f)
            );

            _uvs.Add(
                new Vector2(0f, 1f)
            );

            _uvs.Add(
                new Vector2(1f, 1f)
            );
        }

        _tris.Add(b);
        _tris.Add(b + 2);
        _tris.Add(b + 1);

        _tris.Add(b + 1);
        _tris.Add(b + 2);
        _tris.Add(b + 3);
    }

    // ================================================================
    // Find render ring
    // ================================================================

    static SpineRing RingAtSegT(
        SpineRing[] spine,
        float segT)
    {
        int best = 0;

        float bestDistance =
            Mathf.Abs(
                spine[0].segT -
                segT
            );

        for (int i = 1;
             i < spine.Length;
             i++)
        {
            float distance =
                Mathf.Abs(
                    spine[i].segT -
                    segT
                );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return spine[best];
    }

    // ================================================================
    // Rebuild
    // ================================================================

    void RebuildMesh()
    {
        _verts.Clear();
        _uvs.Clear();
        _tris.Clear();

        SpineRing[] spine =
            BuildSpine();

        if (enableSqueezing)
            ApplySqueeze(spine);

        AddBodyStrip(spine);

        FishFin[] fins =
            _chain.profile != null
                ? _chain.profile.fins
                : null;

        if (fins != null &&
            _chain.profile.segments != null &&
            _chain.profile.segments.Length > 0)
        {
            int segmentCount =
                _chain.profile.segments.Length;

            foreach (FishFin fin in fins)
            {
                float segT =
                    fin.attachSegment /
                    (float)Mathf.Max(
                        segmentCount - 1,
                        1
                    );

                SpineRing ring =
                    RingAtSegT(
                        spine,
                        segT
                    );

                AddFin(
                    ring,
                    fin
                );

                if (fin.mirrorOtherSide)
                {
                    FishFin mirrored = fin;

                    mirrored.axis =
                        fin.axis ==
                        FinAxis.Dorsal
                            ? FinAxis.Ventral
                            : FinAxis.Dorsal;

                    AddFin(
                        ring,
                        mirrored
                    );
                }
            }
        }

        _mesh.Clear();

        _mesh.SetVertices(_verts);
        _mesh.SetTriangles(
            _tris,
            0
        );

        if (generateUVs)
        {
            _mesh.SetUVs(
                0,
                _uvs
            );
        }

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}

