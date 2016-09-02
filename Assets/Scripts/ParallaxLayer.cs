using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour {
	public Vector2 speed;
	public bool tileX;
	public bool tileY;

	private Vector2 startingPos;
	private Vector2 cameraStartingPos;

	void Start() {
		startingPos = transform.position;
		cameraStartingPos = Camera.main.transform.position;

		if (!tileX) return;
		var sprite = GetComponent<SpriteRenderer>();
		for (var m = -10; m <= 10; m++) {
			if (m == 0) continue;
			var tilePos = Vector3.right * sprite.bounds.size.x * m;

			var go = new GameObject();
			go.transform.parent = transform;
			go.transform.localPosition = tilePos;

			var childSprite = go.AddComponent<SpriteRenderer>();
			childSprite.sprite = sprite.sprite;
			childSprite.sortingOrder = sprite.sortingOrder;
		}
	}

	void Update() {
		var camDelta = (Vector2)Camera.main.transform.position - cameraStartingPos;
		camDelta.x *= speed.x;
		camDelta.y *= speed.y;

		transform.position = startingPos + camDelta;
	}
}
