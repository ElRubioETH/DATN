using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;

        // Draw detection radius
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.radius);

        // Draw chase radius if in chase state
        if (fov.currentState == FieldOfView.EnemyState.Chase)
        {
            Handles.color = Color.red;
            Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.radius * fov.chaseRadiusMultiplier);
        }

        // Draw FOV angle
        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.radius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.radius);

        // Draw line to player if detected
        if (fov.currentState == FieldOfView.EnemyState.Detect || fov.currentState == FieldOfView.EnemyState.Chase)
        {
            Handles.color = fov.currentState == FieldOfView.EnemyState.Chase ? Color.red : Color.green;
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);

            // Draw detection progress - using the public properties now
            if (fov.currentState == FieldOfView.EnemyState.Detect)
            {
                Handles.Label(fov.transform.position + Vector3.up * 2,
                    $"Detecting: {(fov.DetectionProgress / fov.DetectionTime * 100):0}%");
            }
        }

        // Rest of your patrol path drawing code remains the same...
        if (fov.patrolPoints != null && fov.patrolPoints.Count > 0)
        {
            Handles.color = Color.blue;
            for (int i = 0; i < fov.patrolPoints.Count; i++)
            {
                if (fov.patrolPoints[i] != null)
                {
                    Handles.DrawLine(
                        fov.transform.position,
                        fov.patrolPoints[i].position);

                    // ... rest of patrol drawing code
                }
            }
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}