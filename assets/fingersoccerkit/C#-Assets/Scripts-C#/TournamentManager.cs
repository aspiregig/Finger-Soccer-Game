using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class TournamentManager : MonoBehaviour {

    /// <summary>
    /// Tournament Manager
    /// This class manages all setting for tournament mode including:
    /// team selection, saving and loading tournament status, player advancement, 
    /// win/lose states, etc...
    /// You can grant the player rewards when he/she reaches to the end of the tournament.
    /// </summary>

    public GameObject[] eng_text = new GameObject[10];
    public GameObject[] arab_text = new GameObject[10];

    public static int torunamentLevel;		//starting level. then proceeds to 1, 2 and 3.

	public Texture2D[] availableFlags;		//available flags for teams (11 flag in total)
	public GameObject[] LevelATeams;		//we have 8 teams in total. 1 player and 7 AI opponen. Player Always starts in position 1.
	public GameObject[] LevelBTeams;		//we have 4 winners teams in level B. Player Always starts in position 1.
	public GameObject[] LevelCTeams;		//we have 2 winners teams in level C
	public GameObject[] LevelDTeams;		//we have 1 winner teams in level D

	public GameObject btnStart;
	public GameObject btnStartText;
	public GameObject btnExit;

	public AudioClip tapSfx;					//tap sound for buttons click
	private bool canTap = true;					//flag to prevent double tap
	private float buttonAnimationSpeed = 11;    //speed on animation effect when tapped on button

   

    void Awake () {

		//avoid starting the game in paused mode
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;

		//Init Torunament Settings

		//Get torunament Level
		torunamentLevel = PlayerPrefs.GetInt("TorunamentLevel");

		//if we are starting a new torunament
		if(torunamentLevel == 0) {
			//reset all previous data
			resetTournamentSettings();
			//Set Level A settings
			setLevelASettings();
		}

		if(torunamentLevel == 1) {
			setLevelBSettings();
		}

		if(torunamentLevel == 2) {
			setLevelCSettings();
		}

		if(torunamentLevel == 3) {
			setLevelDSettings();
		}

		//other settings
		canTap = true;
	}


	void Update () {
        language();

		if(canTap) {
			StartCoroutine(tapManager());
		}

		//Debug. Fake tournament advancement
		AdvanceInTournament();

		//Back/Reload
		if(Input.GetKeyDown(KeyCode.R)) {
			//PlayerPrefs.DeleteAll();
			resetTournamentSettings();
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}		
		if(Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("Menu-c#");
		if(Input.GetKeyDown(KeyCode.P))
			printTeams();

	}

    //*****************************************************************************
    // language option
    //*****************************************************************************
    void language()
    {
        if (pubR.language_option == 0)
        {
            for (int i = 0; i < eng_text.Length; i++)
            {
                eng_text[i].SetActive(true);
                arab_text[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < eng_text.Length; i++)
            {
                arab_text[i].SetActive(true);
                eng_text[i].SetActive(false);
            }
        }
    }


    void AdvanceInTournament() {

		if(torunamentLevel >= 3)
			return;

		if(Input.GetKeyDown(KeyCode.W)) {
			PlayerPrefs.SetInt("TorunamentMatchResult", 1);
			PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if(Input.GetKeyDown(KeyCode.L)) {
			PlayerPrefs.SetInt("TorunamentMatchResult", 0);
			PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}


	public void shuffleList(List<int> _List) {
		for (int i = _List.Count - 1; i > 0; i--) {
			int r = Random.Range(0, i);
			int tmp = _List[i];
			_List[i] = _List[r];
			_List[r] = tmp;
		}
	}


	void setLevelASettings() {
		//set player team. Note that index 0 of availableTeams array always refers to human player team
		int playerFlag = PlayerPrefs.GetInt("PlayerFlag", 0);

		//set other 7 AI teams and their flags (and avoid selecting human player team a an AI again)
		List<int> cpuTeamsIndex = new List<int>();
		for(int i = 0; i < 8; i++) {
			if(i != playerFlag)
				cpuTeamsIndex.Add(i);
		}
		
		if(cpuTeamsIndex.Count == 8)
			cpuTeamsIndex.RemoveAt(cpuTeamsIndex.Count - 1);
		
		shuffleList(cpuTeamsIndex);
		
		for(int j = 0; j < 8; j++) {
			if(j == 0) {
				LevelATeams[j].GetComponent<Renderer>().material.mainTexture = availableFlags[playerFlag];
				PlayerPrefs.SetInt("TournamentTeams" + j.ToString(), playerFlag);
			} else {
				LevelATeams[j].GetComponent<Renderer>().material.mainTexture = availableFlags[cpuTeamsIndex[j-1]];
				PlayerPrefs.SetInt("TournamentTeams" + j.ToString(), cpuTeamsIndex[j-1]);
			}
		}

		//set start button text
		btnStartText.GetComponent<TextMesh>().text = "Start";
		btnStart.GetComponent<BoxCollider>().enabled = true;
	}


	void setLevelBSettings() {
		//set team flags again
		for(int i = 0; i < 8; i++) {
			LevelATeams[i].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("TournamentTeams" + i.ToString())];
		}
		
		//construc 3 fake match result for other events in Level A of tournament
		for(int j = 0; j < 4; j++) {
			//override for player
			if(j == 0) {
				PlayerPrefs.SetInt("MatchResultsLevelA" + j.ToString(), PlayerPrefs.GetInt("TorunamentMatchResult"));
			} else {
				float rnd = Random.value;
				if(rnd >= 0.5f)
					PlayerPrefs.SetInt("MatchResultsLevelA" + j.ToString(), 1);
				else
					PlayerPrefs.SetInt("MatchResultsLevelA" + j.ToString(), 0);
			}
		}

		//set group B teams (transfer the winners from Level A to level B
		for(int k = 0; k < 4; k++) {
			if(PlayerPrefs.GetInt("MatchResultsLevelA" + k.ToString()) == 1) 
				PlayerPrefs.SetInt( "WinnersLevelA" + k.ToString(), PlayerPrefs.GetInt("TournamentTeams" + (k*2).ToString()) );
			else
				PlayerPrefs.SetInt( "WinnersLevelA" + k.ToString(), PlayerPrefs.GetInt("TournamentTeams" + ((k*2)+1).ToString()) );

			LevelBTeams[k].GetComponent<Renderer>().enabled = true;
			LevelBTeams[k].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelA" + k.ToString())];
		}

		//set start button text
		if(PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {
			btnStartText.GetComponent<TextMesh>().text = "Continue";
			btnStart.GetComponent<BoxCollider>().enabled = true;
		} else {
			btnStartText.GetComponent<TextMesh>().text = "Exit";
			btnStart.GetComponent<BoxCollider>().enabled = true;
			btnExit.SetActive(false);
		}

	}


	void setLevelCSettings() {
		//set team flags again
		for(int i = 0; i < 8; i++) {
			LevelATeams[i].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("TournamentTeams" + i.ToString())];
		}
		for(int j = 0; j < 4; j++) {
			LevelBTeams[j].GetComponent<Renderer>().enabled = true;
			LevelBTeams[j].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelA" + j.ToString())];
		}

		//construc 1 fake match result for other eventsin Level B of tournament
		for(int k = 0; k < 2; k++) {
			//override for player
			if(k == 0) {
				PlayerPrefs.SetInt("MatchResultsLevelB" + k.ToString(), PlayerPrefs.GetInt("TorunamentMatchResult"));
			} else {
				float rnd = Random.value;
				if(rnd >= 0.5f)
					PlayerPrefs.SetInt("MatchResultsLevelB" + k.ToString(), 1);
				else
					PlayerPrefs.SetInt("MatchResultsLevelB" + k.ToString(), 0);
			}
		}

		//set group C teams (transfer the winners from Level B to level C
		for(int m = 0; m < 2; m++) {
			if(PlayerPrefs.GetInt("MatchResultsLevelB" + m.ToString()) == 1) 
				PlayerPrefs.SetInt( "WinnersLevelB" + m.ToString(), PlayerPrefs.GetInt("WinnersLevelA" + (m*2).ToString()) );
			else
				PlayerPrefs.SetInt( "WinnersLevelB" + m.ToString(), PlayerPrefs.GetInt("WinnersLevelA" + ((m*2)+1).ToString()) );
			
			LevelCTeams[m].GetComponent<Renderer>().enabled = true;
			LevelCTeams[m].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelB" + m.ToString())];
		}


		//set start button text
		if(PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {
			btnStartText.GetComponent<TextMesh>().text = "Continue";
			btnStart.GetComponent<BoxCollider>().enabled = true;
		} else {
			btnStartText.GetComponent<TextMesh>().text = "Exit";
			btnStart.GetComponent<BoxCollider>().enabled = true;
			btnExit.SetActive(false);
		}
		
	}


	void setLevelDSettings() {
		//set team flags again
		for(int i = 0; i < 8; i++) {
			LevelATeams[i].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("TournamentTeams" + i.ToString())];
		}
		for(int j = 0; j < 4; j++) {
			LevelBTeams[j].GetComponent<Renderer>().enabled = true;
			LevelBTeams[j].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelA" + j.ToString())];
		}
		for(int k = 0; k < 2; k++) {
			LevelCTeams[k].GetComponent<Renderer>().enabled = true;
			LevelCTeams[k].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelB" + k.ToString())];
		}

		//declae the winner
		LevelDTeams[0].GetComponent<Renderer>().enabled = true;
		if(PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {

			LevelDTeams[0].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelB0")];

			btnStartText.GetComponent<TextMesh>().text = "Finish";
			btnStart.GetComponent<BoxCollider>().enabled = true;
			btnExit.SetActive(false);

		} else {

			LevelDTeams[0].GetComponent<Renderer>().material.mainTexture = availableFlags[PlayerPrefs.GetInt("WinnersLevelB1")];

			btnStartText.GetComponent<TextMesh>().text = "Oh No!!";
			btnStart.GetComponent<BoxCollider>().enabled = true;
			btnExit.SetActive(false);
		}
	}


	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator tapManager (){
		
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
		
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			
			switch(objectHit.name) {
				
			case "BtnStart":
				playSfx(tapSfx);
				StartCoroutine(animateButton(objectHit));
				yield return new WaitForSeconds(0.5f);
				processStart();
				break;

			case "BtnExit":
				playSfx(tapSfx);
				resetTournamentSettings();
				StartCoroutine(animateButton(objectHit));
				yield return new WaitForSeconds(0.5f);
				SceneManager.LoadScene("Menu-c#");
				break;
			}
		}
	}


	void processStart() {

		//if we are starting the tournament for the first time
		if(torunamentLevel == 0) {
			//set player & AI teams
			PlayerPrefs.SetInt("PlayerFlag", PlayerPrefs.GetInt("TournamentTeams0"));
			PlayerPrefs.SetInt("OpponentFlag", PlayerPrefs.GetInt("TournamentTeams1"));
			PlayerPrefs.SetInt("GameMode", 0); //Player vs AI

			//load the match
			SceneManager.LoadScene("Game-c#");
		}

		//if we have won the first game, we must play another match
		if(torunamentLevel == 1 && PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {
			//set player & AI teams
			PlayerPrefs.SetInt("PlayerFlag", PlayerPrefs.GetInt("WinnersLevelA0"));
			PlayerPrefs.SetInt("OpponentFlag", PlayerPrefs.GetInt("WinnersLevelA1"));
			PlayerPrefs.SetInt("GameMode", 0); //Player vs AI

			SceneManager.LoadScene("Game-c#");
		}

		//if we have won the second game, we must play another match
		if(torunamentLevel == 2 && PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {
			//set player & AI teams
			PlayerPrefs.SetInt("PlayerFlag", PlayerPrefs.GetInt("WinnersLevelB0"));
			PlayerPrefs.SetInt("OpponentFlag", PlayerPrefs.GetInt("WinnersLevelB1"));
			PlayerPrefs.SetInt("GameMode", 0); //Player vs AI
			
			SceneManager.LoadScene("Game-c#");
		}
		
		//if we have won the cup, we should exit the tournament
		if(torunamentLevel == 3 && PlayerPrefs.GetInt("TorunamentMatchResult") == 1) {

			//grant any score, money, bonus, etc, in here...
			//for example, give player (winner) 5000 coin as the prize
			int playerMoney = PlayerPrefs.GetInt("PlayerMoney");
			playerMoney += 5000;
			PlayerPrefs.SetInt("PlayerMoney", playerMoney);

			//reset tournament settings and advancements
			resetTournamentSettings();

			SceneManager.LoadScene("Menu-c#");
		}

		//if we have lost the match, we should exit
		if(torunamentLevel > 0 && torunamentLevel <= 3 && PlayerPrefs.GetInt("TorunamentMatchResult") == 0) {

			//reset tournament settings and advancements
			resetTournamentSettings();

			SceneManager.LoadScene("Menu-c#");
		}
	}


	//*****************************************************************************
	// This function animates a button by modifying it's scales on x-y plane.
	// can be used on any element to simulate the tap effect.
	//*****************************************************************************
	IEnumerator animateButton ( GameObject _btn  ){
		canTap = false;
		Vector3 startingScale = _btn.transform.localScale;	//initial scale	
		Vector3 destinationScale = startingScale * 1.1f;		//target scale
		
		//Scale up
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * buttonAnimationSpeed;
			_btn.transform.localScale = new Vector3( Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
			                                        Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
			                                        _btn.transform.localScale.z);
			yield return 0;
		}
		
		//Scale down
		float r = 0.0f; 
		if(_btn.transform.localScale.x >= destinationScale.x) {
			while (r <= 1.0f) {
				r += Time.deltaTime * buttonAnimationSpeed;
				_btn.transform.localScale = new Vector3( Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
				                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
				                                        _btn.transform.localScale.z);
				yield return 0;
			}
		}
		
		if(r >= 1)
			canTap = true;
	}


	//debug
	void printTeams() {

		//print name + flag index of all teams
		for(int i = 0; i < 8; i++) {
			print ("Team " + i.ToString() + " Flag Index = " + PlayerPrefs.GetInt("TournamentTeams" + i.ToString()) );
		}

		//print name + flag index of level A winners
		for(int j = 0; j < 4; j++) {
			print ("Winners A " + j.ToString() + " Flag Index = " + PlayerPrefs.GetInt("WinnersLevelA" + j.ToString()) );
		}

		//print name + flag index of level B winners
		for(int k = 0; k < 2; k++) {
			print ("Winners B " + k.ToString() + " Flag Index = " + PlayerPrefs.GetInt("WinnersLevelB" + k.ToString()) );
		}
	}


	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	void playSfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}


	void resetTournamentSettings() {

		//We should not delete all keys, except when we are debugging the project. This can ruin your live game.
		//PlayerPrefs.DeleteAll();

		//we could do this more efficiently, but we just want to demonstrate how you should proceed with deleting different keys.
		//you can mix these for loops to have better performance in your final project.

		PlayerPrefs.DeleteKey("TorunamentLevel");
		PlayerPrefs.DeleteKey("TorunamentMatchResult");

		for(int i = 0; i < 8; i++)
			PlayerPrefs.DeleteKey("TournamentTeams" + i.ToString());

		for(int j = 0; j < 4; j++)
			PlayerPrefs.DeleteKey("MatchResultsLevelA" + j.ToString());

		for(int k = 0; k < 4; k++)
			PlayerPrefs.DeleteKey("WinnersLevelA" + k.ToString());

		for(int l = 0; l < 2; l++)
			PlayerPrefs.DeleteKey("MatchResultsLevelB" + l.ToString());

		for(int m = 0; m < 2; m++)
			PlayerPrefs.DeleteKey("WinnersLevelB" + m.ToString());
	}
}
