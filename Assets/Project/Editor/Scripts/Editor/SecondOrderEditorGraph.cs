using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SecondOrderEditorGraph
{
    // Vertex buffers
    private Vector3[] _rectVertices = new Vector3[4];
    private Vector3[] _lineVertices = new Vector3[2];
    private Vector3[] _curveVertices;
    
    private float _minX, _maxX, _minY, _maxY;
    private Rect _graphRect;
    
    private float RangeX => _maxX - _minX;
    private float RangeY => _maxY - _minY;
    
    private int _graphIterations;
    private struct LineColorPair
    {
        public float position;
        public Color color;
    }
    
    public struct GraphColors
    {
        public Color background;
        public Color outline;
        public Color gridLine;
        public Color function;
        public Color customLine;
    }
    
    private GraphColors _graphColors;
    
    public SecondOrderEditorGraph(float minX, float minY, float maxX, float maxY, int iterations = 48)
    {
        if (minX >= maxX)
            throw new System.ArgumentException("Editor graph: minimum X value cannot be greater than maximum!", "minX");
        if (minY >= maxY)
            throw new System.ArgumentException("Editor graph: minimum Y value cannot be greater than maximum!", "minY");

        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
        
        _graphIterations = iterations;
		
        _curveVertices = new Vector3[_graphIterations];
        
        // Default graph colors
        _graphColors = new GraphColors
        {
            background = new Color(0.15f, 0.15f, 0.15f, 0f),
            outline = new Color(0f, 0f, 0f, 1f),
            gridLine = Color.gray,
            function = Color.cyan,
            customLine = Color.white
        };
    }
    
    public void DrawGraph(float width, float height, float frequency, float dampingCoefficient, float initialResponse)
	{
		// Initialize the graph rect parameters
		using (new GUILayout.HorizontalScope())
		{
			// List of GUI Layout Options
			List<GUILayoutOption> options = new List<GUILayoutOption>();
			options.Add(GUILayout.ExpandWidth(true));
			options.Add(GUILayout.ExpandHeight(true));
		
			// Styling parameters of the container graph rect
			GUIStyle style = new GUIStyle();
			style.margin = new RectOffset(10, 10, 20, 20);
		
			// Reserve layout space for the graph rect based on parameters
			_graphRect = GUILayoutUtility.GetRect(width, height, style, options.ToArray());
		}

		// Only continue if we're repainting the graph
		if (Event.current.type != EventType.Repaint)
			return;

		// Check if the vertex buffer for the curve is equal to the amount of points 
		if (_curveVertices.Length != _graphIterations) {
			_curveVertices = new Vector3[_graphIterations];
		}
		
		
		// Evaluate the Second Order Dynamics Step Response
		int vertexCount = 0;
		Vector2[] graphPosition = EvaluateSecondOrderDynamics(vertexCount, frequency, dampingCoefficient, initialResponse);
		
		// Add the evaluated points to the vertex buffer	
		foreach(Vector2 position in graphPosition)
		{
			_curveVertices[vertexCount++] = UnitToGraphUnclamped(position.x, position.y);
		}

		// Draw the Background of the Graph
		DrawRect(_minX, _minY, _maxX, _maxY, _graphColors.background, _graphColors.outline);

		// Vertical lines
		DrawLine(0, _minY, 0, _maxY, _graphColors.customLine, 2);
		
		// Horizontal lines
		DrawLine(_minX, 0, _maxX, 0, _graphColors.customLine, 2);
		DrawLine(_minX, 1, _maxX, 1, Color.green, 2);
		
		// Draw the labels for the X axis
		Vector3 labelPosition = UnitToGraphUnclamped(_minX, 0) - new Vector3(15f, 0f, 0f);
		Handles.Label(labelPosition, "0");
		labelPosition = UnitToGraphUnclamped(_minX, 1) - new Vector3(15f, 0f, 0f);
		Handles.Label(labelPosition, "1");

		// Draw the Curve
		if (vertexCount > 1)
		{
			Handles.color = _graphColors.function;
			Handles.DrawAAPolyLine(2.0f, vertexCount, _curveVertices);
		}
	}
    
    private Vector2[] EvaluateSecondOrderDynamics(int vertexCount, float frequency, float dampingCoefficient, float initialResponse)
	{
		Vector2[] graphPositions = new Vector2[_graphIterations];

		for(int i = 0; i < 2; i++)
		{
			vertexCount = 0;
			SecondOrderDynamics dynamics = new SecondOrderDynamics(frequency, dampingCoefficient, initialResponse, Vector3.zero);

			while (vertexCount < _graphIterations)
			{
				// Evaluate the dynamics
				Vector3 targetPosition = new Vector3(0f, 1f, 0f);
				float y = dynamics.UpdatePosition(Time.fixedDeltaTime * 0.1f, targetPosition, Vector3.zero).y;
				float x = (RangeX / _graphIterations) * vertexCount;
			
				// Adjust the graph scale so that the curves fit within the graph
				if (i == 0)
				{
					if (y < _minY)
					{
						_minY = y;
					}
					else if (y > _maxY)
					{
						_maxY = y;
					}
				}
				
				// Add the evaluated point to the graph position array
				if(i == 1) { graphPositions[vertexCount] = new Vector2(x, y); }

				vertexCount++;
			}
		}
		
		return graphPositions;
	}
    
    private Vector3 UnitToGraph(float x, float y)
    {
	    x = Mathf.Lerp(_graphRect.x, _graphRect.xMax, (x - _minX) / RangeX);
	    y = Mathf.Lerp(_graphRect.yMax, _graphRect.y, (y - _minY) / RangeY);

	    return new Vector3(x, y, 0);
    }
	
    private Vector3 UnitToGraphUnclamped(float x, float y)
    {
	    x = Mathf.LerpUnclamped(_graphRect.x, _graphRect.xMax, (x - _minX) / RangeX);
	    y = Mathf.LerpUnclamped(_graphRect.yMax, _graphRect.y, (y - _minY) / RangeY);

	    return new Vector3(x, y, 0);
    }
    
    void DrawLine(float x1, float y1, float x2, float y2, Color color, float width)
    {
	    _lineVertices[0] = UnitToGraph(x1, y1);
	    _lineVertices[1] = UnitToGraph(x2, y2);
	    Handles.color = color;
	    Handles.DrawAAPolyLine(width, _lineVertices);
    }
    
    private void DrawRect(float x1, float y1, float x2, float y2, Color fill, Color line)
    {
	    _rectVertices[0] = UnitToGraph(x1, y1);
	    _rectVertices[1] = UnitToGraph(x2, y1);
	    _rectVertices[2] = UnitToGraph(x2, y2);
	    _rectVertices[3] = UnitToGraph(x1, y2);

	    Handles.DrawSolidRectangleWithOutline(_rectVertices, fill, line);
    }
}
