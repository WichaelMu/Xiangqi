using System.Collections.Generic;
using UnityEngine;
using MW;
using MW.Conversion;

public class Board : MonoBehaviour
{
	public Point[] board;
	public BoardUI UI;

	[Tooltip("The scale of the board.")] public float Scalar = 1;
	public float InverseScalar { get { return 1 / Scalar; } }

	[SerializeField] SpriteRenderer G, M, P, T, RG, RE, RX, GG, GE, GX;

	Dictionary<byte, SpriteRenderer> byteToSprite;
	static readonly Color Red = Colour.Colour255(190, 0, 0);
	static readonly Color Green = Colour.Colour255(15, 100, 0);
	public Dictionary<Qi, Point> InternalBoardR;
	public Dictionary<Qi, Point> InternalBoardG;

	void Awake()
	{
		board = new Point[90];
		MakeBoard();

		byteToSprite = new Dictionary<byte, SpriteRenderer>()
		{
			{ Qi.Goo, G },
			{ Qi.Ma, M },
			{ Qi.Paow, P },
			{ Qi.T, T },
			{ Qi.Goong | Qi.R, RG },
			{ Qi.Jerng | Qi.R, RE },
			{ Qi.Xut | Qi.R, RX },
			{ Qi.Goong | Qi.G, GG },
			{ Qi.Jerng | Qi.G, GE },
			{ Qi.Xut | Qi.G, GX }
		};

		SpawnQis();
		byteToSprite.Clear();
	}

	/// <param name="Index">The index at <see cref="Board.board"/> to check.</param>
	/// <returns>The Point at Index.</returns>
	public Point At(int Index)
	{
		return board[Index];
	}

	/// <summary>Registers a move from to on <see cref="Board.board"/></summary>
	/// <param name="from">The departing Point.</param>
	/// <param name="to">The destination Point.</param>
	public void RegisterMove(Point from, Point to)
	{
		Qi fromQi = from.GetQi();
		Qi toQi = to.GetQi();
		if (Qi.Colour(fromQi) == Qi.R)
		{
			InternalBoardR[fromQi] = to;

			if (toQi != Qi.None)
				InternalBoardG.Remove(toQi);
		}
		else
		{
			InternalBoardG[fromQi] = to;

			if (toQi != Qi.None)
				InternalBoardR.Remove(toQi);
		}

		from.MoveOutbound(to);
	}

	/// <param name="Index">The index to for validity.</param>
	/// <returns>If Index is >= 0 &amp;&amp; &lt; 90.</returns>
	public static bool IsValid(int Index)
	{
		return Index >= 0 && Index < 90;
	}

	void MakeBoard()
	{
		for (byte rank = 0; rank < 5; rank++)
		{
			for (byte file = 0; file < 9; file++)
			{
				int Position = rank * 9 + file;
				board[Position] = new Point(new Vector2(file * Scalar, rank * Scalar), DetermineStartingQis(file, rank), Position);

				byte MirroredRank = (byte)(Utils.MirrorNumber(rank, 0, 9));
				byte MirroredFile = (byte)(Utils.MirrorNumber(file, 0, 8));
				byte MirroredPosition = (byte)(MirroredRank * 9 + MirroredFile);

				board[MirroredPosition] = new Point(new Vector2(MirroredFile * Scalar, MirroredRank * Scalar), DetermineStartingQis(MirroredFile, MirroredRank), MirroredPosition);
			}
		}
	}

	Qi DetermineStartingQis(byte file, byte rank)
	{
		byte result = 0;

		result |= (rank < 5)
			? Qi.R
			: Qi.G;

		if (rank > 3)
		{
			rank = (byte)Mathf.Abs(rank - 9);
		}

		if (rank == 0)
		{
			if (file == 0 || file == 8)
			{
				result |= Qi.Goo;
			}
			else if (file == 1 || file == 7)
			{
				result |= Qi.Ma;
			}
			else if (file == 2 || file == 6)
			{
				result |= Qi.Jerng;
			}
			else if (file == 3 || file == 5)
			{
				result |= Qi.T;
			}
			else
			{
				result |= Qi.Goong;
			}
		}
		else if (rank == 2)
		{
			if (file == 1 || file == 7)
			{
				result |= Qi.Paow;
			}
			else
			{
				return Qi.Null;
			}
		}
		else if (rank == 3)
		{
			if (file == 0 || file == 2 || file == 4 || file == 6 || file == 8)
			{
				result |= Qi.Xut;
			}
			else
			{
				return Qi.Null;
			}
		}
		else
		{
			return Qi.Null;
		}

		return new Qi(result);
	}

