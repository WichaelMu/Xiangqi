using System.Collections.Generic;
using UnityEngine;
using MW.General;
using MW.Conversion;

public class Board : MonoBehaviour
{

	public static Point[] board;

	[Tooltip("The scale of the board.")] public float Scalar = 1;
	public float InverseScalar { get { return 1 / Scalar; } }

	[SerializeField] SpriteRenderer G, M, P, T, RG, RE, RX, GG, GE, GX;

	Dictionary<byte, SpriteRenderer> byteToSprite;
	static readonly Color Red = Colour.Colour255(190, 0, 0);
	static readonly Color Green = Colour.Colour255(15, 100, 0);

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

	void MakeBoard()
	{
		for (byte rank = 0; rank < 5; rank++)
		{
			for (byte file = 0; file < 9; file++)
			{
				int Position = rank * 9 + file;
				board[Position] = new Point(new Vector2(file * Scalar, rank * Scalar), DetermineStartingQis(file, rank), Position);

				byte MirroredRank = (byte)(Common.MirrorNumber(rank, 0, 9));
				byte MirroredFile = (byte)(Common.MirrorNumber(file, 0, 8));
				byte MirroredPosition = (byte)(MirroredRank * 9 + MirroredFile);

				board[MirroredPosition] = new Point(new Vector2(MirroredFile * Scalar, MirroredRank * Scalar), DetermineStartingQis(MirroredFile, MirroredRank), MirroredPosition);
			}
		}
	}

	byte DetermineStartingQis(byte file, byte rank)
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
				return Qi.None;
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
				return Qi.None;
			}
		}
		else
		{
			return Qi.None;
		}

		return result;
	}

	void SpawnQis()
	{
		foreach (Point p in board)
		{
			byte qi = p.GetQi();
			if (qi == Qi.None) { continue; }

			if (Qi.IsSlidingQi(qi) || Qi.IsMa(qi) || Qi.Type(qi) == Qi.T)
			{
				SpriteRenderer inGameQi = Instantiate(byteToSprite[Qi.Type(qi)], p.Position, Quaternion.identity);
				inGameQi.color = Qi.Colour(qi) == Qi.R ? Red : Green;
			}
			else
			{
				Instantiate(byteToSprite[qi], p.Position, Quaternion.identity);
			}
		}
	}

	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			for (int i = 0; i < 90; ++i)
			{
				Point p = board[i];
				Gizmos.DrawSphere(p.Position, .1f);
				UnityEditor.Handles.Label(p.Position, (p.Position.y + " " + p.Position.x).ToString() + "\n" + p.GetQi().ToString() + "\n" + i);
			}
		}
	}
}
