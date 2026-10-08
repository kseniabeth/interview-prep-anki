using System;
using System.Collections.Generic;
using System.Globalization;
public static class Trees
{
    public sealed class Node
    {
        public int Value; public Node? Left; public Node? Right;
        public Node(int value, Node? left = null, Node? right = null) { Value = value; Left = left; Right = right; }
    }
    // Number of nodes on longest root-to-leaf path. Null tree has depth 0.
    public static int Depth(Node? root) => root == null ? 0 : 1 + Math.Max(Depth(root.Left),Depth(root.Right));
    public static List<int[]> Levels(Node? root)
    {
        var result = new List<int[]>(); if (root == null) return result;
        var queue = new Queue<Node>(); queue.Enqueue(root);
        while (queue.Count > 0)
        {
            int size = queue.Count; var level = new int[size];
            for (int i = 0; i < size; i++)
            {
                Node node = queue.Dequeue(); level[i] = node.Value;
                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }
            result.Add(level);
        }
        return result;
    }
    // Strict BST: duplicate keys invalid. Iterative to tolerate tall trees.
    public static bool ValidBst(Node? root)
    {
        var stack = new Stack<(Node Node,long Low,long High)>();
        if (root != null) stack.Push((root,long.MinValue,long.MaxValue));
        while (stack.TryPop(out var frame))
        {
            if (frame.Node.Value <= frame.Low || frame.Node.Value >= frame.High) return false;
            if (frame.Node.Left != null) stack.Push((frame.Node.Left,frame.Low,frame.Node.Value));
            if (frame.Node.Right != null) stack.Push((frame.Node.Right,frame.Node.Value,frame.High));
        }
        return true;
    }
    // Assumes valid BST. k is 1-based; throws if k exceeds node count.
    public static int KthSmallest(Node? root, int k)
    {
        if (k < 1) throw new ArgumentOutOfRangeException(nameof(k));
        var stack = new Stack<Node>(); Node? current = root;
        while (current != null || stack.Count > 0)
        {
            while (current != null) { stack.Push(current); current = current.Left; }
            current = stack.Pop(); if (--k == 0) return current.Value; current = current.Right;
        }
        throw new ArgumentOutOfRangeException(nameof(k));
    }
    // Both target NODE REFERENCES must exist in tree. Values need not be unique.
    public static Node? LowestCommonAncestor(Node? root, Node p, Node q)
    {
        ArgumentNullException.ThrowIfNull(p); ArgumentNullException.ThrowIfNull(q);
        if (root == null || ReferenceEquals(root,p) || ReferenceEquals(root,q)) return root;
        Node? left = LowestCommonAncestor(root.Left,p,q), right = LowestCommonAncestor(root.Right,p,q);
        return left != null && right != null ? root : left ?? right;
    }
    // Preorder with '#' nulls. Recursion suitable only for bounded-depth input.
    public static string Serialize(Node? root)
    {
        var tokens = new List<string>();
        void Visit(Node? node)
        {
            if (node == null) { tokens.Add("#"); return; }
            tokens.Add(node.Value.ToString(CultureInfo.InvariantCulture)); Visit(node.Left); Visit(node.Right);
        }
        Visit(root); return string.Join(",",tokens);
    }
    public static Node? Deserialize(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        string[] tokens = data.Split(','); int index = 0;
        Node? Read()
        {
            if (index == tokens.Length) throw new FormatException("Truncated tree.");
            string token = tokens[index++]; if (token == "#") return null;
            int value = int.Parse(token,CultureInfo.InvariantCulture);
            var node = new Node(value); node.Left = Read(); node.Right = Read(); return node;
        }
        Node? root = Read(); if (index != tokens.Length) throw new FormatException("Trailing tokens."); return root;
    }

    // Known-present unique BST keys. Does not verify presence or BST validity.
    public static Node? BstLowestCommonAncestor(Node? root, int p, int q)
    {
        while (root != null)
        {
            if (p < root.Value && q < root.Value) root = root.Left;
            else if (p > root.Value && q > root.Value) root = root.Right;
            else return root;
        }
        return null;
    }
    public static int[] RightSideView(Node? root)
    {
        var levels = Levels(root); var answer = new int[levels.Count];
        for (int i = 0; i < levels.Count; i++) answer[i] = levels[i][^1];
        return answer;
    }
    public static List<int[]> ZigzagLevels(Node? root)
    {
        var levels = Levels(root);
        for (int i = 1; i < levels.Count; i += 2) Array.Reverse(levels[i]);
        return levels;
    }

}
