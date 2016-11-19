using UnityEngine;
using System.Collections;

public class SphereController : MonoBehaviour {

	public static string CONTROLLER_TAG = "GameController";
	public static string ACTIVE = "READY";
	public static string NOT_ACTIVE = "NOT_READY";

	public Main main;
	public static float window_start_time = -.2f;
	public static float window_end_time = .1f;
	public static float visible_start_time = -1f;
	public static float visible_end_time = .1f;
	public static float ring_start_size = 1.5f;
	public static float ring_end_size = 0.5f;
	public static float ring_speed = (ring_end_size - ring_start_size) / (visible_end_time - visible_start_time);
	public static float light_start_size = .8f;
	public static float light_end_size = 0f;
	public static float light_speed = (light_end_size - light_start_size) / (visible_end_time - visible_start_time);

	public float target_time; // ideal hit time, set by overhead
	public string state = NOT_ACTIVE; // current state of the sphere
	public float hit_time; // recorded time that it was hit, if any, local
	public GameObject ring;

	private Light light;
	private float curr_time; // time in local basis

	public Material mat;

	// Use this for initialization
	void Start () {
		curr_time = main.elapsed_time - target_time;
		state = ACTIVE;
		ring = this.gameObject.transform.Find("Ring").gameObject;
		light = GetComponent<Light> ();

		// make the ring look at you
		ring.transform.LookAt(main.head.transform);

		// Fade in
		this.gameObject.GetComponent<FadeObjectInOut>().logInitialFadeSequence = true;
		this.gameObject.GetComponent<FadeObjectInOut>().FadeIn();
		StartCoroutine (FadeOut ());
	}

	IEnumerator FadeOut() {
		yield return new WaitForSeconds (-visible_start_time);
		this.gameObject.GetComponent<FadeObjectInOut> ().FadeOut (window_end_time);
	}

	// Update is called once per frame
	void Update () {
		// update current time
		curr_time = main.elapsed_time - target_time;

		// debugging
		if (curr_time >= 0) {
			MeshRenderer r = GetComponent<MeshRenderer> ();
			r.material = mat;
		}

		// make the ring and light a little smaller
		float ring_scale = ring_speed * (curr_time - visible_start_time) + ring_start_size;
		float light_scale = light_speed * (curr_time - visible_start_time) + light_start_size;
		ring.transform.localScale = new Vector3(ring_scale, ring_scale, 1f);
		light.intensity = light_scale;

		// make the ring look at you
		ring.transform.LookAt(main.head.transform);

		// time is up, pack up and leave
		if (curr_time > visible_end_time) {
			// calculate out a score based off of hit time
			int score = 0;

			// you don't suck and you hit it
			if (!state.Equals(ACTIVE)) {
				if (Mathf.Abs (hit_time) < .025f) {
					score = Main.GOOD_SCORE;
				} else if (Mathf.Abs (hit_time) < .045f) {
					score = Main.OK_SCORE;
				} else if (Mathf.Abs (hit_time) < .1f) {
					score = Main.BAD_SCORE;
				} else {
					score = 0;
				}
			}

			// send score to overhead
			main.addScore(score);

			// kill yourself
			killYourself(score);
		}
	}

	// Plays the appropriate closing animation based on score and destroys itself
	void killYourself(int score) {
		GameObject textObject = (GameObject) Instantiate (main.getDestroyText(score), this.transform.position, Quaternion.identity);
		textObject.GetComponent<TextController> ().main = main;
		Destroy (this.gameObject);
	}

    /*
	// There exists a controller within the bounds of the sphere
	void OnTriggerStay(Collider other) {
		if (!other.tag.Equals (CONTROLLER_TAG))
			return;
		SteamVR_TrackedObject trackedObj = other.gameObject.GetComponentInParent<SteamVR_TrackedObject> ();
		SteamVR_Controller.Device device = SteamVR_Controller.Input ((int)trackedObj.index);
		// Controller inside sphere and trigger has been pressed down (not held down, hopefully)
		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			// we're in the valid timing window
			if (curr_time >= window_start_time && curr_time <= window_end_time) {
				// Sphere hasn't been hit yet, hit it
				if (state.Equals (ACTIVE)) {
					state = NOT_ACTIVE;
					hit_time = curr_time;
				}
			}
		}
	}*/

    void OnTriggerEnter(Collider other)
    {

        if (!other.tag.Equals(CONTROLLER_TAG))
            return;
        SteamVR_TrackedObject trackedObj = other.gameObject.GetComponentInParent<SteamVR_TrackedObject>();
        SteamVR_Controller.Device device = SteamVR_Controller.Input((int)trackedObj.index);
        //device.TriggerHapticPulse(60000);

        // we're in the valid timing window
        if (curr_time >= window_start_time && curr_time <= window_end_time)
        {
            // Sphere hasn't been hit yet, hit it
            if (state.Equals(ACTIVE))
            {
                state = NOT_ACTIVE;
                hit_time = curr_time;
                StartCoroutine(main.LongVibration(0.2f, 1, trackedObj));
            }
        }
    }


}
