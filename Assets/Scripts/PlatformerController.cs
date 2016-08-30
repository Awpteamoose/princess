using System;
using UnityEngine;
using System.Collections;
using System.Threading;

public class PlatformerController : MonoBehaviour
{
	public float speed;
	public float jumpPower;
	public float gravity;
	public float maxSlope;
	public SpriteRenderer sprite;

	private new Rigidbody2D rigidbody;
	private Animator animator;
	private new BoxCollider2D collider;

	private bool grounded;
	private float jumpVelocity, groundedTime, flyingTime;
	private CollisionData col;
	private PointsOfInterest p;
	
	[NonSerialized] private int groundMask;
	[NonSerialized] private Vector2 rcStart, rcEnd;

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

	private void SideCollision(Vector2 side, Vector2 originOffset, ref bool constrained, out RaycastHit2D hit, ref PlatformData pdata) {
		CheckCollision(side, originOffset, (p.width * 0.5f) + 0.05f, out hit, ref pdata);
		if (!hit || pdata.jumpThrough) return;
		rigidbody.position += (Vector2.Distance(rcEnd, hit.point) - 0.025f) * -side;
		var angle = Vector2.Angle(Vector2.up, hit.normal);
		constrained = angle > maxSlope;
	}

	private void FixedUpdate () {
		p.Acquire(collider);
		col.Reset();
		grounded = false;
		var move = new Vector2();

		SideCollision(Vector2.left, p.topMiddle, ref col.left, out col.leftHit, ref col.pLeft);
		SideCollision(Vector2.left, p.topMiddle * 0.5f, ref col.left, out col.leftHit, ref col.pLeft);
		SideCollision(Vector2.left, Vector2.zero, ref col.left, out col.leftHit, ref col.pLeft);
		SideCollision(Vector2.left, -p.topMiddle * 0.5f, ref col.left, out col.leftHit, ref col.pLeft);

		SideCollision(Vector2.right, p.topMiddle, ref col.right, out col.rightHit, ref col.pRight);
		SideCollision(Vector2.right, p.topMiddle * 0.5f, ref col.right, out col.rightHit, ref col.pRight);
		SideCollision(Vector2.right, Vector2.zero, ref col.right, out col.rightHit, ref col.pRight);
		SideCollision(Vector2.right, -p.topMiddle * 0.5f, ref col.right, out col.rightHit, ref col.pRight);

		CheckCollision(Vector2.up, p.topMiddle * 0.5f, (p.height * 0.25f) + 0.05f, out col.topHit, ref col.pTop);
		if (col.topHit && !col.pTop.jumpThrough) {
			rigidbody.position += (Vector2.Distance(rcEnd, col.topHit.point) - 0.025f) * - Vector2.down;
		}

		CheckCollision(Vector2.down, p.bottomMiddle * 0.5f, (p.height * 0.25f) + 0.05f, out col.bottomHit, ref col.pBottom);
		if (col.bottomHit && (!col.pBottom.jumpThrough || jumpVelocity <= 0))
			rigidbody.position += (Vector2.Distance(rcEnd, col.bottomHit.point) - 0.05f) * - Vector2.down;

		if (col.bottomHit) {
			var angle = Vector2.Angle(Vector2.up, col.bottomHit.normal);
			grounded = jumpVelocity <= 0 && angle <= maxSlope;
		}

		if (col.topHit && !col.pTop.jumpThrough) {
			jumpVelocity = Mathf.Min(jumpVelocity, 0);
		} else if (grounded) {
			jumpVelocity = 0;
			if (Input.GetKey(KeyCode.Z)) {
				jumpVelocity = jumpPower;
				grounded = false;
			}
		}

		if (grounded) {
			animator.SetBool("grounded", true);
			flyingTime = 0;
			groundedTime += Time.fixedDeltaTime;
		} else {
			move += Vector2.up * Time.fixedDeltaTime * jumpVelocity;
			jumpVelocity -= gravity * Time.fixedDeltaTime;
			animator.SetBool("rising", jumpVelocity > 0);
			animator.SetBool("grounded", false);
			groundedTime = 0;
			flyingTime += Time.fixedDeltaTime;
		}

		if (Input.GetKey(KeyCode.RightArrow)) {
			if (!col.right)
				move += Vector2.right * Time.fixedDeltaTime * speed;
			sprite.flipX = true;
			animator.SetBool("running", true);
		} else if (Input.GetKey(KeyCode.LeftArrow)) {
			if (!col.left)
				move += Vector2.left * Time.fixedDeltaTime * speed;
			sprite.flipX = false;
			animator.SetBool("running", true);
		} else {
			animator.SetBool("running", false);
		}

		rigidbody.MovePosition(rigidbody.position + move);
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

	private struct PointsOfInterest {
		public Vector2 topLeft, topMiddle, topRight, rightMiddle, bottomRight, bottomMiddle, bottomLeft, leftMiddle;
		public float width, height;

		public void Acquire(BoxCollider2D collider) {
			width = collider.size.x;
			height = collider.size.y;

			topMiddle = Vector2.up * height * 0.5f;
			rightMiddle = Vector2.right * width * 0.5f;

			bottomMiddle = -topMiddle;
			leftMiddle = -rightMiddle;

			topLeft = topMiddle + leftMiddle;
			topRight = topMiddle + rightMiddle;

			bottomLeft = bottomMiddle + leftMiddle;
			bottomRight = bottomMiddle + rightMiddle;
		}
	}
}
