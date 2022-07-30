using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOffSet : MonoBehaviour 
{
	public Material mat;
	public float Xoffset;
	public float Yoffset;
	Vector2 offset;
	public bool playAnimation;
	// Use this for initialization
	void Awake () 
	{
		mat = this.gameObject.GetComponent<Renderer> ().material;
		offset = mat.GetTextureOffset ("_MainTex");

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (playAnimation) {
		

			offset.x += Xoffset * Time.deltaTime;
			offset.y += Yoffset * Time.deltaTime;

			mat.SetTextureOffset ("_MainTex", offset);
		} else 
		{
			return;
		}

		
	}
}
