using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARNavigation.Navigation
{
    /// <summary>
    /// Handles visualization of navigation paths in AR
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [Header("Path Visualization")]
        public LineRenderer pathLine;
        public GameObject arrowPrefab;
        public GameObject waypointPrefab;
        
        [Header("Visual Settings")]
        public float pathHeight = 0.05f;
        public float lineWidth = 0.1f;
        public Color pathColor = new Color(0, 0.7f, 1f, 0.8f);
        public int pathSegments = 50;
        public bool useBezierCurves = true;
        
        [Header("Arrow Settings")]
        public float arrowSpacing = 2.0f;
        public float arrowHeight = 0.1f;
        public float arrowScale = 0.3f;
        
        [Header("Waypoint Settings")]
        public bool showWaypoints = true;
        public float waypointScale = 0.5f;
        public float waypointHeight = 0.1f;
        
        // Path rendering objects
        private List<GameObject> pathArrows = new List<GameObject>();
        private List<GameObject> pathWaypoints = new List<GameObject>();
        private List<Vector3> currentPathPoints = new List<Vector3>();
        
        private void Awake()
        {
            // Create line renderer if not assigned
            if (pathLine == null)
            {
                GameObject lineObj = new GameObject("PathLine");
                lineObj.transform.SetParent(transform);
                pathLine = lineObj.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }
        }
        
        private void SetupLineRenderer()
        {
            // Configure line renderer
            pathLine.startWidth = lineWidth;
            pathLine.endWidth = lineWidth;
            pathLine.material = new Material(Shader.Find("Sprites/Default"));
            pathLine.startColor = pathColor;
            pathLine.endColor = pathColor;
            pathLine.positionCount = 0;
            pathLine.useWorldSpace = true;
            
            // Set line renderer to zero points initially
            pathLine.positionCount = 0;
        }
        
        /// <summary>
        /// Visualize a path with the specified points
        /// </summary>
        public void VisualizePath(List<Vector3> pathPoints)
        {
            // Clear existing visualization
            ClearPath();
            
            if (pathPoints == null || pathPoints.Count < 2)
            {
                Debug.LogWarning("Not enough points to visualize path");
                return;
            }
            
            // Store current path points
            currentPathPoints = new List<Vector3>(pathPoints);
            
            // Adjust path height to be consistent
            List<Vector3> heightAdjustedPoints = new List<Vector3>();
            foreach (Vector3 point in pathPoints)
            {
                heightAdjustedPoints.Add(new Vector3(point.x, point.y + pathHeight, point.z));
            }
            
            // Draw path line
            if (useBezierCurves && pathPoints.Count > 2)
            {
                DrawBezierPath(heightAdjustedPoints);
            }
            else
            {
                DrawLinearPath(heightAdjustedPoints);
            }
            
            // Place arrows along the path
            PlacePathArrows();
            
            // Place waypoints at key points
            if (showWaypoints)
            {
                PlaceWaypoints(heightAdjustedPoints);
            }
        }
        
        /// <summary>
        /// Draw a linear path connecting the points directly
        /// </summary>
        private void DrawLinearPath(List<Vector3> points)
        {
            pathLine.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                pathLine.SetPosition(i, points[i]);
            }
        }
        
        /// <summary>
        /// Draw a smooth Bezier curve through the points
        /// </summary>
        private void DrawBezierPath(List<Vector3> points)
        {
            int curveSegments = pathSegments;
            
            // Create bezier path with control points
            List<Vector3> bezierPoints = new List<Vector3>();
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p0 = points[i];
                Vector3 p1 = points[i + 1];
                
                // For bezier control points, use nearby points
                Vector3 c0, c1;
                
                if (i > 0)
                {
                    // Use the previous point to influence control point
                    Vector3 prev = points[i - 1];
                    c0 = p0 + (p0 - prev).normalized * Vector3.Distance(p0, p1) * 0.3f;
                }
                else
                {
                    // First point, so just use direction to next point
                    c0 = p0 + (p1 - p0).normalized * Vector3.Distance(p0, p1) * 0.3f;
                }
                
                if (i < points.Count - 2)
                {
                    // Use the next point to influence control point
                    Vector3 next = points[i + 2];
                    c1 = p1 + (p1 - next).normalized * Vector3.Distance(p0, p1) * 0.3f;
                }
                else
                {
                    // Last segment, so just use direction from previous point
                    c1 = p1 + (p1 - p0).normalized * Vector3.Distance(p0, p1) * 0.3f;
                }
                
                // Calculate points along the curve
                for (int j = 0; j <= curveSegments; j++)
                {
                    float t = j / (float)curveSegments;
                    Vector3 point = CalculateBezierPoint(p0, c0, c1, p1, t);
                    bezierPoints.Add(point);
                }
            }
            
            // Set line renderer positions
            pathLine.positionCount = bezierPoints.Count;
            for (int i = 0; i < bezierPoints.Count; i++)
            {
                pathLine.SetPosition(i, bezierPoints[i]);
            }
        }
        
        /// <summary>
        /// Calculate a point along a cubic Bezier curve
        /// </summary>
        private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            
            Vector3 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * p3;
            
            return point;
        }
        
        /// <summary>
        /// Place arrow indicators along the path
        /// </summary>
        private void PlacePathArrows()
        {
            if (arrowPrefab == null || pathLine.positionCount < 2)
                return;
                
            // Get positions from line renderer for accurate arrow placement
            Vector3[] positions = new Vector3[pathLine.positionCount];
            pathLine.GetPositions(positions);
            
            // Calculate total path length
            float totalLength = 0;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                totalLength += Vector3.Distance(positions[i], positions[i + 1]);
            }
            
            // Place arrows evenly along the path
            int numArrows = Mathf.Max(1, Mathf.FloorToInt(totalLength / arrowSpacing));
            float stepSize = totalLength / numArrows;
            
            float currentDistance = stepSize / 2; // Start in the middle of the first segment
            int currentSegment = 0;
            float accumulatedLength = 0;
            
            for (int i = 0; i < numArrows; i++)
            {
                // Find the right segment for the current distance
                while (currentSegment < positions.Length - 2)
                {
                    float segmentLength = Vector3.Distance(positions[currentSegment], positions[currentSegment + 1]);
                    if (accumulatedLength + segmentLength >= currentDistance)
                        break;
                        
                    accumulatedLength += segmentLength;
                    currentSegment++;
                }
                
                if (currentSegment >= positions.Length - 1)
                    break;
                    
                // Calculate position along the segment
                Vector3 segStart = positions[currentSegment];
                Vector3 segEnd = positions[currentSegment + 1];
                float segmentLength = Vector3.Distance(segStart, segEnd);
                float t = (currentDistance - accumulatedLength) / segmentLength;
                Vector3 arrowPosition = Vector3.Lerp(segStart, segEnd, t);
                
                // Calculate direction for arrow rotation
                Vector3 direction = (segEnd - segStart).normalized;
                
                // Create arrow
                GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
                arrow.transform.SetParent(transform);
                arrow.transform.localScale = Vector3.one * arrowScale;
                
                // Orient arrow along path direction
                if (direction != Vector3.zero)
                {
                    arrow.transform.forward = direction;
                }
                
                // Keep track of arrows
                pathArrows.Add(arrow);
                
                // Move to next position
                currentDistance += stepSize;
            }
        }
        
        /// <summary>
        /// Place waypoint markers at key points along the path
        /// </summary>
        private void PlaceWaypoints(List<Vector3> points)
        {
            if (waypointPrefab == null || points.Count < 2)
                return;
                
            // Place waypoints at key points (not at every point to avoid clutter)
            for (int i = 1; i < points.Count - 1; i++)
            {
                // If direction changes significantly, add a waypoint
                if (i > 0 && i < points.Count - 1)
                {
                    Vector3 prevDir = (points[i] - points[i - 1]).normalized;
                    Vector3 nextDir = (points[i + 1] - points[i]).normalized;
                    
                    // If direction changes by more than 30 degrees, add a waypoint
                    if (Vector3.Angle(prevDir, nextDir) > 30f)
                    {
                        CreateWaypoint(points[i]);
                    }
                }
            }
            
            // Always add a waypoint at the destination
            if (points.Count > 0)
            {
                CreateWaypoint(points[points.Count - 1]);
            }
        }
        
        /// <summary>
        /// Create a waypoint at the specified position
        /// </summary>
        private void CreateWaypoint(Vector3 position)
        {
            GameObject waypoint = Instantiate(waypointPrefab, position, Quaternion.identity);
            waypoint.transform.SetParent(transform);
            waypoint.transform.localScale = Vector3.one * waypointScale;
            pathWaypoints.Add(waypoint);
        }
        
        /// <summary>
        /// Clear the current path visualization
        /// </summary>
        public void ClearPath()
        {
            // Clear line renderer
            if (pathLine != null)
            {
                pathLine.positionCount = 0;
            }
            
            // Destroy arrow objects
            foreach (GameObject arrow in pathArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            pathArrows.Clear();
            
            // Destroy waypoint objects
            foreach (GameObject waypoint in pathWaypoints)
            {
                if (waypoint != null)
                {
                    Destroy(waypoint);
                }
            }
            pathWaypoints.Clear();
            
            // Clear stored path points
            currentPathPoints.Clear();
        }
    }
}
