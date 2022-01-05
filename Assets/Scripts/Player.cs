using UnityEngine;
using MW.General;
using MW.IO;

public class Player : MonoBehaviour
{
	Camera _camera;

	[SerializeField] Board board;

	float inverseScalar = 1;

	void Start()
	{
		_camera = Camera.main;

		inverseScalar = board.InverseScalar;
	}

	void Update()
	{
		Vector2 pos = _camera.ScreenToWorldPoint(Input.mousePosition);

		if (CheckValidClick(pos, out Point cursorPoint))
		{
			byte cursorQi = cursorPoint.GetQi();

			MoveHandler.Handle(cursorQi);
		}
	}

	bool CheckValidClick(Vector2 pos, out Point cursorPoint)
	{
		int xb = Mathf.RoundToInt(NearestScalar(pos.x, board.Scalar) * inverseScalar);
		int yb = Mathf.RoundToInt(NearestScalar(pos.y, board.Scalar) * inverseScalar);

		if (I.Click(MW.EButton.LeftMouse, false, true))
		{
			// Mark qi for selection.
			if (xb >= 0 && xb <= 9 && yb >= 0 && yb <= 10)
			{
				cursorPoint = Board.board[yb * 9 + xb];
				return true;
			}
		}

		cursorPoint = null;
		return false;
	}

	int NearestScalar(float In, float Scalar)
	{
		return (int)(System.Math.Round(In * inverseScalar, System.MidpointRounding.AwayFromZero) * Scalar);
	}
}
