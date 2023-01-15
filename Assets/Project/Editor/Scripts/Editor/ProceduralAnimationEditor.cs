using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralAnimation))]
public class ProceduralAnimationEditor : Editor
{
    private SerializedProperty _frequency;
    private SerializedProperty _dampingCoefficient;
    private SerializedProperty _initialResponse;
    private SerializedProperty _labelPosition;
    
    private float minx = 0;
    private float maxx = 2;
    private float miny = 0;
    private float maxy = 2;
    private Vector2 graphSize = new Vector2(128, 128);
    
    private void OnEnable()
    {
        // Link the SerializedProperty to the variable 
        _frequency = serializedObject.FindProperty("frequency");
        _dampingCoefficient = serializedObject.FindProperty("dampingCoefficient");
        _initialResponse = serializedObject.FindProperty("initialResponse");
        _labelPosition = serializedObject.FindProperty("labelPosition");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // fetch current values from the target
        serializedObject.Update();
        
        SecondOrderDynamics secondOrderDynamics = new SecondOrderDynamics(_frequency.floatValue, _dampingCoefficient.floatValue, _initialResponse.floatValue, Vector3.zero);
        
        
        minx = EditorGUILayout.Slider("minx",minx, -2, 2);
        GUILayout.Space(10f);
        maxx = EditorGUILayout.Slider("maxx",maxx, -2, 2);
        GUILayout.Space(10f);
        miny = EditorGUILayout.Slider("miny", miny, -2, 2);
        GUILayout.Space(10f);
        maxy = EditorGUILayout.Slider("maxy", maxy, -2, 2);
        GUILayout.Space(20f);
        graphSize = EditorGUILayout.Vector2Field("graphSize", graphSize);
        
        EditorGraph graph = new EditorGraph(minx, miny, maxx, maxy, "Step Response", 1000);
        graph.labelPosition = _labelPosition.vector2Value; 
        graph.GridLinesX = 0.5f;
        graph.GridLinesY = 0.5f;
        graph.AddFunction(x => x, Color.cyan);
        graph.AddLineY(0, Color.white);
        graph.AddLineY(1, Color.green);
        graph.AddLineX(0, Color.white);
        graph.AddClickEvent((x, y) => Debug.LogFormat("You clicked at {0};{1}.", x, y));
        graph.DrawSOD(graphSize.x, graphSize.y, secondOrderDynamics);
        Debug.Log(graphSize); 
    }
}
