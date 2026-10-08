using System;
public sealed class Trie
{
    private sealed class Node { public readonly Node?[] Children = new Node?[26]; public bool End; }
    private readonly Node root = new();
    private static void Validate(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        foreach (char c in word) if (c < 'a' || c > 'z') throw new ArgumentException("Only lowercase ASCII a-z.");
    }
    public void Insert(string word)
    {
        Validate(word); Node current = root;
        foreach (char c in word) { current.Children[c-'a'] ??= new Node(); current = current.Children[c-'a']!; }
        current.End = true;
    }
    private Node? Walk(string text)
    {
        Validate(text); Node? current = root;
        foreach (char c in text) { current = current.Children[c-'a']; if (current == null) return null; }
        return current;
    }
    public bool Search(string word) => Walk(word)?.End == true;
    public bool StartsWith(string prefix) => Walk(prefix) != null;
}
