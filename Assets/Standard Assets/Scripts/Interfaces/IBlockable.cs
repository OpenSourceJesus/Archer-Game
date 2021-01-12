using UnityEngine;

public interface IBlockable
{
	Renderer Renderer {get; set;}
	Collider2D Collider {get; set;}
}