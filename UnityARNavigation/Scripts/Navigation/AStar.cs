using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARNavigation.Data;

namespace ARNavigation.Navigation
{
    /// <summary>
    /// A* Pathfinding algorithm implementation for AR navigation
    /// </summary>
    public class AStar
    {
        // Internal classes for pathfinding
        private class Node
        {
            public FloorData.NavigationNode navNode;
            public Node parent;
            public float g; // Cost from start
            public float h; // Heuristic (estimated cost to goal)
            public float f => g + h; // Total cost

            public Node(FloorData.NavigationNode navNode, Node parent, float g, float h)
            {
                this.navNode = navNode;
                this.parent = parent;
                this.g = g;
                this.h = h;
            }
        }

        // Find a path between two positions using A* algorithm
        public static List<Vector3> FindPath(FloorData floorData, Vector3 startPos, Vector3 endPos)
        {
            if (floorData == null || floorData.navigationNodes.Count == 0)
            {
                Debug.LogError("No navigation nodes available for pathfinding");
                return null;
            }

            // Find the nearest nodes to start and end positions
            FloorData.NavigationNode startNode = floorData.FindNearestNode(startPos);
            FloorData.NavigationNode endNode = floorData.FindNearestNode(endPos);

            if (startNode == null || endNode == null)
            {
                Debug.LogError("Could not find valid start or end nodes for pathfinding");
                return null;
            }

            // If start and end are the same node, return a simple path
            if (startNode.nodeID == endNode.nodeID)
            {
                List<Vector3> simplePath = new List<Vector3>
                {
                    startPos,
                    endPos
                };
                return simplePath;
            }

            // A* algorithm implementation
            Dictionary<string, Node> allNodes = new Dictionary<string, Node>();
            List<Node> openSet = new List<Node>();
            HashSet<string> closedSet = new HashSet<string>();

            // Create start node and add to open set
            Node startPathNode = new Node(startNode, null, 0, Vector3.Distance(startNode.position, endNode.position));
            openSet.Add(startPathNode);
            allNodes[startNode.nodeID] = startPathNode;

            while (openSet.Count > 0)
            {
                // Get node with lowest f cost
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].f < current.f || (openSet[i].f == current.f && openSet[i].h < current.h))
                    {
                        current = openSet[i];
                    }
                }

                // Remove current from open set and add to closed set
                openSet.Remove(current);
                closedSet.Add(current.navNode.nodeID);

                // Check if we reached the end
                if (current.navNode.nodeID == endNode.nodeID)
                {
                    // Reconstruct path
                    return ReconstructPath(current, startPos, endPos);
                }

                // Check all connected nodes
                foreach (string connectedNodeID in current.navNode.connectedNodes)
                {
                    // Skip if node is in closed set
                    if (closedSet.Contains(connectedNodeID))
                        continue;

                    // Get the connected node
                    FloorData.NavigationNode connectedNavNode = floorData.FindNodeByID(connectedNodeID);
                    if (connectedNavNode == null || !connectedNavNode.isWalkable)
                        continue;

                    // Calculate new cost to this node
                    float moveCost = Vector3.Distance(current.navNode.position, connectedNavNode.position) * connectedNavNode.costMultiplier;
                    float newG = current.g + moveCost;
                    float newH = Vector3.Distance(connectedNavNode.position, endNode.position);

                    // Check if node is already in open set
                    if (allNodes.TryGetValue(connectedNodeID, out Node existingNode))
                    {
                        if (newG < existingNode.g)
                        {
                            // Found a better path - update node
                            existingNode.parent = current;
                            existingNode.g = newG;
                            if (!openSet.Contains(existingNode))
                            {
                                openSet.Add(existingNode);
                            }
                        }
                    }
                    else
                    {
                        // Add new node to open set
                        Node newNode = new Node(connectedNavNode, current, newG, newH);
                        openSet.Add(newNode);
                        allNodes[connectedNodeID] = newNode;
                    }
                }
            }

            // No path found
            Debug.LogWarning("No path found between points");
            return null;
        }

        // Reconstruct path from end node back to start node
        private static List<Vector3> ReconstructPath(Node endNode, Vector3 startPos, Vector3 endPos)
        {
            List<Vector3> path = new List<Vector3>();
            Node current = endNode;

            // Add end position as final target
            path.Add(endPos);

            // Traverse back through the nodes to create the path
            while (current != null)
            {
                path.Add(current.navNode.position);
                current = current.parent;
            }

            // Add actual start position
            path.Add(startPos);

            // Reverse the path (from start to end)
            path.Reverse();

            // Optimize path (remove unnecessary nodes)
            return OptimizePath(path);
        }

        // Remove unnecessary nodes in the path
        private static List<Vector3> OptimizePath(List<Vector3> path)
        {
            if (path.Count <= 2)
                return path;

            List<Vector3> optimizedPath = new List<Vector3>();
            optimizedPath.Add(path[0]); // Always include start point

            // Simplify the path by checking if we can go directly between points
            for (int i = 0; i < path.Count - 2; i++)
            {
                Vector3 current = path[i];
                Vector3 next = path[i + 1];
                Vector3 afterNext = path[i + 2];

                // Check if the direction changes significantly
                Vector3 dirCurrent = (next - current).normalized;
                Vector3 dirNext = (afterNext - next).normalized;

                // If direction changes by more than 10 degrees, add the point
                if (Vector3.Angle(dirCurrent, dirNext) > 10f)
                {
                    optimizedPath.Add(next);
                }
            }

            // Always include end point
            optimizedPath.Add(path[path.Count - 1]);

            return optimizedPath;
        }
    }
}