	void SpawnQis()
	{
		if (InternalBoardR == null)
		{
			InternalBoardR = new Dictionary<Qi, Point>();
		}

		if (InternalBoardG == null)
		{
			InternalBoardG = new Dictionary<Qi, Point>();
		}

		foreach (Point p in board)
		{
			byte qi = p.GetQiAsByte();
			if (qi == Qi.None) { continue; }

			SpriteRenderer newQi;

			if (Qi.IsSlidingQi(qi) || Qi.IsMa(qi) || Qi.Type(qi) == Qi.T)
			{
				newQi = Instantiate(byteToSprite[Qi.Type(qi)], p.Position, Quaternion.identity);
				newQi.color = Qi.Colour(qi) == Qi.R ? Red : Green;
			}
			else
			{
				newQi = Instantiate(byteToSprite[qi], p.Position, Quaternion.identity);
			}

			// After spawning a new qi, add it to their respective colour Dictionary.
			if (Qi.Colour(qi) == Qi.R)
			{
				InternalBoardR.Add(p.GetQi(), p);
			}
			else
			{
				InternalBoardG.Add(p.GetQi(), p);
			}

			p.GetQi().SetTransform(newQi.transform);
		}
	}

	/// <param name="index">The index to check for <see cref="Qi.None"/></param>
	/// <param name="qi">The out byte of the qi at index, regardless of if it equals to <see cref="Qi.None"/>.</param>
	/// <returns>True if the qi at index is NOT <see cref="Qi.None"/></returns>
	public bool QiIsNotNone(int index, out byte qi)
	{
		byte qiBoard = board[index].GetQiAsByte();

		qi = qiBoard;

		return qiBoard != Qi.None;
	}

	public HashSet<Point> GetAttackedPoints(ref byte colourToGetAttacks)
	{
		MArray<Point> attackedMovesFromQisOfColour = new MArray<Point>();

		if (colourToGetAttacks == Qi.R)
		{
			foreach (KeyValuePair<Qi, Point> qp in InternalBoardR)
			{
				MArray<Point> legalForThisQi = MoveHandler.Handle(this, qp.Value, true);
				
				// Sometimes the MArray will have nothing if a Paow cannot attack anything.
				// Also do not pop the first element if this qi is a Paow; it will remove
				// the first point that it can attack after finding something to launch over.
				if (legalForThisQi.Num > 0 && Qi.Type(qp.Key) != Qi.Paow)
					legalForThisQi.FirstPop();
				attackedMovesFromQisOfColour += legalForThisQi;
			}
		}
		else
		{
			foreach (KeyValuePair<Qi, Point> qp in InternalBoardG)
			{
				MArray<Point> legalForThisQi = MoveHandler.Handle(this, qp.Value, true);
				
				// Sometimes the MArray will have nothing if a Paow cannot attack anything.
				// Also do not pop the first element if this qi is a Paow; it will remove
				// the first point that it can attack after finding something to launch over.
				if (legalForThisQi.Num > 0 && Qi.Type(qp.Key) != Qi.Paow)
					legalForThisQi.FirstPop();
				attackedMovesFromQisOfColour += legalForThisQi;
			}
		}

		// Populate the HashSet of attacked points.
		HashSet<Point> attacked = new HashSet<Point>();
		for (int i = 0; i < attackedMovesFromQisOfColour.Num; ++i)
		{
			Point attackedPoint = attackedMovesFromQisOfColour[i];

			if (!attacked.Contains(attackedPoint))
				attacked.Add(attackedPoint);
		}

		return attacked;
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			for (int i = 0; i < 90; ++i)
			{
				Point p = board[i];
				//Gizmos.DrawSphere(p.Position, Scalar * .1f);
				UnityEditor.Handles.Label(p.Position, (p.Position.y + " " + p.Position.x).ToString() + "\n" + p.GetQiAsByte().ToString() + "\n" + i);
			}

			if (MoveHandler.attacked != null)
				foreach (var q in MoveHandler.attacked)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere(q.Position, Scalar * .1f);
				}
		}
	}
#endif
}
