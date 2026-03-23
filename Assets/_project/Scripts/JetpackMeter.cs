using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasRenderer))]
public class JetpackMeter : Graphic
{
    [Header("Zones")]
    public float innerBrownRadius = 65f;
    public float outerBrownRadius = 200f;
    public float lineThickness = 3f;
    public float thinLineThickness = 1.5f;
    public int segments = 128;

    [Header("Colors")]
    public Color innerBrownColor = new Color(0.76f, 0.6f, 0.42f, 0.8f);
    public Color redColor = new Color(0.9f, 0.2f, 0.2f, 0.8f);
    public Color outerBrownColor = new Color(0.76f, 0.6f, 0.42f, 0.4f);
    public Color thinLineColor = new Color(1f, 1f, 1f, 0.3f);
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.5f);
    public Color cursorColor = new Color(1f, 1f, 1f, 1f);

    [Header("Cursor")]
    public float cursorSize = 8f;

    private Vector2 _cursorPos;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float range = outerBrownRadius - innerBrownRadius;
        float r25 = innerBrownRadius + range * 0.25f;
        float r50 = innerBrownRadius + range * 0.5f;
        float r75 = innerBrownRadius + range * 0.75f;

        DrawCircle(vh, Vector2.zero, innerBrownRadius, innerBrownColor, lineThickness);
        DrawCircle(vh, Vector2.zero, r25, thinLineColor, thinLineThickness);
        DrawCircle(vh, Vector2.zero, r50, redColor, lineThickness);
        DrawCircle(vh, Vector2.zero, r75, thinLineColor, thinLineThickness);
        DrawCircle(vh, Vector2.zero, outerBrownRadius, outerBrownColor, lineThickness);
        DrawCrosshair(vh, Vector2.zero, outerBrownRadius, crosshairColor, lineThickness);
        DrawCursor(vh, _cursorPos, cursorSize, cursorColor);
    }

    void DrawCircle(VertexHelper vh, Vector2 center, float radius, Color c, float thickness)
    {
        int startIndex = vh.currentVertCount;
        float inner = radius - thickness * 0.5f;
        float outer = radius + thickness * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            UIVertex v = UIVertex.simpleVert;
            v.color = c;

            v.position = new Vector3(center.x + cos * inner, center.y + sin * inner);
            vh.AddVert(v);

            v.position = new Vector3(center.x + cos * outer, center.y + sin * outer);
            vh.AddVert(v);
        }

        for (int i = 0; i < segments; i++)
        {
            int idx = startIndex + i * 2;
            vh.AddTriangle(idx, idx + 1, idx + 3);
            vh.AddTriangle(idx, idx + 3, idx + 2);
        }
    }

    void DrawCrosshair(VertexHelper vh, Vector2 center, float size, Color c, float thickness)
    {
        DrawLine(vh, new Vector2(center.x - size, center.y), new Vector2(center.x + size, center.y), c, thickness);
        DrawLine(vh, new Vector2(center.x, center.y - size), new Vector2(center.x, center.y + size), c, thickness);
    }

    void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Color c, float thickness)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        int startIndex = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = c;

        v.position = new Vector3(start.x - perp.x, start.y - perp.y);
        vh.AddVert(v);
        v.position = new Vector3(start.x + perp.x, start.y + perp.y);
        vh.AddVert(v);
        v.position = new Vector3(end.x + perp.x, end.y + perp.y);
        vh.AddVert(v);
        v.position = new Vector3(end.x - perp.x, end.y - perp.y);
        vh.AddVert(v);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    void DrawCursor(VertexHelper vh, Vector2 pos, float size, Color c)
    {
        DrawLine(vh, new Vector2(pos.x - size, pos.y), new Vector2(pos.x + size, pos.y), c, 2f);
        DrawLine(vh, new Vector2(pos.x, pos.y - size), new Vector2(pos.x, pos.y + size), c, 2f);
    }

    void Update()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 cursorPos = Mouse.current.position.ReadValue();
        Vector2 offset = cursorPos - screenCenter;

        float scale = outerBrownRadius / (Screen.width * 0.15f);
        _cursorPos = offset * scale;

        SetVerticesDirty();
    }
}