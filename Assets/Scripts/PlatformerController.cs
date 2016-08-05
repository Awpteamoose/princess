using UnityEngine;
using System.Collections;
using System.Threading;

public class PlatformerController : MonoBehaviour
{
	public float speed;
	public float jumpPower;
	public float gravity;
	public SpriteRenderer sprite;
	public Transform groundCheck;

	private new Rigidbody2D rigidbody;
	private Animator animator;

	[ReadOnly] public bool grounded;
	[ReadOnly] public bool leanRight;
	[ReadOnly] public bool leanLeft;
	[ReadOnly] public float jumpVelocity;

	void Start ()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		animator = sprite.GetComponent<Animator>();
	}
	
	void FixedUpdate ()
	{
		var hit = Physics2D.Linecast(transform.position, groundCheck.position, LayerMask.GetMask("Ground"));
		grounded = jumpVelocity <= 0 && hit.collider != null;
		//Debug.DrawLine(transform.position, groundCheck.position, grounded ? Color.green : Color.red);

		var move = new Vector2();
		if (!grounded) {
			move += Vector2.up * Time.fixedDeltaTime * jumpVelocity;
			jumpVelocity -= gravity * Time.fixedDeltaTime;
			if (jumpVelocity > 0)
				animator.SetBool("rising", true);
			else
				animator.SetBool("rising", false);
			animator.SetBool("grounded", false);
		}
		else {
			jumpVelocity = 0;
			if (Input.GetKey(KeyCode.Z)) {
				jumpVelocity = jumpPower;
				grounded = false;
			}
			rigidbody.position = hit.point - Vector2.up * 0.15f - (Vector2)groundCheck.localPosition;
			animator.SetBool("grounded", true);
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			if (!leanRight)
				move += Vector2.right * Time.fixedDeltaTime * speed;
			sprite.flipX = true;
			animator.SetBool("running", true);
		} else if (Input.GetKey(KeyCode.LeftArrow)) {
			if (!leanLeft)
				move += Vector2.left * Time.fixedDeltaTime * speed;
			sprite.flipX = false;
			animator.SetBool("running", true);
		} else {
			animator.SetBool("running", false);
		}

		rigidbody.MovePosition(rigidbody.position + move);

		leanRight = false;
		leanLeft = false;
		// Actually returns legit and pretty accurate collision points,
		// but I should do my own raycasts on these points to figure out what to do
		var results = new RaycastHit2D[20];
		var hits = rigidbody.Cast(Vector2.down, results, 0);
		for (var i = 0; i < hits; i++) {
			//Debug.DrawLine(transform.position, results[i].point, Color.blue);
			var normal = (results[i].point - rigidbody.position).normalized;
			if (Vector2.Dot(normal, Vector2.up) > 0.9f)
				jumpVelocity = Mathf.Min(jumpVelocity, 0);

			var dot = Vector2.Dot(normal, Vector2.right);
			leanRight = dot > 0.5f;
			leanLeft = dot < -0.5f;
		}
	}
}
