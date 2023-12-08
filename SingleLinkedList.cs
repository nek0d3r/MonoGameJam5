using System;
using System.Collections.Generic;
using System.Linq;

public class SingleLinkedListNode<T>
{
    public bool HasValue { get; private set; } = false;
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            HasValue = _value != null;
        }
    }
    private T _value = default;
    public SingleLinkedListNode<T> Next { get; set; } = null;

    public SingleLinkedListNode(T data)
    {
        _value = data;
    }
}

public class SingleLinkedList<T>
{
    public SingleLinkedListNode<T> First
    {
        get => nodes.Count > 0 ? nodes.First() : null;
    }
    public SingleLinkedListNode<T> Last
    {
        get => nodes.Count > 0 ? nodes.Last() : null;
    }
    private List<SingleLinkedListNode<T>> nodes = new List<SingleLinkedListNode<T>>();

    public void Unshift(SingleLinkedListNode<T> node)
    {
        if (nodes.Count == 0)
        {
            nodes.Add(node);
            return;
        }

        node.Next = nodes.First();
        nodes.Insert(0, node);
    }

    public void Push(SingleLinkedListNode<T> node)
    {
        if (nodes.Count == 0)
        {
            nodes.Add(node);
            return;
        }

        nodes.Last().Next = node;
        nodes.Add(node);
    }

    // Why implement IEnumerable when you can be the laziest coder on earth?
    internal bool Any(Func<object, bool> value)
    {
        return nodes.Any(value);
    }
}