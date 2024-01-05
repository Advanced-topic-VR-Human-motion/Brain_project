using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FABRIKEffector))]
public class FABRIKEffectorEditor : Editor
{
    private Quaternion rotation;
    private bool editingAxis;

    public void OnEnable()
    {
        FABRIKEffector effector = target as FABRIKEffector;
        Transform transform = effector.transform;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty upAxisConstraintProperty = serializedObject.FindProperty("upAxisConstraint");
        SerializedProperty forwardAxisConstraintProperty = serializedObject.FindProperty("forwardAxisConstraint");

        rotation = transform.rotation * Quaternion.LookRotation(forwardAxisConstraintProperty.vector3Value, upAxisConstraintProperty.vector3Value);
        editingAxis = false;
    }

    public override void OnInspectorGUI()
    {
        bool modified = false;

        DrawDefaultInspector();

        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        SerializedProperty swingUpDownConstraintProperty = serializedObject.FindProperty("swingUpDownConstraint");
        SerializedProperty swingLeftRightConstraintProperty = serializedObject.FindProperty("swingLeftRightConstraint");
        SerializedProperty twistConstraintProperty = serializedObject.FindProperty("twistConstraint");


        // bool swingToggle = EditorGUILayout.Toggle("Swing Constraint", !float.IsNaN(swingConstraintProperty.floatValue));
        bool swingUpDownToggle = EditorGUILayout.Toggle("Swing Up/Down Constraint", !float.IsNaN(swingUpDownConstraintProperty.floatValue));
        float swingUpDownConstraint = float.NaN;


        if (swingUpDownToggle)
        {
            swingUpDownConstraint = Mathf.Clamp(EditorGUILayout.FloatField(swingUpDownConstraintProperty.floatValue), 0.0F, 360.0F);

            if (float.IsInfinity(swingUpDownConstraint) || float.IsNaN(swingUpDownConstraint))
            {
                swingUpDownConstraint = 0.0F;
            }

        }
        else
        {
            swingUpDownConstraint = float.NaN;
        }

        bool swingLeftRightToggle = EditorGUILayout.Toggle("Swing Left/Right Constraint", !float.IsNaN(swingLeftRightConstraintProperty.floatValue));
        float swingLeftRightConstraint = float.NaN;
        if(swingLeftRightToggle)
        {
            swingLeftRightConstraint = Mathf.Clamp(EditorGUILayout.FloatField(swingLeftRightConstraintProperty.floatValue), 0.0F, 360.0F);

            if (float.IsInfinity(swingLeftRightConstraint) || float.IsNaN(swingLeftRightConstraint))
            {
                swingLeftRightConstraint = 0.0F;
            }
        }
        else
        {
            swingLeftRightConstraint = float.NaN;
        }

        modified = modified || swingUpDownConstraint != swingUpDownConstraintProperty.floatValue || swingLeftRightConstraint != swingLeftRightConstraintProperty.floatValue;

        bool twistToggle = EditorGUILayout.Toggle("Twist Constraint", !float.IsNaN(twistConstraintProperty.floatValue));

        float twistConstraint;

        if (twistToggle)
        {
            twistConstraint = Mathf.Clamp(EditorGUILayout.FloatField(twistConstraintProperty.floatValue), 0.0F, 360.0F);

            if (float.IsInfinity(twistConstraint) || float.IsNaN(twistConstraint))
            {
                twistConstraint = 0.0F;
            }
        }
        else
        {
            twistConstraint = float.NaN;
        }

        modified = modified || twistConstraint != twistConstraintProperty.floatValue;

        if (modified)
        {
            swingUpDownConstraintProperty.floatValue = swingUpDownConstraint;
            swingLeftRightConstraintProperty.floatValue = swingLeftRightConstraint;
            twistConstraintProperty.floatValue = twistConstraint;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }

        if(GUILayout.Button("Edit Axis of Constraint (G: upwards, B: forwards)"))
        {
            Tools.current = Tool.None;

            editingAxis = true;
        }
    }

    public void OnSceneGUI()
    {
        if (editingAxis)
        {
            if (Tools.current != Tool.None)
            {
                editingAxis = false;
            }
            else
            {
                FABRIKEffector effector = target as FABRIKEffector;
                Transform transform = effector.transform;

                EditorGUI.BeginChangeCheck();

                Quaternion new_rotation = Handles.RotationHandle(rotation, transform.position);

                Handles.color = Handles.yAxisColor;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(new_rotation * Vector3.up), 0.5F, EventType.Repaint);

                Handles.color = Handles.zAxisColor;
                Handles.ArrowHandleCap(0, transform.position, new_rotation, 1.0F, EventType.Repaint);

                if (EditorGUI.EndChangeCheck())
                {
                    rotation = new_rotation;

                    SerializedObject serializedObject = new SerializedObject(target);
                    SerializedProperty upAxisConstraintProperty = serializedObject.FindProperty("upAxisConstraint");
                    SerializedProperty forwardAxisConstraintProperty = serializedObject.FindProperty("forwardAxisConstraint");

                    upAxisConstraintProperty.vector3Value = Quaternion.Inverse(transform.rotation) * new_rotation * Vector3.up;
                    forwardAxisConstraintProperty.vector3Value = Quaternion.Inverse(transform.rotation) * new_rotation * Vector3.forward;

                    serializedObject.ApplyModifiedProperties();

                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
