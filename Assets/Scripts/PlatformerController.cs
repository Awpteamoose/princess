using System;
using UnityEngine;

public class PlatformerController : MonoBehaviour
{
	public float speed;
	public float jumpPower;
	public float gravity;
	public float maxSlope;
	public SpriteRenderer sprite;
	public Rigidbody2D leftSide, rightSide;
	public Rigidbody2D attack;

	private new Rigidbody2D rigidbody;
	private Animator animator;
	private new BoxCollider2D collider;

	private bool grounded;
	private float jumpVelocity, groundedTime, flyingTime;
	private CollisionData col;
	private State state;
	private float attackTime;
	
	[NonSerialized] private int groundMask;
	[NonSerialized] private Vector2 rcStart, rcEnd;

	private enum State {
		IDLE,
		RUN,
		RISE,
		FALL,
		ATTACK
	}

	private void Start () {
		rigidbody = GetComponent<Rigidbody2D>();
		animator = sprite.GetComponent<Animator>();
		collider = GetComponent<BoxCollider2D>();
		groundMask = LayerMask.GetMask("Ground");
	}

	private void CheckCollision(Vector2 side, Vector2 originOffset, float length, out RaycastHit2D hit, ref PlatformData pdata) {
		rcStart = rigidbody.position + originOffset;
		rcEnd = rcStart + side * length;

		hit = Physics2D.Linecast(rcStart, rcEnd, groundMask);
		Debug.DrawLine(rcStart, rcEnd, Color.cyan);
		if (!hit) return;
		pdata = hit.transform.GetComponent<PlatformData>();
	}

	private void SideCollision(Vector2 side, Rigidbody2D sideBody, ref bool constrained, out RaycastHit2D collision, ref PlatformData pdata) {
		var results = new RaycastHit2D[20];
		var hits = sideBody.Cast(side, results, 0);
		var maxPenetration = 0f;

		collision = results[0];

		for (var i = 0; i < hits; i++) {
			var hit = results[i];

			// Ignore non-ground colliders
			if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;

			var data = hit.transform.GetComponent<PlatformData>();
			// Ignore jump-through platforms in side collisions
			if (data.jumpThrough) continue;

			// Ignore legit slopes
			var angle = Vector2.Angle(Vector2.up, hit.normal);
			if (angle < maxSlope) continue;

			// Only care about the most penetrating collision
			var sideCollider = sideBody.GetComponent<BoxCollider2D>();
			var colliderEdge = sideBody.position.x + sideCollider.offset.x + (side.x * sideCollider.size.x * 0.5f);
			var penetration = Mathf.Abs(colliderEdge - hit.point.x);
			if (penetration < maxPenetration) continue;
			maxPenetration = penetration;

			constrained = true;
			collision = hit;
			pdata = data;
		}
		rigidbody.position += maxPenetration * -side;
	}

	private void FixedUpdate () {
		col.Reset();
		grounded = false;
		var move = new Vector2();

		SideCollision(Vector2.left, leftSide, ref col.left, out col.leftHit, ref col.pLeft);
		SideCollision(Vector2.right, rightSide, ref col.right, out col.rightHit, ref col.pRight);

		CheckCollision(Vector2.up, Vector2.up * collider.size.y * 0.25f, collider.size.y * 0.25f + 0.05f, out col.topHit, ref col.pTop);
		if (col.topHit && !col.pTop.jumpThrough) {
			rigidbody.position += (Vector2.Distance(rcEnd, col.topHit.point) - 0.025f) * - Vector2.down;
		}

		CheckCollision(Vector2.down, Vector2.down * collider.size.y * 0.25f, collider.size.y * 0.25f + 0.05f, out col.bottomHit, ref col.pBottom);
		if (col.bottomHit) {
			var distance = Vector2.Distance(rcEnd, col.bottomHit.point);
			if (!col.pBottom.jumpThrough || (jumpVelocity <= 0 && distance < 0.1f)) {
				rigidbody.position += (distance - 0.05f) * -Vector2.down;
				col.bottom = true;
			}
		}

		if (col.bottom && col.bottomHit) {
			var angle = Vector2.Angle(Vector2.up, col.bottomHit.normal);
			grounded = jumpVelocity <= 0 && angle <= maxSlope;
		}

		if (col.topHit && !col.pTop.jumpThrough) {
			jumpVelocity = Mathf.Min(jumpVelocity, 0);
		} else if (grounded) {
			jumpVelocity = 0;
			if (Input.GetKey(KeyCode.Z) && state != State.ATTACK) {
				jumpVelocity = jumpPower;
				grounded = false;
			}
		}

		if (grounded) {
			if (state != State.ATTACK) {
				state = State.IDLE;
				if (Input.GetKey(KeyCode.X)) {
					state = State.ATTACK;
					attackTime = 0.417f;
					attack.gameObject.SetActive(true);
					attack.transform.localPosition = new Vector3(sprite.flipX ? 0.5f : -0.5f, 0, 0);
				}
			}
			flyingTime = 0;
			groundedTime += Time.fixedDeltaTime;
		} else {
			move += Vector2.up * Time.fixedDeltaTime * jumpVelocity;
			jumpVelocity -= gravity * Time.fixedDeltaTime;
			if (state != State.ATTACK)
				state = jumpVelocity > 0 ? State.RISE : State.FALL;
			groundedTime = 0;
			flyingTime += Time.fixedDeltaTime;
		}

		if (state != State.ATTACK) {
			if (Input.GetKey(KeyCode.RightArrow)) {
				if (!col.right)
					move += Vector2.right * Time.fixedDeltaTime * speed;
				sprite.flipX = true;
				if (state == State.IDLE)
					state = State.RUN;
			} else if (Input.GetKey(KeyCode.LeftArrow)) {
				if (!col.left)
					move += Vector2.left * Time.fixedDeltaTime * speed;
				sprite.flipX = false;
				if (state == State.IDLE)
					state = State.RUN;
			}
		} else {
			attackTime -= Time.fixedDeltaTime;
			if (attackTime <= 0) {
				state = State.IDLE;
				attack.gameObject.SetActive(false);
			}

			var results = new RaycastHit2D[20];
			var hits = attack.Cast(sprite.flipX ? Vector2.right : Vector2.left, results, 0);
		}

		rigidbody.MovePosition(rigidbody.position + move);
		animator.SetInteger("state", (int)state);
	}

	private struct CollisionData {
		public bool left, right, top, bottom;
		public PlatformData pLeft, pRight, pTop, pBottom;
		public RaycastHit2D leftHit, rightHit, topHit, bottomHit;

		public void Reset() {
			left = false;
			right = false;
			top = false;
			bottom = false;

			pLeft = null;
			pRight = null;
			pTop = null;
			pBottom = null;
		}
	}
}
