using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallBehavior))]
public class BallEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BallBehavior ball = (BallBehavior)target;

        GUILayout.Label("Ball hit paddle decresae factor");
        GUILayout.BeginHorizontal();
        ball.paddleHitDecreaseFactor.x = EditorGUILayout.Slider("X-axis", ball.paddleHitDecreaseFactor.x, 0f, 1f);
        ball.paddleHitDecreaseFactor.y = EditorGUILayout.Slider("Y-axis", ball.paddleHitDecreaseFactor.y, 0f, 1f);
        GUILayout.EndHorizontal();

        GUILayout.Label("Ball hit wall decresae factor");
        ball.wallHitDrag = EditorGUILayout.Slider(ball.wallHitDrag, 0f, 1f);
    }

}
