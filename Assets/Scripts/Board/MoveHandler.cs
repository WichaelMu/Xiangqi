using System.Collections.Generic;
using MW;
using MW.Diagnostics;

/// <summary>Determines the legal moves for a given <see cref="Qi"/> using <see cref="Handle(Board, Point)"/>.</summary>
public class MoveHandler
{
	delegate bool Rule(int origin, int offset, byte colour, Board board = null);

	static readonly int[] Straight = new int[] { 1, 9, -1, -9 };
	static readonly int[] Diagonal = new int[] { 10, 8, -10, -8 };
	static readonly int[] Ma = new int[] { 11, -7, 19, 17, -11, 7, -19, -17, };

	/// <summary>All legal moves on <see cref="Board.board"/> for <see cref="Qi.T"/>.</summary>
	static readonly HashSet<int> TLegality = new HashSet<int> { 3, 5, 13, 21, 23, 56, 58, 76, 84, 86 };
	/// <summary>All legal moves on <see cref="Board.board"/> for <see cref="Qi.Goong"/>.</summary>
	static readonly HashSet<int> GoongLegality = new HashSet<int> { 3, 4, 5, 12, 13, 14, 21, 22, 23, 56, 57, 58, 75, 76, 77, 84, 85, 86 };

	/// <summary>Handles legal movements, relative to the qi on origin.</summary>
	/// <param name="board">The <see cref="Board"/> to search in.</param>
	/// <param name="origin">The <see cref="Point"/> at which to handle the movements of it's qi.</param>
	/// <returns>Legal moves.</returns>
	public static MArray<Point> Handle(Board board, Point origin)
	{
		byte qi = origin.GetQiAsByte();
		byte colour = Qi.Colour(qi);
		byte type = Qi.Type(qi);

		if (Qi.IsSlidingQi(qi))
		{
			return SlidingQi(board, origin.Index, colour, type);
		}
		else if (Qi.IsDiagonalQi(qi))
		{
			return DiagonalQi(board, origin.Index, colour, type);
		}
		else if (Qi.IsXutOrGoong(qi))
		{
			return SingleMoveQi(board, origin.Index, colour, type);
		}
		else if (Qi.IsMa(qi))
		{
			return MaQi(board, origin.Index, colour);
		}

		return new MArray<Point>();
	}

	/// <summary>The initial call for legal moves on any sliding qi.</summary>
	/// <param name="board">The board to search in.</param>
	/// <param name="origin">The index position of this sliding qi on board.</param>
	/// <param name="colour">The Qi.colour of this sliding qi.</param>
	/// <param name="type">The type of sliding qi. Goo || Paow.</param>
	/// <returns>Legal moves.</returns>
	static MArray<Point> SlidingQi(Board board, int origin, byte colour, byte type)
	{
		MArray<Point> pseudoLegalMoves = new MArray<Point>();

		if (type == Qi.Goo)
			pseudoLegalMoves = TraverseInitialise(board, origin, Straight, SlidingRulesFailed, ref colour, ref type);
		else
			TraversePaow(ref board, origin, Straight, ref pseudoLegalMoves, ref colour);

		return pseudoLegalMoves;
	}

	static MArray<Point> DiagonalQi(Board board, int origin, byte colour, byte type)
	{
		MArray<Point> pseudoLegalMoves;

		if (type == Qi.T)
		{
			pseudoLegalMoves = TraverseInitialise(board, origin, Diagonal, TRulesFailed, ref colour, ref type, depth: 1);
		}
		else
		{
			pseudoLegalMoves = TraverseInitialise(board, origin, Diagonal, JerngRulesFailed, ref colour, ref type, depth: 2);
			RemoveDirectJerngMoves(ref pseudoLegalMoves);
		}

		return pseudoLegalMoves;
	}

	static MArray<Point> SingleMoveQi(Board board, int origin, byte colour, byte type)
	{
		MArray<Point> pseudoLegalMoves;

		if (type == Qi.Xut)
		{
			pseudoLegalMoves = TraverseInitialise(board, origin, Straight, XutRulesFailed, ref colour, ref type, depth: 1);
		}
		else
		{
			pseudoLegalMoves = TraverseInitialise(board, origin, Straight, GoongRulesFailed, ref colour, ref type, depth: 1);
		}

		return pseudoLegalMoves;
	}

