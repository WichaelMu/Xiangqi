using System.Collections;
using UnityEngine;
using MW;
using MW.IO;
using MW.Diagnostics;

public class Player : MonoBehaviour
{
	Camera _camera;

#pragma warning disable IDE0044
	[SerializeField] Board board;
	[SerializeField] BoardUI boardUI;
#pragma warning restore IDE0044

	float inverseScalar = 1;

	MArray<Point> legalMoves;
	bool bHasQiSelected = false;

	void Start()
	{
		_camera = Camera.main;

		inverseScalar = board.InverseScalar;
	}

	void Update()
	{
		if (!bHasQiSelected)
		{
			if (IsPointUnderMouseValid(out Point pointUnderMouse))
			{
				legalMoves = MoveHandler.Handle(board, pointUnderMouse);
				boardUI.HighlightIntersections(legalMoves);

				bHasQiSelected = true;
			}
		}
		else
		{
			if (IsPointUnderMouseValid(out Point pointUnderMouse))
			{
				if (legalMoves.Contains(pointUnderMouse) && pointUnderMouse != legalMoves[0])
				{
					Board.RegisterMove(legalMoves[0], pointUnderMouse);
					Move(pointUnderMouse.GetQiTransform(), pointUnderMouse);
				}

				bHasQiSelected = false;
				board.UI.HideHighlightedIntersections();
				legalMoves.Flush();
			}
		}

	}

	/// <param name="pointUnderMouse">The <see cref="out"/> parameter of the <see cref="Point"/> under the Mouse, regardless if there is a <see cref="Point"/></param>
	/// <returns><see cref="true"/> if there is a <see cref="Point"/>under the Mouse. Outs the <see cref="Point"/> under the Mouse if <see cref="true"/>, <see cref="null"/> otherwise.</returns>
	bool IsPointUnderMouseValid(out Point pointUnderMouse)
	{
		if (I.Click(EButton.LeftMouse, false, true))
		{
			Vector2 pos = _camera.ScreenToWorldPoint(Input.mousePosition);
			int xb = NearestScalar(pos.x, board.Scalar);
			int yb = NearestScalar(pos.y, board.Scalar);

			// Mark qi for selection.
			if (xb >= 0 && xb <= 9 && yb >= 0 && yb <= 10)
			{
				pointUnderMouse = Board.At(yb * 9 + xb);
				return true;
			}
		}

		pointUnderMouse = null;
		return false;
	}

	int NearestScalar(float In, float Scalar)
	{
		return Mathf.RoundToInt((int)(System.Math.Round(In * inverseScalar, System.MidpointRounding.AwayFromZero) * Scalar) * inverseScalar);
	}

	#region Movement

	const float kTimeToInterpolate = .25f;
	const float kInverseInterpolationTime = 1 / kTimeToInterpolate;

	public void Move(Transform t, Point final)
	{
		StartCoroutine(MoveTo(t, final));
	}

	public void Move(Transform t, Vector2 final)
	{
		StartCoroutine(MoveTo(t, final));
	}

	IEnumerator MoveTo(Transform qi, Point final)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * kInverseInterpolationTime;
			qi.position = Vector2.Lerp(qi.position, final.Position, t);
			yield return null;
		}
	}

	IEnumerator MoveTo(Transform qi, Vector2 final)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * kInverseInterpolationTime;
			qi.position = Vector2.Lerp(qi.position, final, t);
			yield return null;
		}
	}

	#endregion
}
