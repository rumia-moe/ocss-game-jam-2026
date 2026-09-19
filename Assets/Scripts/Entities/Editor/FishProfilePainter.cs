// Assets/Editor/FishProfilePainter.cs
// Place this file inside an Editor/ folder anywhere in your Assets directory.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FishProfilePainter : EditorWindow
{
    // ── State ─────────────────────────────────────────────────────────────────

    enum Tool { None, DrawBody, PlaceFin, Pan }

    Tool          _activeTool  = Tool.DrawBody;
    List<Vector2> _ctrlPts     = new();       // dorsal silhouette control points (canvas space)
    List<FinDef>  _fins        = new();       // placed fins

    struct FinDef
    {
        public float spineT;      // 0-1 along spine
        public float height;      // extrusion height
        public float length;      // length along spine
        public bool  isVentral;
    }

    // Fin drag state
    bool    _draggingFin    = false;
    int     _draggingFinIdx = -1;
    Vector2 _finDragStart;

    // Pan / zoom
    Vector2 _panOffset   = new(60f, 0f);
    float   _zoom        = 1f;
    Vector2 _lastMousePos;
    bool    _isPanning;

    // Preview mesh
    Mesh    _previewMesh;
    bool    _meshDirty = true;

    int     _draggingCtrlIdx = -1;
    Vector2 _ctrlDragOffset;
    bool    _draggingCtrl;

    // Export settings
    int    _segmentCount  = 8;
    string _savePath      = "Assets/FishProfiles";
    string _profileName   = "NewFishProfile";

    // Canvas region (set each repaint)
    Rect _canvasRect;

    // Visual constants
    const float CANVAS_H       = 500f;
    const float CTRL_RADIUS    = 6f;
    const float SPINE_SAMPLE   = 80;   // points sampled along spine for display

    // ── Menu entry ────────────────────────────────────────────────────────────

    [MenuItem("Window/IK/Fish Profile Painter")]
    static void Open() => GetWindow<FishProfilePainter>("Fish Painter");

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        _previewMesh = new Mesh { name = "FishPreview" };
        ResetCanvas();
    }

    void OnDisable()
    {
        if (_previewMesh != null) DestroyImmediate(_previewMesh);
    }

    void ResetCanvas()
    {
        _ctrlPts.Clear();
        _fins.Clear();
        _panOffset  = new Vector2(80f, CANVAS_H * 0.5f);
        _zoom       = 160f;   // 1 unit = 160px by default
        _meshDirty  = true;
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        DrawToolbar();

        // Reserve canvas area
        _canvasRect = GUILayoutUtility.GetRect(position.width, CANVAS_H);
        DrawCanvas(_canvasRect);

        GUILayout.Space(4);
        DrawSidebar();

        // Consume events over canvas
        HandleCanvasInput(_canvasRect);
    }

    // ── Toolbar ───────────────────────────────────────────────────────────────

    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUI.color = _activeTool == Tool.DrawBody ? Color.cyan : Color.white;
        if (GUILayout.Button("✏ Draw Body", EditorStyles.toolbarButton, GUILayout.Width(90)))
            _activeTool = Tool.DrawBody;

        GUI.color = _activeTool == Tool.PlaceFin ? Color.cyan : Color.white;
        if (GUILayout.Button("⊕ Place Fin", EditorStyles.toolbarButton, GUILayout.Width(90)))
            _activeTool = Tool.PlaceFin;

        GUI.color = _activeTool == Tool.Pan ? Color.cyan : Color.white;
        if (GUILayout.Button("✋ Pan", EditorStyles.toolbarButton, GUILayout.Width(60)))
            _activeTool = Tool.Pan;

        GUI.color = Color.white;
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(55)))
        {
            if (EditorUtility.DisplayDialog("Reset", "Clear all points and fins?", "Yes", "Cancel"))
                ResetCanvas();
        }

        EditorGUILayout.EndHorizontal();
    }

    // ── Canvas drawing ────────────────────────────────────────────────────────

    void DrawCanvas(Rect r)
    {
        // Background
        EditorGUI.DrawRect(r, new Color(0.12f, 0.14f, 0.18f));

        // Grid
        DrawGrid(r);

        if (_ctrlPts.Count == 0)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.5f, 0.55f, 0.6f) },
                fontSize  = 13
            };
            GUI.Label(r, "Click in this area with Draw Body selected\nto place silhouette control points  (head → tail)", style);
            return;
        }

        // Spine curve
        if (_ctrlPts.Count >= 2)
            DrawSpineCurve(r);

        // Control points
        foreach (var pt in _ctrlPts)
        {
            var sp = CanvasToScreen(pt, r);
            DrawDot(sp, CTRL_RADIUS, new Color(0.3f, 0.8f, 1f));
        }

        // Fins
        DrawFins(r);

        // Hover hint
        if (_activeTool == Tool.DrawBody)
        {
            var mp = CanvasFromScreen(Event.current.mousePosition, r);
            if (r.Contains(Event.current.mousePosition))
            {
                var sp = CanvasToScreen(mp, r);
                DrawDot(sp, CTRL_RADIUS * 0.6f, new Color(1f, 1f, 1f, 0.25f));
            }
        }
    }

    void DrawGrid(Rect r)
    {
        Handles.color = new Color(1f, 1f, 1f, 0.04f);
        float step = _zoom * 0.25f;
        if (step < 8f) step *= 4f;

        // vertical lines
        float startX = r.x + Mathf.Repeat(_panOffset.x, step);
        for (float x = startX; x < r.xMax; x += step)
            Handles.DrawLine(new Vector3(x, r.y), new Vector3(x, r.yMax));

        // horizontal lines
        float startY = r.y + Mathf.Repeat(_panOffset.y, step);
        for (float y = startY; y < r.yMax; y += step)
            Handles.DrawLine(new Vector3(r.x, y), new Vector3(r.xMax, y));

        // Spine axis
        Handles.color = new Color(1f, 1f, 1f, 0.10f);
        float axisY = r.y + _panOffset.y;
        Handles.DrawLine(new Vector3(r.x, axisY), new Vector3(r.xMax, axisY));
    }

    void DrawSpineCurve(Rect r)
    {
        // Sample the spline and draw dorsal edge, ventral mirror, and body fill
        var dorsalPts  = SampleSpline(_ctrlPts, (int)SPINE_SAMPLE);
        var ventralPts = new List<Vector2>();
        foreach (var p in dorsalPts)
            ventralPts.Add(new Vector2(p.x, -p.y));  // mirror Y

        // Body fill (polygon approximation)
        var fillPts = new List<Vector3>();
        foreach (var p in dorsalPts)  fillPts.Add(CanvasToScreen(p, r));
        for (int i = ventralPts.Count - 1; i >= 0; i--) fillPts.Add(CanvasToScreen(ventralPts[i], r));

        Handles.color = new Color(0.2f, 0.45f, 0.65f, 0.35f);
        Handles.DrawAAConvexPolygon(fillPts.ToArray());

        // Dorsal edge
        Handles.color = new Color(0.35f, 0.75f, 1f, 0.9f);
        for (int i = 0; i < dorsalPts.Count - 1; i++)
            Handles.DrawAAPolyLine(2.5f, CanvasToScreen(dorsalPts[i], r), CanvasToScreen(dorsalPts[i + 1], r));

        // Ventral edge (mirrored)
        Handles.color = new Color(0.35f, 0.75f, 1f, 0.55f);
        for (int i = 0; i < ventralPts.Count - 1; i++)
            Handles.DrawAAPolyLine(1.5f, CanvasToScreen(ventralPts[i], r), CanvasToScreen(ventralPts[i + 1], r));

        // Spine
        Handles.color = new Color(1f, 1f, 1f, 0.15f);
        var spinePts = new List<Vector3>();
        foreach (var p in dorsalPts) spinePts.Add(CanvasToScreen(new Vector2(p.x, 0f), r));
        Handles.DrawAAPolyLine(1f, spinePts.ToArray());
    }

    void DrawFins(Rect r)
    {
        if (_ctrlPts.Count < 2) return;
        var spine = SampleSpline(_ctrlPts, (int)SPINE_SAMPLE);

        for (int fi = 0; fi < _fins.Count; fi++)
        {
            var fin  = _fins[fi];
            int idx  = Mathf.Clamp(Mathf.RoundToInt(fin.spineT * (spine.Count - 1)), 0, spine.Count - 1);
            var root = spine[idx];

            float ySign = fin.isVentral ? -1f : 1f;
            float bodyH = root.y; // dorsal edge Y = half height

            Vector2 finRoot = new(root.x, ySign * bodyH);
            Vector2 finTip  = new(root.x + fin.length, ySign * (bodyH + fin.height));
            Vector2 finBack = new(root.x,               ySign * (bodyH + fin.height * 0.1f));

            // Fin triangle
            var pts = new Vector3[]
            {
                CanvasToScreen(finRoot, r),
                CanvasToScreen(new Vector2(root.x + fin.length * 0.85f, ySign * (bodyH + fin.height)), r),
                CanvasToScreen(finBack, r)
            };

            Handles.color = new Color(0.2f, 0.65f, 0.4f, 0.45f);
            Handles.DrawAAConvexPolygon(pts);
            Handles.color = new Color(0.3f, 0.9f, 0.55f, 0.9f);
            Handles.DrawAAPolyLine(2f, pts[0], pts[1], pts[2], pts[0]);

            // Drag handle at tip
            var tipScreen = CanvasToScreen(new Vector2(root.x + fin.length * 0.85f, ySign * (bodyH + fin.height)), r);
            DrawDot(tipScreen, CTRL_RADIUS, new Color(0.3f, 1f, 0.5f));

            // Label
            var labelPos = CanvasToScreen(new Vector2(root.x, ySign * (bodyH + fin.height + 0.05f)), r);
            var labelRect = new Rect(labelPos.x - 30f, labelPos.y - 10f, 60f, 18f);
            GUI.Label(labelRect, fin.isVentral ? "ventral" : "dorsal",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.4f, 1f, 0.6f) }, alignment = TextAnchor.MiddleCenter });
        }
    }

    void DrawDot(Vector2 pos, float radius, Color col)
    {
        Handles.color = col;
        Handles.DrawSolidDisc(pos, Vector3.forward, radius);
        Handles.color = Color.white;
        Handles.DrawWireDisc(pos, Vector3.forward, radius);
    }


    void HandleCanvasInput(Rect r)
    {
        var e = Event.current;
        if (!r.Contains(e.mousePosition) && !_isPanning && !_draggingFin) return;

        switch (e.type)
        {
            case EventType.MouseDown:
                
                if (e.button == 2 || (e.button == 0 && _activeTool == Tool.Pan))
                {
                    _isPanning    = true;
                    _lastMousePos = e.mousePosition;
                    e.Use();
                }
                else if (e.button == 0 && _activeTool == Tool.DrawBody)
                {
                    int hit = HitTestCtrl(e.mousePosition, r);
                    if (e.shift && hit >= 0)
                    {
                        _ctrlPts.RemoveAt(hit);
                        _meshDirty = true;
                    }
                    else if (hit >= 0)
                    {
                        // Start dragging the existing point
                        _draggingCtrl    = true;
                        _draggingCtrlIdx = hit;
                    }
                    else
                    {
                        var cp = CanvasFromScreen(e.mousePosition, r);
                        cp.y = Mathf.Abs(cp.y);
                        InsertCtrlPt(cp);
                        _meshDirty = true;
                    }
                    e.Use(); Repaint();
                }
                else if (e.button == 0 && _activeTool == Tool.PlaceFin)
                {
                    // Check if clicking existing fin handle
                    int fi = HitTestFin(e.mousePosition, r);
                    if (fi >= 0)
                    {
                        _draggingFin    = true;
                        _draggingFinIdx = fi;
                        _finDragStart   = e.mousePosition;
                    }
                    else
                    {
                        // Place new fin
                        var cp    = CanvasFromScreen(e.mousePosition, r);
                        float t   = SpineTFromCanvasX(cp.x);
                        bool vent = cp.y < 0f;
                        _fins.Add(new FinDef { spineT = t, height = 0.15f, length = 0.12f, isVentral = vent });
                        _draggingFin    = true;
                        _draggingFinIdx = _fins.Count - 1;
                        _finDragStart   = e.mousePosition;
                    }
                    e.Use(); Repaint();
                }
                else if (e.button == 1)
                {
                    // Right-click context menu
                    ShowContextMenu(e.mousePosition, r);
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (_isPanning)
                {
                    _panOffset += e.mousePosition - _lastMousePos;
                    _lastMousePos = e.mousePosition;
                    e.Use(); Repaint();
                }else if (_draggingCtrl && _draggingCtrlIdx >= 0)
                {
                    var cp = CanvasFromScreen(e.mousePosition, r);
                    cp.y = Mathf.Abs(cp.y);   // keep dorsal-only constraint
                    _ctrlPts[_draggingCtrlIdx] = cp;
                    _meshDirty = true;
                    e.Use(); Repaint();
                }
                else if (_draggingFin && _draggingFinIdx >= 0)
                {
                    var delta  = e.mousePosition - _finDragStart;
                    var fin    = _fins[_draggingFinIdx];
                    fin.height = Mathf.Max(0.02f, fin.height + delta.y / -_zoom);
                    fin.length = Mathf.Max(0.02f, fin.length + delta.x /  _zoom);
                    _fins[_draggingFinIdx] = fin;
                    _finDragStart = e.mousePosition;
                    _meshDirty = true;
                    e.Use(); Repaint();
                }
                break;

            case EventType.MouseUp:
                _isPanning       = false;
                _draggingCtrl    = false;
                _draggingCtrlIdx = -1;
                _draggingFin     = false;
                _draggingFinIdx  = -1;
                break;

            case EventType.ScrollWheel:
                float zoomDelta = -e.delta.y * 0.05f;
                float oldZoom   = _zoom;
                _zoom = Mathf.Clamp(_zoom * (1f + zoomDelta), 40f, 800f);
                // Zoom toward mouse
                var mp = e.mousePosition - new Vector2(r.x, r.y);
                _panOffset = mp - (_zoom / oldZoom) * (mp - _panOffset);
                e.Use(); Repaint();
                break;
        }
    }

    void ShowContextMenu(Vector2 mousePos, Rect r)
    {
        var menu = new GenericMenu();
        int hit  = HitTestCtrl(mousePos, r);
        if (hit >= 0)
            menu.AddItem(new GUIContent("Remove Point"), false, () => { _ctrlPts.RemoveAt(hit); _meshDirty = true; Repaint(); });

        int fi = HitTestFin(mousePos, r);
        if (fi >= 0)
            menu.AddItem(new GUIContent("Remove Fin"), false, () => { _fins.RemoveAt(fi); Repaint(); });

        if (hit < 0 && fi < 0)
            menu.AddDisabledItem(new GUIContent("Right-click a point or fin to remove it"));

        menu.ShowAsContext();
    }


    void DrawSidebar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Stats
        EditorGUILayout.LabelField("Profile", EditorStyles.boldLabel);
        _segmentCount = EditorGUILayout.IntSlider("Segments", _segmentCount, 3, 20);
        _profileName  = EditorGUILayout.TextField("Name", _profileName);
        _savePath     = EditorGUILayout.TextField("Save folder", _savePath);

        EditorGUILayout.Space(4);

        // Fins list (editable)
        if (_fins.Count > 0)
        {
            EditorGUILayout.LabelField("Fins", EditorStyles.boldLabel);
            for (int i = 0; i < _fins.Count; i++)
            {
                var fin = _fins[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(fin.isVentral ? "▼ Ventral" : "▲ Dorsal", GUILayout.Width(70));
                fin.spineT  = EditorGUILayout.Slider(fin.spineT, 0f, 1f, GUILayout.Width(120));
                fin.height  = EditorGUILayout.FloatField(fin.height, GUILayout.Width(50));
                fin.length  = EditorGUILayout.FloatField(fin.length, GUILayout.Width(50));
                if (GUILayout.Button("✕", GUILayout.Width(22))) { _fins.RemoveAt(i); Repaint(); break; }
                _fins[i] = fin;
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(6);

        // Action buttons
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = _ctrlPts.Count >= 2;
        if (GUILayout.Button("💾  Save Profile", GUILayout.Height(32)))
        {
            try { SaveProfile(); }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Save failed", ex.Message, "OK");
            }
        }

        GUI.enabled = true;
        if (GUILayout.Button("📋  Load Profile", GUILayout.Height(32)))
            LoadProfile();

        EditorGUILayout.EndHorizontal();

        // Tips
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Draw Body: click to place dorsal points (head→tail). Shift+click a point to remove.\n" +
            "Place Fin: click above spine for dorsal fin, below for ventral. Drag tip to resize.\n" +
            "Pan: middle-mouse or Pan tool. Scroll to zoom.",
            MessageType.None);

        EditorGUILayout.EndVertical();
    }

    // ── Spline helpers ────────────────────────────────────────────────────────

    // Catmull-Rom position
    static Vector2 CRPos(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t*t, t3 = t2*t;
        return 0.5f * (2f*p1 + (-p0+p2)*t + (2f*p0-5f*p1+4f*p2-p3)*t2 + (-p0+3f*p1-3f*p2+p3)*t3);
    }

    List<Vector2> SampleSpline(List<Vector2> pts, int samples)
    {
        var result = new List<Vector2>();
        if (pts.Count < 2) return result;
        int segs = pts.Count - 1;
        for (int s = 0; s <= samples; s++)
        {
            float globalT = s / (float)samples * segs;
            int   seg     = Mathf.Min((int)globalT, segs - 1);
            float t       = globalT - seg;
            int   i0 = Mathf.Max(seg - 1, 0);
            int   i1 = seg;
            int   i2 = seg + 1;
            int   i3 = Mathf.Min(seg + 2, pts.Count - 1);
            result.Add(CRPos(pts[i0], pts[i1], pts[i2], pts[i3], t));
        }
        return result;
    }

    // Insert a new control point in X-sorted order
    void InsertCtrlPt(Vector2 cp)
    {
        int insertAt = _ctrlPts.Count;
        for (int i = 0; i < _ctrlPts.Count; i++)
        {
            if (_ctrlPts[i].x > cp.x) { insertAt = i; break; }
        }
        _ctrlPts.Insert(insertAt, cp);
    }

    float SpineTFromCanvasX(float cx)
    {
        if (_ctrlPts.Count < 2) return 0f;
        float minX = _ctrlPts[0].x, maxX = _ctrlPts[_ctrlPts.Count - 1].x;
        return Mathf.InverseLerp(minX, maxX, cx);
    }

    // ── Coordinate conversion ─────────────────────────────────────────────────

    // Canvas space: X = along fish (world units), Y = height above spine (world units)
    // Screen space: pixel position within the EditorWindow

    Vector2 CanvasToScreen(Vector2 cp, Rect r)
    {
        return new Vector2(
            r.x + _panOffset.x + cp.x * _zoom,
            r.y + _panOffset.y - cp.y * _zoom   // Y flipped (up = positive canvas)
        );
    }

    Vector2 CanvasFromScreen(Vector2 sp, Rect r)
    {
        return new Vector2(
            (sp.x - r.x - _panOffset.x) / _zoom,
            (r.y  + _panOffset.y - sp.y) / _zoom
        );
    }


    int HitTestCtrl(Vector2 screenPos, Rect r)
    {
        for (int i = 0; i < _ctrlPts.Count; i++)
        {
            var sp = CanvasToScreen(_ctrlPts[i], r);
            if (Vector2.Distance(sp, screenPos) < CTRL_RADIUS + 4f)
                return i;
        }
        return -1;
    }

    int HitTestFin(Vector2 screenPos, Rect r)
    {
        if (_ctrlPts.Count < 2) return -1;
        var spine = SampleSpline(_ctrlPts, (int)SPINE_SAMPLE);

        for (int fi = 0; fi < _fins.Count; fi++)
        {
            var fin  = _fins[fi];
            int idx  = Mathf.Clamp(Mathf.RoundToInt(fin.spineT * (spine.Count - 1)), 0, spine.Count - 1);
            var root = spine[idx];
            float ySign  = fin.isVentral ? -1f : 1f;
            float bodyH  = root.y;
            var tipCanvas = new Vector2(root.x + fin.length * 0.85f, ySign * (bodyH + fin.height));
            var tipScreen = CanvasToScreen(tipCanvas, r);
            if (Vector2.Distance(tipScreen, screenPos) < CTRL_RADIUS + 4f)
                return fi;
        }
        return -1;
    }


    void SaveProfile()
    {
        
        if (_ctrlPts.Count < 2) { EditorUtility.DisplayDialog("Error", "Need at least 2 body points.", "OK"); return; }

        string safeName = string.Concat(_profileName.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeName)) { EditorUtility.DisplayDialog("Error", "Profile name is empty or all invalid characters.", "OK"); return; }

        if (!Directory.Exists(_savePath))
            Directory.CreateDirectory(_savePath);

        string assetPath = $"{_savePath}/{safeName}.asset";

        // Sample the spline into segments
        int    n      = _segmentCount;
        var    spine  = SampleSpline(_ctrlPts, n);
        var    segs   = new FishProfile.SegmentData[n];

        float totalX  = spine[spine.Count - 1].x - spine[0].x;

        for (int i = 0; i < n; i++)
        {
            float t    = i / (float)(n - 1);
            int   idxA = i;
            int   idxB = Mathf.Min(i + 1, spine.Count - 1);

            float segLen = Vector2.Distance(spine[idxA], spine[idxB]);
            float height = spine[idxA].y * 2f;  // full height = dorsal * 2

            segs[i] = new FishProfile.SegmentData
            {
                length      = Mathf.Max(segLen, 0.01f),
                height      = Mathf.Max(height, 0.01f),
                lateralBend = Mathf.Lerp(8f, 45f, t)   // sensible default bend ramp
            };
        }

        // Build fins
        var fishFins = new FishFin[_fins.Count];
        for (int i = 0; i < _fins.Count; i++)
        {
            var fd  = _fins[i];
            int seg = Mathf.Clamp(Mathf.RoundToInt(fd.spineT * (n - 1)), 0, n - 1);
            fishFins[i] = new FishFin
            {
                attachSegment   = seg,
                offset          = Vector2.zero,
                size            = new Vector2(fd.length, fd.height),
                mirrorOtherSide = false,
                axis            = fd.isVentral ? FinAxis.Ventral : FinAxis.Dorsal
            };
        }

        // Create asset
        if (!Directory.Exists(_savePath))
            Directory.CreateDirectory(_savePath);

        var profile            = ScriptableObject.CreateInstance<FishProfile>();
        profile.segments       = segs;
        profile.fins           = fishFins;
        profile.followSpeed    = 0.15f;
        profile.solverIterations = 5;
        profile.controlPoints = _ctrlPts.ToArray();

        AssetDatabase.CreateAsset(profile, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Saved", $"Profile saved to\n{assetPath}", "OK");
        Selection.activeObject = profile;
    }

    void LoadProfile()
    {
        string path = EditorUtility.OpenFilePanel("Load Fish Profile", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        // Convert absolute path to relative
        path = "Assets" + path.Substring(Application.dataPath.Length);
        var profile = AssetDatabase.LoadAssetAtPath<FishProfile>(path);
        if (profile == null) { EditorUtility.DisplayDialog("Error", "Could not load FishProfile at that path.", "OK"); return; }

        _ctrlPts.Clear();
        _fins.Clear();

        if (profile.controlPoints != null && profile.controlPoints.Length >= 2)
        {
            _ctrlPts.AddRange(profile.controlPoints);
        }
        else
        {
            // Backwards compatibility with old profiles
            float cursor = 0f;

            for (int i = 0; i < profile.segments.Length; i++)
            {
                var seg = profile.segments[i];

                _ctrlPts.Add(new Vector2(
                    cursor,
                    seg.height * 0.5f
                ));

                cursor += seg.length;
            }
        }

        // Reconstruct fins
        if (profile.fins != null)
        {
            int segCount = profile.segments.Length;
            foreach (var fin in profile.fins)
            {
                _fins.Add(new FinDef
                {
                    spineT    = fin.attachSegment / (float)Mathf.Max(segCount - 1, 1),
                    height    = fin.size.y,
                    length    = fin.size.x,
                    isVentral = fin.axis == FinAxis.Ventral
                });
            }
        }

        _profileName = profile.name;
        _meshDirty   = true;
        Repaint();
    }
}