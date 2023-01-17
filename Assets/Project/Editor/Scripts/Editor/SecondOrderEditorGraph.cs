using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SecondOrderEditorGraph
{
    // Vertex buffers
    private Vector3[] _rectVertices = new Vector3[4];
    private Vector3[] _lineVertices = new Vector3[2];
    private Vector3[] _curveVertices;
    
    private List<LineColorPair> _linesX = new List<LineColorPair>();
    private List<LineColorPair> _linesY = new List<LineColorPair>();
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
		
		
		// Evaluate the Second Order Dynamic Step Response
		int vertexCount = 0;
		SecondOrderDynamics dynamicsBounds = new SecondOrderDynamics(frequency, dampingCoefficient, initialResponse, Vector3.zero);

		Vector3 TargetPosition(int _vertexCount)
		{
			return new Vector3(RangeX * vertexCount / (_graphIterations - 1), 1f, 0f);
			return new Vector3((RangeX / _graphIterations) * _vertexCount, 1f, 0f);
		}

		
		while (vertexCount < _graphIterations)
		{
			var targetPosition = TargetPosition(vertexCount);
			Vector3 xy = dynamicsBounds.UpdatePosition(Time.fixedDeltaTime * 0.1f, targetPosition, Vector3.zero);
			var y = xy.y;
			
			if (y < _minY) {
				_minY = y;
			} else if (y > _maxY) {
				_maxY = y;
			}

			vertexCount++;
		}
		
		vertexCount = 0;
		SecondOrderDynamics dynamicsDrawn = new SecondOrderDynamics(frequency, dampingCoefficient, initialResponse, Vector3.zero);
		
		while (vertexCount < _graphIterations) {
			
			var targetPosition = TargetPosition(vertexCount);
			// Debug.Log("Before: " + targetPosition);
			Vector3 xy = dynamicsDrawn.UpdatePosition(Time.fixedDeltaTime * 0.1f, targetPosition, Vector3.zero);
			// Debug.Log("After: " + xy);
			var x = xy.x;
			var y = xy.y;
			
			_curveVertices[vertexCount++] = UnitToGraphUnclamped(x, y);
			
		}
		

		// Draw the Background of the Graph
		DrawRect(_minX, _minY, _maxX, _maxY, _graphColors.background, _graphColors.outline);
		
		/*
		// Vertical helper lines
		if (GridLinesX > 0)
		{
			float multiplier = 1;
			while ((rangeX / (GridLinesX * multiplier)) > (rect.width / 2f))
				multiplier *= 2;

			for (float x = minX; x <= maxX; x += GridLinesX * multiplier) {
				
				DrawLine(x, minY, x, maxY, Colors.GridLine, 1);
				
				if (x == 0) { continue;}
				
				// Get Label position relative to the graph and offset it to be left of the line
				Vector3 labelPosition = UnitToGraphUnlocked(x, minY);
				labelPosition.y += 10f;
				labelPosition.x -= 7.5f;
			
				// Draw the label
				Handles.Label(labelPosition, x.ToString());
			}
		}
		// Horizontal helper lines
		if (GridLinesY > 0)
		{
			float multiplier = 1;
			while ((rangeY / (GridLinesY * multiplier)) > (rect.height / 2f))
				multiplier *= 2;

			for (float y = minY; y <= maxY; y += GridLinesY * multiplier)
				DrawLine(minX, y, maxX, y, Colors.GridLine, 1);
		}

		// Vertical lines
		foreach (var line in linesX)
		{
			DrawLine(line.Position, minY, line.Position, maxY, line.Color, 2);
		}
		// Horizontal lines
		foreach (var line in linesY)
		{
			// Draw the horizontal line
			DrawLine(minX, line.Position, maxX, line.Position, line.Color, 2);
			
			// Get Label position relative to the graph and offset it to be left of the line
			Vector3 labelPosition = UnitToGraphUnlocked(minX, line.Position);
			labelPosition.x -= 15f;
			
			// Draw the label
			Handles.Label(labelPosition, line.Position.ToString());
		}
		*/
		
		
		if (vertexCount > 1)
		{
			Handles.color = _graphColors.function;
			Handles.DrawAAPolyLine(2.0f, vertexCount, _curveVertices);
		}
		
		
	}
    /*
    private Vector2 EvaluateSecondOrderDynamics(SecondOrderDynamics dynamics, float frequency, float dampingCoefficient, float initialResponse)
	{
		//Vector3 targetPosition = new Vector3(RangeX * vertexCount / (_graphIterations - 1), 1f, 0);
		//Vector3 xy = dynamics.UpdatePosition(Time.fixedDeltaTime / (_graphIterations / 100), targetPosition, Vector3.zero);
		//return 
	}
    */
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
    
    private void DrawRect(float x1, float y1, float x2, float y2, Color fill, Color line)
    {
	    _rectVertices[0] = UnitToGraph(x1, y1);
	    _rectVertices[1] = UnitToGraph(x2, y1);
	    _rectVertices[2] = UnitToGraph(x2, y2);
	    _rectVertices[3] = UnitToGraph(x1, y2);

	    Handles.DrawSolidRectangleWithOutline(_rectVertices, fill, line);
    }
}
