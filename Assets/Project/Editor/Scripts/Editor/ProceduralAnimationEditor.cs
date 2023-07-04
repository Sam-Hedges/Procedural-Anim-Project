using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(ProceduralAnimation))]
[CustomEditor(typeof(ProceduralMultiLegController))]
public class ProceduralAnimationEditor : Editor
{
    private SerializedProperty _frequency;
    private SerializedProperty _dampingCoefficient;
    private SerializedProperty _initialResponse;

    private Vector2 graphSize = new(128, 200);
    private int iterations = 1500;
    
    private void OnEnable()
    {
        // Link the SerializedProperty to the variable 
        _frequency = serializedObject.FindProperty("frequency");
        _dampingCoefficient = serializedObject.FindProperty("dampingCoefficient");
        _initialResponse = serializedObject.FindProperty("initialResponse");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // fetch current values from the target
        serializedObject.Update();

        graphSize = EditorGUILayout.Vector2Field("graphSize", graphSize);
        iterations = EditorGUILayout.IntField("iterations", iterations);

        SecondOrderEditorGraph graph = new SecondOrderEditorGraph(0, 0, 2, 1, iterations);
        graph.DrawGraph(graphSize.x, graphSize.y, _frequency.floatValue, _dampingCoefficient.floatValue, _initialResponse.floatValue);
    }
}

[CustomEditor(typeof(SecondOrderDynamicsDemo))]
public class ProceduralAnimationEditor2 : Editor
{
    private SerializedProperty _frequency;
    private SerializedProperty _dampingCoefficient;
    private SerializedProperty _initialResponse;

    private Vector2 graphSize = new(128, 200);
    private int iterations = 1500;
    
    private void OnEnable()
    {
        // Link the SerializedProperty to the variable 
        _frequency = serializedObject.FindProperty("frequency");
        _dampingCoefficient = serializedObject.FindProperty("dampingCoefficient");
        _initialResponse = serializedObject.FindProperty("initialResponse");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // fetch current values from the target
        serializedObject.Update();

        graphSize = EditorGUILayout.Vector2Field("graphSize", graphSize);
        iterations = EditorGUILayout.IntField("iterations", iterations);

        SecondOrderEditorGraph graph = new SecondOrderEditorGraph(0, 0, 2, 1, iterations);
        graph.DrawGraph(graphSize.x, graphSize.y, _frequency.floatValue, _dampingCoefficient.floatValue, _initialResponse.floatValue);
    }
}