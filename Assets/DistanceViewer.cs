using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DistanceViewer : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    [Header("Arrow Settings")]
    public Color arrowColor = Color.yellow;
    public ArrowType arrowType = ArrowType.Default;
    public float arrowHeadLength = 0.2f;
    public float arrowHeadAngle = 20.0f;

    void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;

        Vector3 direction = pointB.position - pointA.position;
        float distance = direction.magnitude;

        // DrawArrow로 A에서 B 방향 화살표
        DrawArrow.ForGizmo(pointA.position, direction, arrowColor, false, arrowHeadLength, arrowHeadAngle);

#if UNITY_EDITOR
        // Scene View 중간에 거리 표시
        Handles.Label(pointA.position + direction * 0.5f, $"Distance: {distance:F2}");
#endif
    }

    // 런타임 중에도 화살표 보고 싶으면 이 메서드 호출
    public void DrawDebugArrow()
    {
        if (pointA == null || pointB == null) return;

        Vector3 direction = pointB.position - pointA.position;
        DrawArrow.ForDebug(pointA.position, direction, 0f, arrowColor, arrowType, arrowHeadLength, arrowHeadAngle);
    }
}
