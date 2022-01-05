using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{

	public readonly Vector2 Position;

	byte qi;

	public int Index = 0;

	public Point(Vector2 Position, byte Qi, int Index)
	{
		this.Position = Position;
		qi = Qi;
		this.Index = Index;
	}

	public byte GetQi()
	{
		return qi;
	}
}
