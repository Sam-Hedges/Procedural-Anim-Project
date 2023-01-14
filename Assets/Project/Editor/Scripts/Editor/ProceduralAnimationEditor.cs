using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralAnimation))]
public class ProceduralAnimationEditor : Editor
{
    private SerializedProperty _frequency;
    private SerializedProperty _dampingCoefficient;
    private SerializedProperty _initialResponse;
    
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
        
        SecondOrderDynamics secondOrderDynamics = new SecondOrderDynamics(_frequency.floatValue, _dampingCoefficient.floatValue, _initialResponse.floatValue, Vector3.zero);

        EditorGraph graph = new EditorGraph(0, 0, 2, 2, "Step Response", 1000);
        graph.GridLinesX = 0.5f;
        graph.GridLinesY = 0.5f;
        graph.AddFunction(x => x, Color.cyan);
        graph.AddLineY(0, Color.white);
        graph.AddLineY(1, Color.green);
        graph.AddLineX(0, Color.white);
        graph.AddClickEvent((x, y) => Debug.LogFormat("You clicked at {0};{1}.", x, y));
        graph.DrawSOD(128, 128, secondOrderDynamics);
    }
}
