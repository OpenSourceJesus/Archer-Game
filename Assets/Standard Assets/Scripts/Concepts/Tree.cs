using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Linq;
using Extensions;

public class TreeNode<T> : IEnumerable<TreeNode<T>>
{
	private readonly T _value;
	private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();
	public TreeNode (T value)
	{
		_value = value;
	}
	public TreeNode<T> this[int i]
	{
		get { return _children[i]; }
	}
	public TreeNode<T> Parent { get; private set; }
	public T Value { get { return _value; } }
	public ReadOnlyCollection<TreeNode<T>> Children
	{
		get { return _children.AsReadOnly(); }
	}

	public TreeNode<T> AddChild (T value)
	{
		var node = new TreeNode<T>(value) {Parent = this};
		_children.Add(node);
		return node;
	}

	public TreeNode<T>[] AddChildren (params T[] values)
	{
		return values.Select(AddChild).ToArray();
	}

	public bool RemoveChild (TreeNode<T> node)
	{
		return _children.Remove(node);
	}

	public void Traverse (Action<T> action)
	{
		action(Value);
		foreach (var child in _children)
			child.Traverse(action);
	}

	public IEnumerable<T> Flatten ()
	{
		return new[] {Value}.Concat(_children.SelectMany(x => x.Flatten()));
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator();
	}

	public IEnumerator<TreeNode<T>> GetEnumerator ()
	{
		yield return this;
		foreach (var directChild in this.Children)
		{
			foreach (var anyChild in directChild)
				yield return anyChild;
		}
	}

	public virtual TreeNode<T> GetRoot ()
	{
		TreeNode<T> root = this;
		while (root.Parent != null)
			root = root.Parent;
		return root;
	}

	public virtual bool Contains (T value)
	{
		TreeNode<T> root = this;
		IEnumerator rootEnumerator = root.GetEnumerator();
		TreeNode<T> node;
		while (rootEnumerator.MoveNext())
		{
			node = (TreeNode<T>) rootEnumerator.Current; 
			if (node._value.Equals(value))
				return true;
		}
		return false;
	}

	public virtual TreeNode<T> GetChild (T value)
	{
		TreeNode<T> root = this;
		IEnumerator rootEnumerator = root.GetEnumerator();
		TreeNode<T> node;
		while (rootEnumerator.MoveNext())
		{
			node = (TreeNode<T>) rootEnumerator.Current; 
			if (node._value.Equals(value))
				return node;
		}
		return null;
	}

	public virtual int[] GetPathToChild (T value)
	{
		if (_value.Equals(value))
			return new int[0];
		List<KeyValuePair<int[], TreeNode<T>>> remainingChildValuesAndPaths = new List<KeyValuePair<int[], TreeNode<T>>>();
		remainingChildValuesAndPaths.Add(new KeyValuePair<int[], TreeNode<T>>(new int[0], this));
		while (remainingChildValuesAndPaths.Count > 0)
		{
			for (int i = 0; i < remainingChildValuesAndPaths[0].Value.Children.Count; i ++)
			{
				if (remainingChildValuesAndPaths[0].Value.Children[i]._value.Equals(value))
					return remainingChildValuesAndPaths[0].Key.Add(i);
				remainingChildValuesAndPaths.Add(new KeyValuePair<int[], TreeNode<T>>(remainingChildValuesAndPaths[0].Key.Add(i), remainingChildValuesAndPaths[0].Value.Children[i]));
			}
			remainingChildValuesAndPaths.RemoveAt(0);
		}
		return null;
	}

	public virtual TreeNode<T> GetChildAtPath (int[] path)
	{
		TreeNode<T> output = this;
		for (int i = 0; i < path.Length; i ++)
		{
			if (output.Children.Count > path[i])
				output = output.Children[path[i]];
		}
		return output;
	}

	public virtual int GetMaxTiers ()
	{
		throw new Exception("Not implemented yet");
	}
}