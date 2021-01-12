using UnityEngine;

public interface IDestructable
{
	uint MaxHp {get; set;}
	float Hp {get; set;}
	
	void TakeDamage (float amount);
	void Death ();
}