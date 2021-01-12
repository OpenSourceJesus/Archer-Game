using UnityEngine;

public interface ICollisionHandler
{
	void OnCollisionEnter2DHandler (Collision2D coll);
	void OnTriggerEnter2DHandler (Collider2D other);
	void OnCollisionStay2DHandler (Collision2D coll);
	void OnTriggerStay2DHandler (Collider2D other);
	void OnCollisionExit2DHandler (Collision2D coll);
	void OnTriggerExit2DHandler (Collider2D other);
}