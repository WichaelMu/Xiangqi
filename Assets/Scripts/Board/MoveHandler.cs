using MW;
using MW.Diagnostics;

public class MoveHandler
{
	delegate void Process(int origin, int[] offset, ref MArray<Point> legal);

	static readonly int[] Straight = new int[] { 1, 9, -1, -9 };
	static readonly int[] Diagonal = new int[] { 10, 8, -10, -8 };
	static readonly int[] Ma = new int[] { 19, 17, -19, -17 };
	static readonly int[] Jerng = new int[] { 20, 16, -20, -16 };

	public static MArray<Point> Handle(Board board, Point origin)
	{
		byte qi = origin.GetQiAsByte();

		if (Qi.IsSlidingQi(qi))
		{
			return SlidingQi(board, origin.Index, qi);
		}

		return new MArray<Point>();
	}

	static MArray<Point> SlidingQi(Board board, int origin, byte relativePiece)
	{
		byte colour = Qi.Colour(relativePiece);
		byte type = Qi.Type(relativePiece);

		MArray<Point> pseudoLegalMoves = TraverseInitialise(board, origin, Straight, ref colour, ref type);

		return pseudoLegalMoves;
	}

	/// <summary>Begins the traversing of legal Points.</summary>
	/// <param name="origin">Where to begin.</param>
	/// <param name="offsets">How to traverse.</param>
	/// <param name="depth">How far this search should go.</param>
	/// <param name="ignoreLastN">How many offsets from the back to ignore. (Going backwards).</param>
	/// <returns>Legal moves.</returns>
	static MArray<Point> TraverseInitialise(Board board, int origin, int[] offsets, ref byte colour, ref byte type, byte depth = 10, int ignoreLastN = 0)
	{
		MArray<Point> legal = new MArray<Point>();

		for (int i = 0; i < offsets.Length - ignoreLastN; ++i)
		{
			Traverse(board, origin, offsets[i], ref legal, depth, ref colour, ref type);
		}

		return legal;
	}

	static void Traverse(Board board, int origin, int offset, ref MArray<Point> legal, byte depth, ref byte colour, ref byte type)
	{
		// Log.Print(origin, offset, depth);
		// If out of bounds, exit. If depth limit reached, also exit.
		if (origin > 89 || origin < 0 || depth == 0)
			return;

		if ((origin + offset) > 90 && origin % 9 != 0)
			return;

		if (!legal.Contains(Board.At(origin)))
			legal.Push(Board.At(origin));

		// If reached another qi, exit.
		if (board.QiIsNotNone(origin, out byte qi) && depth != 10)
		{
			// Do not include this qi if it is the same colour.
			if (Qi.Colour(qi) == colour)
			{
				legal.TopPop();
			}

			return;
		}

		Traverse(board, origin + offset, offset, ref legal, (byte)(depth - 1), ref colour, ref type);
	}
}
