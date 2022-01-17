using System.Collections;
using UnityEngine;
using MW;
using MW.Easing;
using MW.Vector;

public class PlayerUI : MonoBehaviour
{

	[SerializeField] SpriteRenderer previousMoveMarker;
	[SerializeField] Color previousColourFrom, previousColourTo;

	SpriteRenderer sprF;
	SpriteRenderer sprT;

	Player player;

	void Start()
	{
		player = GetComponent<Player>();

		// Spawn previous move markers out of the scene.
		sprF = Instantiate(previousMoveMarker, new MVector(1000), Quaternion.identity);
		sprT = Instantiate(previousMoveMarker, new MVector(1000), Quaternion.identity);

		float previousMarkerScale = player.board.Scalar * .5f;
		previousMoveMarker.transform.localScale = new Vector3(previousMarkerScale, previousMarkerScale, 0) * 1.5f;

		sprF.color = previousColourFrom;
		sprT.color = previousColourTo;
	}

	public void HighlightPreviousMove(Point from, Point to)
	{
		sprF.transform.position = from.Position;
		sprT.transform.position = to.Position;
	}

	#region Movement

	const float kTimeToInterpolate = .55f;
	const float kInverseInterpolationTime = 1 / kTimeToInterpolate;

	/// <summary>Moves a <see cref="Transform"/> to a <see cref="Point.Position"/>.</summary>
	/// <param name="t">The <see cref="Transform"/> to move to final.</param>
	/// <param name="final">The <see cref="Point"/> for t to move to.</param>
	public void Move(Transform t, Point final)
	{
		StartCoroutine(MoveTo(t, final));
	}

	/// <summary>Moves a <see cref="Transform"/> to a <see cref="Vector2"/>.</summary>
	/// <param name="t">The <see cref="Transform"/> to move to final.</param>
	/// <param name="final">The <see cref="Vector2"/> for t to move to.</param>
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
}
