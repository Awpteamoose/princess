using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShowCollider : MonoBehaviour {

	public new bool enabled;

	public Vector2 Rotate(Vector2 v, float degrees) {
		float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (sin * tx) + (cos * ty);
		return v;
	}

	void OnDrawGizmos() {
		if (!enabled) return;
		Gizmos.color = Color.green;
		var box = GetComponent<BoxCollider2D>();
		var points = new Vector2[4];
		if (box) {
			var s = box.size * 0.5f;
			points[0] = new Vector2(-s.x, s.y);
			points[1] = new Vector2(s.x, s.y);
			points[2] = new Vector2(s.x, -s.y);
			points[3] = new Vector2(-s.x, -s.y);
		} else {
			var poly = GetComponent<PolygonCollider2D>();
			points = poly.points;
		}

		for (var i = 0; i < points.Length; i++) {
			points[i] = Rotate(points[i], transform.rotation.eulerAngles.z);
			points[i] += (Vector2)transform.position;
		}

		for(var i = 0; i < points.Length - 1; i++)
		{
			Gizmos.DrawLine(points[i], points[i+1]);
		}
		Gizmos.DrawLine(points[3], points[0]);
	}

}
