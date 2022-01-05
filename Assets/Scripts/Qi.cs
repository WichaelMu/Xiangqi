

public class Qi
{
	public const byte None = 0b00000000;

	/*
	 * 0 0 0 0 0 0 0 0
	 *		 | Only moveable by 1.
	 *	       | Variant of a qi.
	 *	     | Is a 'Straight' qi.
	 *	   | Can move diagonally.
	 *	 | Ma.
	 *     | Red.
	 *   | Green.
	 * | Discard.
	 */

	public const byte Goong			= 0b00000001;
	public const byte Xut			= 0b00000011;
	public const byte XutOrGoongMask	= Xut & Goong;

	public const byte Goo			= 0b00000100;
	public const byte Paow			= 0b00000110;
	public const byte GooOrPaowMask		= Goo & Paow;

	public const byte Ma			= 0b00010000;

	public const byte T			= 0b00001000;
	public const byte Jerng			= 0b00001010;
	public const byte TOrJerngMask		= T & Jerng;

	public const byte R			= 0b00100000;
	public const byte G			= 0b01000000;

	const byte ColourMask			= 0b1100000;
	const byte TypeMask			= 0b0011111;

	public static byte Type(byte In)
	{
		return (byte)(In & TypeMask);
	}

	public static byte Colour(byte In)
	{
		return (byte)(In & ColourMask);
	}

	public static bool IsXutOrGoong(byte In)
	{
		return (~In & XutOrGoongMask) == 0;
	}

	public static bool IsSlidingQi(byte In)
	{
		return (In & GooOrPaowMask) == GooOrPaowMask;
	}

	public static bool IsDiagonalQi(byte In)
	{
		return (In & TOrJerngMask) == TOrJerngMask;
	}

	public static bool IsMa(byte In)
	{
		return (In & Ma) == Ma;
	}
}
