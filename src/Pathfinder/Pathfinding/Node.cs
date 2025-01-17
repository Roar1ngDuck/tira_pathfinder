using System;

namespace Pathfinder.Pathfinding;

public struct Node : IEquatable<Node>
{
    public Node(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X, Y;

    public bool Equals(Node other) => X == other.X && Y == other.Y;
    public static bool operator ==(Node node, Node other) => node.Equals(other);
    public static bool operator !=(Node node, Node other) => !node.Equals(other);
    public override bool Equals(object obj) => obj is Node other && Equals(other);
}