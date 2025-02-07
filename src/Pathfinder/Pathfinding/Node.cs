using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace Pathfinder.Pathfinding;

public class Node : IEquatable<Node>
{
    public Node(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X, Y;

    public bool Equals(Node other) => (this is null && other is null) || (other is not null && this is not null && X == other.X && Y == other.Y);
    public static bool operator ==(Node node, Node other) => node.Equals(other);
    public static bool operator !=(Node node, Node other) => !node.Equals(other);
    public override bool Equals(object obj) => obj is Node other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}