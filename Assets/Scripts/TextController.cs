using UnityEngine;
using System.Collections;

public class TextController : MonoBehaviour {

	public float delay;
	public float fadeOutTime;
	private FadeObjectInOut fader;
	public Main main;

	// Use this for initialization
	void Start () {
		//Renderer r = this.gameObject.GetComponent<Renderer> ();
		//r.material.SetColor ("_Color", new Color (0, 0, 0, 0));
		fader = this.gameObject.GetComponent<FadeObjectInOut> ();
		fader.logInitialFadeSequence = true;
		fader.FadeIn ();
		StartCoroutine (FadeOut());
	}

	IEnumerator FadeOut() {
		yield return new WaitForSeconds (delay);
		fader.FadeOut (fadeOutTime);
		Destroy (this.gameObject, fadeOutTime);
	}
	
	// Update is called once per frame
	void Update () {
		transform.forward = main.head.transform.forward;
	}
}
