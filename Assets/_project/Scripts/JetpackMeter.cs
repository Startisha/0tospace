using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CanvasRenderer))]
public class JetpackMeter : Graphic
{
    [Header("Arc Settings")]
    public float radius = 80f;
    public float arcThickness = 8f;
    public int arcSegments = 60;

    [Header("Sweetspot")]
    public float sweetspotAngle = 15f;
    public Color sweetspotColor = new Color(0.11f, 0.62f, 0.46f, 0.8f);

    [Header("Needle")]
    public float needleLength = 70f;
    public Color needleColor = Color.white;

    private float _currentAngle = 90f;
    private float _targetAngle = 90f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        DrawArc(vh, radius, 0f, 180f, color, arcThickness);
        DrawArc(vh, radius, 90f - sweetspotAngle, 90f + sweetspotAngle, sweetspotColor, arcThickness);
        DrawNeedle(vh, _currentAngle, needleColor);
    }

    void DrawArc(VertexHelper vh, float r, float startDeg, float endDeg, Color c, float thickness)
    {
        int segments = Mathf.RoundToInt(arcSegments * (endDeg - startDeg) / 180f);
        segments = Mathf.Max(segments, 4);

        float inner = r - thickness * 0.5f;
        float outer = r + thickness * 0.5f;

        int startIndex = vh.currentVertCount;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float deg = Mathf.Lerp(startDeg, endDeg, t);
            float rad = deg * Mathf.Deg2Rad;

            float cosA = -Mathf.Cos(rad);
            float sinA = -Mathf.Sin(rad);

            UIVertex v = UIVertex.simpleVert;
            v.color = c;

            v.position = new Vector3(cosA * inner, sinA * inner);
            vh.AddVert(v);

            v.position = new Vector3(cosA * outer, sinA * outer);
            vh.AddVert(v);
        }

        for (int i = 0; i < segments; i++)
        {
            int idx = startIndex + i * 2;
            vh.AddTriangle(idx, idx + 1, idx + 3);
            vh.AddTriangle(idx, idx + 3, idx + 2);
        }
    }

    void DrawNeedle(VertexHelper vh, float angleDeg, Color c)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cosA = -Mathf.Cos(rad);
        float sinA = -Mathf.Sin(rad);

        Vector3 tip = new Vector3(cosA * needleLength, sinA * needleLength);
        Vector3 perpendicular = new Vector3(-sinA, cosA) * 1.5f;

        int startIndex = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = c;

        v.position = tip;
        vh.AddVert(v);

        v.position = -perpendicular;
        vh.AddVert(v);

        v.position = perpendicular;
        vh.AddVert(v);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        _targetAngle += mouseDelta.x * 0.5f;
#else
        _targetAngle += Input.GetAxis("Mouse X") * 0.5f;
#endif
        _targetAngle = Mathf.Clamp(_targetAngle, 0f, 180f);
        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * 10f);

        SetVerticesDirty();
    }
}