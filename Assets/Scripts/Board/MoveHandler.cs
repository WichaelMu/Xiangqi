using MW;

public class MoveHandler
{
	delegate void Process(int origin, int[] offset, ref MArray<Point> legal);

	static readonly int[] Straight = new int[] { 1, 9, -1, -9 };
	static readonly int[] Diagonal = new int[] { 10, 8, -10, -8 };
	static readonly int[] Ma = new int[] { 19, 17, -19, -17 };
	static readonly int[] Jerng = new int[] { 20, 16, -20, -16 };

	public static void Handle(byte qi)
	{
		byte colour = Qi.Colour(qi);
		byte type = Qi.Type(qi);

		if (Qi.IsSlidingQi(qi))
		{

		}
	}

	static void SlidingtQi(int origin)
	{
		MArray<Point> pseudoLegalMoves = TraverseInitialise(origin, Straight);
	}

	static MArray<Point> TraverseInitialise(int origin, int[] offsets, bool ignoreLast = false)
	{
		MArray<Point> legal = new MArray<Point>();

		int range = offsets.Length;

		if (ignoreLast)
			range--;

		for (int i = 0; i < range; ++i)
		{
			Traverse(origin, offsets[i], ref legal);
		}

		return legal;
	}

	static void Traverse(int origin, int offset, ref MArray<Point> legal)
	{
		if (origin > 89 || origin < 0)
		{
			return;
		}

		if (Board.board[origin].GetQi() != Qi.None)
		{
			return;
		}

		legal.Push(Board.board[origin]);

		Traverse(origin + offset, offset, ref legal);
	}

	static void TraverseMa(int origin)
	{

	}
}
