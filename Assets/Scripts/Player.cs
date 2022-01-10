using System.Collections;
using UnityEngine;
using MW;
using MW.IO;
using MW.Easing;
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

	byte currentPlayer = Qi.R;

	[Header("Capture Settings")]
	[SerializeField] float capturedPositionsOffsetFromCentre = 10;
	[SerializeField] Transform capturedQisGre;
	[SerializeField] Transform capturedQisRed;
	[SerializeField] Vector3 capturedQisOffset;

	void Start()
	{
		_camera = Camera.main;

		inverseScalar = board.InverseScalar;

		float boardHeight = board.Scalar * 9;
		capturedQisGre.position = new Vector2(-capturedPositionsOffsetFromCentre, boardHeight);
		capturedQisRed.position = new Vector2(board.Scalar * 8 + capturedPositionsOffsetFromCentre, boardHeight);
	}

	void Update()
	{
		// No qi selected.
		if (!bHasQiSelected)
		{
			// Clicked on a valid Point.
			if (IsPointUnderMouseValid(out Point pointUnderMouse))
			{
				byte qiColour = Qi.Colour(pointUnderMouse.GetQiAsByte());

				if (qiColour == currentPlayer)
				{
					legalMoves = MoveHandler.Handle(board, pointUnderMouse);
					boardUI.HighlightIntersections(legalMoves);

					bHasQiSelected = true;
				}
			}
		}
		else // Has a qi selected.
		{
			// Clicked on a valid Point. Legal defined by legalMoves.
			if (IsPointUnderMouseValid(out Point pointUnderMouse))
			{
				// A move is made here.
				if (legalMoves.Contains(pointUnderMouse) && pointUnderMouse != legalMoves[0])
				{
					if (board.QiIsNotNone(pointUnderMouse.Index, out byte _))
					{
						if (currentPlayer == Qi.R)
						{
							Move(pointUnderMouse.GetQiTransform(), capturedQisRed.position);
							capturedQisRed.position -= capturedQisOffset;
						}
						else
						{
							Move(pointUnderMouse.GetQiTransform(), capturedQisGre.position);
							capturedQisGre.position -= capturedQisOffset;
						}
					}

					// Immediately move the selected qi to the pointUnderMouse. In memory only, not visual.
					Board.RegisterMove(legalMoves[0], pointUnderMouse);

					// Move the selected qi to the pointUnderMouse. Visually only, not memory.
					Move(pointUnderMouse.GetQiTransform(), pointUnderMouse);

					// After everything is done, end the turn.
					EndTurn(ref currentPlayer);
				}

				bHasQiSelected = false;
				board.UI.HideHighlightedIntersections();
				legalMoves.Flush();
			}
		}

	}

	/// <param name="pointUnderMouse">The <see cref="out"/> parameter of the <see cref="Point"/> under the Mouse, regardless if there is a <see cref="Point"/></param>
	/// <returns><see cref="true"/> if there is a <see cref="Point"/> under the Mouse. Outs the <see cref="Point"/> under the Mouse if <see cref="true"/>, otherwise <see cref="null"/>.</returns>
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

	const float kTimeToInterpolate = .55f;
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
		while (t <= 2)
		{
			t += Time.deltaTime * kInverseInterpolationTime;
			qi.position = Vector2.Lerp(qi.position, final.Position, Interpolate.Ease(EEquation.EaseInSine, 0, 1, t));
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

	void EndTurn(ref byte player)
	{
		if (player == Qi.R)
		{
			player = Qi.G;
		}
		else
		{
			player = Qi.R;
		}
	}
}
