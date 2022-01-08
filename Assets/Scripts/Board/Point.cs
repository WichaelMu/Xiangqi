using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{

	public readonly Vector2 Position;

	Qi qi;

	public int Index = 0;

	public Point(Vector2 Position, Qi Qi, int Index)
	{
		this.Position = Position;
		qi = Qi;
		this.Index = Index;
	}

	public byte GetQiAsByte()
	{
		return (byte)qi;
	}

	public Qi GetQi()
	{
		return qi;
	}

	public void MoveOutbound (Point destination)
	{
		destination.MoveInbound(qi);

		qi = new Qi(Qi.None);
	}

	public void MoveInbound (Qi inbound)
	{
		qi = inbound;
	}
}
