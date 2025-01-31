using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace Pathfinder.Pathfinding;

public class Node : IEquatable<Node>, IComparable<Node>
{
    public Node(int x, int y)
    {
        X = x;
        Y = y;
        H = null;
        Cost = null;
    }

    public int X, Y;
    public double? Cost { get; set; }
    public double? H { get; set; }
    public int? Direction { get; set; }
    public bool Obstacle { get; set; }

    public (int, int) Position { get => (X, Y); set => (X, Y) = value; }

    public int CompareTo(Node other)
    {
        if (other is null) return 1;
        if (Cost is null) return other.Cost == null ? 0 : 1;
        if (other.Cost is null) return -1;
        return Cost.Value.CompareTo(other.Cost.Value);
    }

    public bool Equals(Node other) => (this is null && other is null) || (other is not null && this is not null && X == other.X && Y == other.Y);
    public static bool operator ==(Node node, Node other) => node.Equals(other);
    public static bool operator !=(Node node, Node other) => !node.Equals(other);
    public override bool Equals(object obj) => obj is Node other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}