	static MArray<Point> MaQi(Board board, int origin, byte colour)
	{
		MArray<Point> pseudoLegalMoves = new MArray<Point>();

		TraverseMa(ref board, origin, Ma, ref pseudoLegalMoves, ref colour);

		return pseudoLegalMoves;
	}

	static void RemoveDirectJerngMoves(ref MArray<Point> jerng)
	{
		MArray<Point> illegal = new MArray<Point>();
		for (int i = 1; i < jerng.Num; ++i)
		{
			int directlyDiagonalIndex = jerng[i].Index;
			foreach (int offset in Diagonal)
			{
				if (directlyDiagonalIndex + offset == jerng[0].Index)
				{
					illegal.Push(jerng[i]);
					break;
				}
			}
		}

		jerng ^= illegal;
	}

	/// <summary>Begins the traversing of legal Points.</summary>
	/// <param name="origin">Where to begin.</param>
	/// <param name="offsets">How to traverse.</param>
	/// <param name="depth">How far this search should go.</param>
	/// <param name="ignoreLastN">How many offsets from the back to ignore. (Going backwards).</param>
	/// <returns>Legal moves.</returns>
	static MArray<Point> TraverseInitialise(Board board, int origin, int[] offsets, Rule Law, ref byte colour, ref byte type, byte depth = 10)
	{
		MArray<Point> legal = new MArray<Point>();
		depth++;
		for (int i = 0; i < offsets.Length; ++i)
		{
			Traverse(ref board, origin, offsets[i], Law, ref legal, ref depth, depth, ref colour, ref type);
		}

		return legal;
	}

	/// <summary>Recursively adds legal Points of a qi at it's position on board.</summary>
	/// <param name="board">The board to check for moves.</param>
	/// <param name="origin">Where to begin searching, recursively.</param>
	/// <param name="offset">The board-direction to mark for legality.</param>
	/// <param name="Law">The rules to follow for legality.</param>
	/// <param name="legal">An MArray of points considered 'legal', having passed Law.</param>
	/// <param name="depth">The offset-wise distance from the initial traversal origin. Default to 10 for no restriction.</param>
	/// <param name="colour">The Qi.colour to indicate a friendly qi.</param>
	/// <param name="type">The type of Qi.</param>
	static void Traverse(ref Board board, int origin, int offset, Rule Law, ref MArray<Point> legal, ref byte depthToBeginWith, byte depth, ref byte colour, ref byte type)
	{
		// Log.Print(origin, offset, depth);
		// If out of bounds, exit. If depth limit reached, also exit.
		if (origin > 89 || origin < 0 || depth == 0)
			return;

		legal.Push(Board.At(origin));

		// If reached another qi, exit.
		if (board.QiIsNotNone(origin, out byte qi) && depth != depthToBeginWith)
		{
			// Do not include this qi if it is the same colour.
			if (Qi.Colour(qi) == colour)
			{
				legal.TopPop();
			}

			return;
		}

		if (Law.Invoke(origin, offset, colour, board))
		{
			return;
		}

		if (AttemptedTraversalToFurtherLeftOrRight(origin, origin + offset))
		{
			return;
		}

		Traverse(ref board, origin + offset, offset, Law, ref legal, ref depthToBeginWith, (byte)(depth - 1), ref colour, ref type);
	}

