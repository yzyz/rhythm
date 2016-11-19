using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	public class Sphere {
		public float x,y,z,t;
		public Sphere(float x, float y, float z, float t) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.t = t;
		}
	}

	public static int BAD_SCORE = 50;
	public static int OK_SCORE = 100;
	public static int GOOD_SCORE = 300;
	public static string[] colors = new string[] {"Blue", "Green", "Red", "Yellow", "Purple"};
	public static Color purple = new Color(255, 0, 255, 255);
	public static Color[] colorc = new Color[] {Color.blue, Color.green, Color.red, Color.yellow, purple};
	public static string WAITING = "WAITING";
	public static string PLAYING = "PLAYING";

	public GameObject head;
	public GameObject base_sphere;
	public GameObject destroy_text_miss;
	public GameObject destroy_text_bad;
	public GameObject destroy_text_ok;
	public GameObject destroy_text_good;
	public TextAsset level;
	public float start_at_time;
	public float elapsed_time;
	public float intro_offset_time;
	public float bps = 128f / 60f;
	public AudioClip demo_clip;
	public int score;
	public GameObject score_display;

	private AudioSource audioSource;
	private string state = WAITING;
	private float game_start_time;
	private List<Sphere> spheres;
	private int index;

	// Use this for initialization
	void Start () {
		audioSource = this.gameObject.GetComponent<AudioSource> ();
		spheres = new List<Sphere>();
		string[] data = level.text.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
		List<float> floats = new List<float> ();
		for (int i = 0; i < data.Length; i++) {
			float f;
			if (float.TryParse (data [i], out f)) {
				floats.Add (f);
			}
		}
		for (int i = 0; i < floats.Count; i += 4) {
			float x = floats[i];
			float y = floats[i+1];
			float z = floats[i+2];
			float t = floats[i+3] / bps + intro_offset_time;
			spheres.Add (new Sphere (x, y, z, t));
		}
		index = 0;

		// update for start at time
		while (index < spheres.Count && spheres[index].t + SphereController.visible_start_time <= start_at_time) {
			print ("Skipping sphere: " + index);
			index++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (state.Equals (WAITING)) {
			if (Input.GetKeyDown ("space")) {
				game_start_time = Time.time - start_at_time;
				elapsed_time = 0f;
				state = PLAYING;
				audioSource.clip = demo_clip;
				audioSource.time = start_at_time;
				audioSource.Play ();
			}
		} else { // we're playing
			elapsed_time = Time.time - game_start_time;
			float t = elapsed_time - SphereController.visible_start_time;
			while (index < spheres.Count && spheres[index].t <= t) {
				addSphere (spheres [index]);
				index++;
			}
			updateScoreDisplay ();
            print(audioSource.time);
		}
	}

	void addSphere(Sphere s) {
		GameObject sphere = (GameObject) Instantiate (base_sphere, new Vector3 (s.x, s.y, s.z), Quaternion.identity);
		//sphere.active = false;

		// Code initialization
		SphereController sc = sphere.GetComponent<SphereController> ();
		sc.target_time = s.t;
		sc.main = this;

		// Color initialization
		int color_index = Random.Range(0, colors.Length - 1);
		string color = colors[color_index];
		sphere.GetComponent<MeshRenderer>().material = 
			Resources.Load("Materials/" + color + "SphereMaterial", typeof(Material)) as Material;
		sphere.transform.Find("Ring").gameObject.GetComponent<MeshRenderer>().material =
			Resources.Load("Materials/" + color + "RingMaterial", typeof(Material)) as Material;
		sphere.GetComponent<Light> ().color = colorc [color_index];

		//sphere.active = true;
	}

	public GameObject getDestroyText(int score) {
		if (score == 0) {
			return destroy_text_miss;
		} else if (score == BAD_SCORE) {
			return destroy_text_bad;
		} else if (score == OK_SCORE) {
			return destroy_text_ok;
		} else if (score == GOOD_SCORE) {
			return destroy_text_good;
		}
		print ("this should never happen. score: " + score);
		return null;
	}

	public void addScore(int score) {
		this.score += score;
	}

	void updateScoreDisplay() {
		score_display.GetComponent<TextMesh> ().text = score.ToString();
	}

    public IEnumerator LongVibration(float length, float strength, SteamVR_TrackedObject obj)
    {
        for (float i = 0; i < length; i += Time.deltaTime)
        {
            SteamVR_Controller.Input((int)obj.index).TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
            yield return null;
        }
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }
}
