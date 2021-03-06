﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class ExamplePongLogic : MonoBehaviour {

	public Rigidbody2D racketLeft;
	public Rigidbody2D racketRight;
	public Rigidbody2D ball;
	public Renderer profilePicturePlaneRenderer_left;
	public Renderer profilePicturePlaneRenderer_right;
	public SpriteRenderer logo;

	public float ballSpeed = 10f;
	public Text uiText;
	public Text uiTextDebug;
	private int scoreRacketLeft = 0;
	private int scoreRacketRight = 0;
	void OnGUI() {
		if (GUILayout.Button("Press Me"))
			Debug.Log("Hello!");
	}

	void Awake () {
		AirConsole.instance.onMessage += OnMessage;
		AirConsole.instance.onConnect += OnConnect;
		AirConsole.instance.onDisconnect += OnDisconnect;
		AirConsole.instance.onDeviceProfileChange += OnDeviceProfileChange;
		uiTextDebug.text = "Connecting... \n \n";


	}
		
	/// <summary>
	/// We start the game if 2 players are connected and the game is not already running (activePlayers == null).
	/// 
	/// NOTE: We store the controller device_ids of the active players. We do not hardcode player device_ids 1 and 2,
	///       because the two controllers that are connected can have other device_ids e.g. 3 and 7.
	///       For more information read: http://developers.airconsole.com/#/guides/device_ids_and_states
	/// 
	/// </summary>
	/// <param name="device_id">The device_id that connected</param>
	void OnConnect (int device_id) {
		if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0) {
			if (AirConsole.instance.GetControllerDeviceIds ().Count >= 2) {
				StartGame ();
			} else {
				uiText.text = "NEED MORE PLAYERS";
			}
		}
	}

	/// <summary>
	/// If the game is running and one of the active players leaves, we reset the game.
	/// </summary>
	/// <param name="device_id">The device_id that has left.</param>
	void OnDisconnect (int device_id) {
		int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
		if (active_player != -1) {
			if (AirConsole.instance.GetControllerDeviceIds ().Count >= 2) {
				StartGame (scoreRacketLeft,scoreRacketRight);
			} else {
				AirConsole.instance.SetActivePlayers (0);
				ResetBall (false);
				uiText.text = "PLAYER LEFT - NEED MORE PLAYERS";
			}
		}
	}

	/// <summary>
	/// We check which one of the active players has moved the paddle.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="data">Data.</param>
	void OnMessage (int device_id, JToken data) {
		int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
		if (active_player != -1) {
			if (active_player == 0) {
				this.racketLeft.velocity = Vector3.up * (float)data ["move"];
			}
			if (active_player == 1) {
				this.racketRight.velocity = Vector3.up * (float)data ["move"];
			}
		}
	}
	void ShowLogo () {
		logo.enabled = true;
	}
	void RemoveLogo () {
		logo.enabled = false;
	}
	void StartGame (int scoreRacketLeft = 0, int scoreRacketRight = 0) {
		//ShowLogo ();
		AirConsole.instance.SetActivePlayers (2);
		UpdateScoreUI ();
		DisplayProfilePictureOfFirstController ();
		DisplayProfilePictureOfSecondController ();
		RemoveLogo ();
		ResetBall (true);
	}

	void ResetBall (bool move) {
		
		// place ball at center
		this.ball.position = Vector3.zero;
		
		// push the ball in a random direction
		if (move) {
			Vector3 startDir = new Vector3 (Random.Range (-1, 1f), Random.Range (-0.1f, 0.1f), 0);
			this.ball.velocity = startDir.normalized * this.ballSpeed;
		} else {
			this.ball.velocity = Vector3.zero;
		}
	}

	void UpdateScoreUI () {
		// update text canvas
		uiText.text = scoreRacketLeft + ":" + scoreRacketRight;
		//Scene scene = SceneManager.GetActiveScene();

		//Debug.Log("Active scene is '" + scene.name + "'.");
//		if (scoreRacketLeft > 2 ) {
//			StartGame ();
//		}

	}
	private IEnumerator DisplayUrlPicture (string url, Renderer profilePicturePlaneRenderer ) {
		// Start a download of the given URL
		WWW www = new WWW (url);

		// Wait for download to complete
		yield return www;

		// assign texture
		profilePicturePlaneRenderer.material.mainTexture = www.texture;
		Color color = Color.white;
		color.a = 1;
		profilePicturePlaneRenderer.material.color = color;

		//yield return new WaitForSeconds (3.0f);

		//color.a = 0;
		//profilePicturePlaneRenderer.material.color = color;

	}

	public void DisplayProfilePictureOfFirstController () {
		//We cannot assume that the first controller's device ID is '1', because device 1 
		//might have left and now the first controller in the list has a different ID.
		//Never hardcode device IDs!		
		int idOfFirstController = AirConsole.instance.GetControllerDeviceIds () [0];

		string urlOfProfilePic = AirConsole.instance.GetProfilePicture (idOfFirstController, 512);
		//Log url to on-screen Console
		Debug.Log ("URL of Profile Picture of first Controller: " + urlOfProfilePic + "\n \n");
		StartCoroutine (DisplayUrlPicture (urlOfProfilePic, profilePicturePlaneRenderer_left ));
	}

	public void DisplayProfilePictureOfSecondController () {
		//We cannot assume that the first controller's device ID is '1', because device 1 
		//might have left and now the first controller in the list has a different ID.
		//Never hardcode device IDs!		
		int idOfSecondController = AirConsole.instance.GetControllerDeviceIds () [1];

		string urlOfProfilePic = AirConsole.instance.GetProfilePicture (idOfSecondController, 512);
		//Log url to on-screen Console
		Debug.Log ("URL of Profile Picture of Second Controller: " + urlOfProfilePic + "\n \n");
		StartCoroutine (DisplayUrlPicture (urlOfProfilePic, profilePicturePlaneRenderer_right ));
	}
	void FixedUpdate () {

		// check if ball reached one of the ends
		if (this.ball.position.x < -9f) {
			scoreRacketRight++;
			UpdateScoreUI ();
			ResetBall (true);
		}

		if (this.ball.position.x > 9f) {
			scoreRacketLeft++;
			UpdateScoreUI ();
			ResetBall (true);
		}
	}

	void update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Time.timeScale = 0;
			Application.LoadLevelAdditive ("menu");
			}
	}
	void score (GameObject score) {
		if(score.name == "ScoreRight")
			{
				scoreRacketRight++;
				UpdateScoreUI ();
				ResetBall (true);

			}
			if(score.name == "ScoreLeft")
			{
				scoreRacketLeft++;
				UpdateScoreUI ();
				ResetBall (true);

			}

		}
	

	void OnDestroy () {

		// unregister airconsole events on scene change
		if (AirConsole.instance != null) {
			AirConsole.instance.onMessage -= OnMessage;
		}
	}
	void OnDeviceProfileChange (int device_id) {
		//Log to on-screen Console
		uiTextDebug.text = uiTextDebug.text.Insert (0, "Device " + device_id + " made changes to its profile. \n \n");
	}
}