	static bool AttemptedTraversalToFurtherLeftOrRight(int origin, int afterOffset)
	{
		// For any case if origin is not on the left side.
		if (origin % 9 != 0)
		{
			// If the origin was on the right of the board ( % 9 == 8 ) &&
			// the offset will put origin on the left of the board, exit.
			if (origin % 9 == 8 && afterOffset % 9 == 0)
			{
				return true;
			}
		}
		else // For any case if the origin is on the left side.
		{
			// If origin is on the left, and taking the offset + 1 will put the origin back on the left, exit.
			if ((afterOffset + 1) % 9 == 0)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>If the given offset of origin will be an illegal traversal. O(1).</summary>
	/// <param name="origin">Where to begin.</param>
	/// <param name="offset">The direction to check for legality.</param>
	/// <returns>True if the offset on origin is illegal.</returns>
	static bool SlidingRulesFailed(int origin, int offset, byte colour, Board board)
	{
		int afterOffset = origin + offset;

		// Do not go above or below the board.
		return afterOffset > 89 || afterOffset < 0;
	}

	static bool JerngRulesFailed(int origin, int offset, byte colour, Board board)
	{
		int afterOffset = origin + offset;

		return (colour == Qi.R ? afterOffset > 44 : afterOffset < 45) || AttemptedTraversalToFurtherLeftOrRight(origin, afterOffset);
	}

	static bool TRulesFailed(int origin, int offset, byte colour, Board board)
	{
		int afterOffset = origin + offset;

		// Hard-code illegal moves as T for both colours.
		return !TLegality.Contains(afterOffset);
	}

	static bool XutRulesFailed(int origin, int offset, byte colour, Board board)
	{
		if (colour == Qi.R)
		{
			if (offset == -9)
			{
				return true;
			}

			if (origin < 45)
			{
				return offset == -1 || offset == 1;
			}
		}
		else
		{
			if (offset == 9)
			{
				return true;
			}

			if (origin > 44)
			{
				return offset == -1 || offset == 1;
			}
		}

		return false;
	}

	static bool GoongRulesFailed(int origin, int offset, byte colour, Board board)
	{
		int afterOffset = origin + offset;

		return !GoongLegality.Contains(afterOffset);
	}

	static void TraversePaow(ref Board board, int origin, int[] offsets, ref MArray<Point> legal, ref byte colour)
	{
		foreach (int offset in offsets)
		{
			TraversePaowLogic(ref board, origin, offset, origin, 10, ref legal, ref colour);
		}
	}

	static void TraversePaowLogic(ref Board board, int origin, int offset, int originOrigin, byte depth, ref MArray<Point> legal, ref byte colour)
	{
		if (origin > 89 || origin < 0 || depth == 0)
			return;

		if (board.QiIsNotNone(origin, out byte _) && origin != originOrigin)
		{
			int tempOrigin = origin;
			bool bFoundSomething = false;

			do
			{
				tempOrigin += offset;

				if (tempOrigin > 89 || tempOrigin < 0)
					break;

				if (board.QiIsNotNone(tempOrigin, out byte qi))
				{
					if (Qi.Colour(qi) != colour)
					{
						legal.Push(Board.At(tempOrigin));
					}

					bFoundSomething = true;
				}

				if (AttemptedTraversalToFurtherLeftOrRight(tempOrigin, tempOrigin + offset))
					break;

			} while (!bFoundSomething);

			return;
		}

		legal.Push(Board.At(origin));

		if (AttemptedTraversalToFurtherLeftOrRight(origin, origin + offset))
			return;

		TraversePaowLogic(ref board, origin + offset, offset, originOrigin, --depth, ref legal, ref colour);
	}

	static void TraverseMa(ref Board board, int origin, int[] offsets, ref MArray<Point> legal, ref byte colour)
	{
		// A Ma can be blocked from going in any cardinal direction if there is a qi blocking that direction.
		HashSet<int> blocked = new HashSet<int>();
		for (int i = 0; i < Straight.Length; ++i)
		{
			int afterStraightOffset = origin + Straight[i];

			// Do not go above or below the board.
			if (afterStraightOffset > 89 || afterStraightOffset < 0)
				continue;

			// If there is a qi blocking Straight[i] direction, mark those offsets as blocked.
			if (board.QiIsNotNone(afterStraightOffset, out byte _))
			{
				blocked.Add(Ma[i * 2]);
				blocked.Add(Ma[i * 2 + 1]);
			}
		}

		// If this Ma is one away from the left or right from the board, stop if from going further left or right.
		if (origin % 9 == 1)
		{
			if (!blocked.Contains(Ma[4]))
				blocked.Add(Ma[4]);
			if (!blocked.Contains(Ma[5]))
				blocked.Add(Ma[5]);
		}
		else if (origin % 9 == 7)
		{
			if (!blocked.Contains(Ma[0]))
				blocked.Add(Ma[0]);
			if (!blocked.Contains(Ma[1]))
				blocked.Add(Ma[1]);
		}

		// Register this Ma's position as legal, so that Player.Move Move works.
		legal.Push(Board.At(origin));

		foreach (int offset in offsets)
		{
			if (blocked.Contains(offset))
				continue;

			int afterOffset = origin + offset;

			if (afterOffset > 89 || afterOffset < 0)
				continue;

			if (board.QiIsNotNone(afterOffset, out byte qi))
				if (Qi.Colour(qi) == colour)
					continue;

			legal.Push(Board.At(afterOffset));
		}
	}
}
