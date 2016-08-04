using UnityEngine;
using System.Collections;
using System.Threading;

public class CharacterController : MonoBehaviour
{
	public float speed;
	public float jumpPower;
	public float gravity;
	public SpriteRenderer sprite;
	public Transform groundCheck;

	private new Rigidbody2D rigidbody;
	private Animator animator;

	public bool grounded;
	public float jumpVelocity;

	void Start ()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		animator = sprite.GetComponent<Animator>();
	}
	
	void FixedUpdate ()
	{
		var hit = Physics2D.Linecast(transform.position, groundCheck.position, LayerMask.GetMask("Ground"));
		grounded = jumpVelocity <= 0 && hit.collider != null;
		Debug.DrawLine(transform.position, groundCheck.position, grounded ? Color.green : Color.red);

		var move = new Vector2();
		if (Input.GetKey(KeyCode.Z)) {
			jumpVelocity = jumpPower;
			grounded = false;
		}
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
			animator.SetBool("grounded", true);
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			move += Vector2.right * Time.fixedDeltaTime * speed;
			sprite.flipX = true;
			animator.SetBool("running", true);
		} else if (Input.GetKey(KeyCode.LeftArrow)) {
			move += Vector2.left * Time.fixedDeltaTime * speed;
			sprite.flipX = false;
			animator.SetBool("running", true);
		} else {
			animator.SetBool("running", false);
		}

		rigidbody.MovePosition(rigidbody.position + move);
	}
}
