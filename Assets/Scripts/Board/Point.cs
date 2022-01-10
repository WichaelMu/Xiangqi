using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{

	public readonly Vector2 Position;

	Qi qi;

	/// <summary>The <see cref="Board"/> index of this Point.</summary>
	public int Index = 0;

	public Point(Vector2 Position, Qi Qi, int Index)
	{
		this.Position = Position;
		qi = Qi;
		this.Index = Index;
	}

	/// <returns>This qi as a <see cref="byte"/>, representing it's colour AND type.</returns>
	public byte GetQiAsByte()
	{
		return (byte)qi;
	}

	/// <returns>This qi as a <see cref="Qi"/> <see cref="object"/>.</returns>
	public Qi GetQi()
	{
		return qi;
	}

	/// <returns>The <see cref="Transform"/> associated with this qi.</returns>
	public Transform GetQiTransform()
	{
		return qi.ReferenceTransform;
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
