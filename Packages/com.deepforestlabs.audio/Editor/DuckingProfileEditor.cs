#nullable enable
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Audio.Editor
{
    [CustomEditor(typeof(DuckingProfile))]
    public sealed class DuckingProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _targetGroup = null!;
        private SerializedProperty _volumeReductionDb = null!;
        private SerializedProperty _attackTime = null!;
        private SerializedProperty _releaseTime = null!;

        private void OnEnable()
        {
            _targetGroup = serializedObject.FindProperty("_targetGroup");
            _volumeReductionDb = serializedObject.FindProperty("_volumeReductionDb");
            _attackTime = serializedObject.FindProperty("_attackTime");
            _releaseTime = serializedObject.FindProperty("_releaseTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetGroup);
            EditorGUILayout.PropertyField(_volumeReductionDb);
            EditorGUILayout.PropertyField(_attackTime);
            EditorGUILayout.PropertyField(_releaseTime);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ducking Curve Preview", EditorStyles.boldLabel);
            DrawCurvePreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCurvePreview()
        {
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100));
            if (Event.current.type != EventType.Repaint)
                return;

            float attack = _attackTime.floatValue;
            float release = _releaseTime.floatValue;
            float reductionDb = _volumeReductionDb.floatValue;

            float totalTime = attack + 0.5f + release;
            if (totalTime <= 0f) totalTime = 1f;

            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            float zeroY = rect.y + 10f;
            Handles.DrawLine(new Vector3(rect.x, zeroY), new Vector3(rect.xMax, zeroY));

            float minY = rect.yMax - 10f;
            Handles.DrawLine(new Vector3(rect.x, minY), new Vector3(rect.xMax, minY));

            float usableHeight = minY - zeroY;
            float reductionNorm = Mathf.Clamp01(Mathf.Abs(reductionDb) / 80f);

            float attackEnd = rect.x + (attack / totalTime) * rect.width;
            float sustainEnd = rect.x + ((attack + 0.5f) / totalTime) * rect.width;

            Handles.color = Color.cyan;

            Handles.DrawLine(
                new Vector3(rect.x, zeroY),
                new Vector3(attackEnd, zeroY + usableHeight * reductionNorm));

            Handles.DrawLine(
                new Vector3(attackEnd, zeroY + usableHeight * reductionNorm),
                new Vector3(sustainEnd, zeroY + usableHeight * reductionNorm));

            Handles.DrawLine(
                new Vector3(sustainEnd, zeroY + usableHeight * reductionNorm),
                new Vector3(rect.xMax, zeroY));

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
            GUI.Label(new Rect(rect.x + 2, zeroY - 14, 60, 14), "0 dB", labelStyle);
            GUI.Label(new Rect(rect.x + 2, minY, 60, 14), $"{reductionDb:F0} dB", labelStyle);
            GUI.Label(new Rect(attackEnd - 10, rect.yMax - 14, 60, 14), $"{attack:F2}s", labelStyle);
            GUI.Label(new Rect(sustainEnd - 10, rect.yMax - 14, 60, 14), $"{release:F2}s", labelStyle);
        }
    }
}
#nullable disable
