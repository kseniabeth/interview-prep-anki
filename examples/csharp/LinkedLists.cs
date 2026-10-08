using System;
using System.Collections.Generic;
public static class LinkedLists
{
    public sealed class Node
    {
        public int Value;
        public Node? Next;
        public Node(int value, Node? next = null) { Value = value; Next = next; }
    }
    public static bool HasCycle(Node? head)
    {
        Node? slow = head, fast = head;
        while (fast?.Next != null)
        {
            slow = slow!.Next;
            fast = fast.Next.Next;
            if (ReferenceEquals(slow, fast)) return true;
        }
        return false;
    }
    // Acyclic list required. Even length returns the SECOND middle; null -> null.
    public static Node? Middle(Node? head)
    {
        Node? slow = head, fast = head;
        while (fast?.Next != null) { slow = slow!.Next; fast = fast.Next.Next; }
        return slow;
    }
    // Acyclic list required; mutates and reuses existing nodes.
    public static Node? Reverse(Node? head)
    {
        Node? previous = null, current = head;
        while (current != null)
        {
            Node? next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }
        return previous;
    }
    // Sorted, acyclic, DISJOINT lists required. Mutates/reuses their nodes.
    public static Node? Merge(Node? a, Node? b)
    {
        var dummy = new Node(0);
        Node tail = dummy;
        while (a != null && b != null)
        {
            if (a.Value <= b.Value) { tail.Next = a; a = a.Next; }
            else { tail.Next = b; b = b.Next; }
            tail = tail.Next;
        }
        tail.Next = a ?? b;
        return dummy.Next;
    }
    // Reverse inclusive 1-based positions. Validates range before mutation.
    public static Node? ReverseRange(Node? head, int left, int right)
    {
        if (left < 1 || right < left) throw new ArgumentOutOfRangeException(nameof(left));
        int length = 0;
        for (Node? node = head; node != null; node = node.Next) length++;
        if (right > length) throw new ArgumentOutOfRangeException(nameof(right));
        var dummy = new Node(0, head);
        Node before = dummy;
        for (int i = 1; i < left; i++) before = before.Next!;
        Node tail = before.Next!;
        for (int i = 0; i < right - left; i++)
        {
            Node moved = tail.Next!;
            tail.Next = moved.Next;
            moved.Next = before.Next;
            before.Next = moved;
        }
        return dummy.Next;
    }

    // Sorted, acyclic, mutually disjoint lists; reuses and mutates nodes.
    public static Node? MergeMany(Node?[] heads)
    {
        ArgumentNullException.ThrowIfNull(heads);
        var heap = new PriorityQueue<Node,int>();
        foreach (Node? head in heads) if (head != null) heap.Enqueue(head,head.Value);
        var dummy = new Node(0); Node tail = dummy;
        while (heap.TryDequeue(out Node? node,out _))
        {
            Node? next = node.Next; tail.Next = node; tail = node;
            if (next != null) heap.Enqueue(next,next.Value);
        }
        tail.Next = null; return dummy.Next;
    }

}
