using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GlobalGameManager : MonoBehaviour {
		
	///*************************************************************************///
	/// Main Game Controller.
	/// This class controls main aspects of the game like rounds, levels, scores and ...
	/// Please note that the game always happens between 2 player: (Player-1 vs Player-2) or (Player-1 vs AI)
	/// Player-2 and AI are the same in some aspects like when they got their turns, but they use different controllers.
	/// Player-2 uses a similar controller as Player-1, while AI uses an artificial intelligent routine to play the game.
	///
	/// Important! All units and ball object inside the game should be fixed at Z=-0.5f positon at all times. 
	/// You can do this with RigidBody's freeze position.
	///*************************************************************************///

	public static string player1Name = "Player_1";
	public static string player2Name = "Player_2";
	public static string cpuName = "CPU";

    public GameObject[] eng_text = new GameObject[10];
    public GameObject[] arab_text = new GameObject[10];

    // Available Game Modes:
    /*
	Indexes:
	0 = 1 player against cpu (normal or Tournament)
	1 = 2 player against each other on the same platform/device
	*/
    public static int gameMode;

	//Odd rounds are player (Player-1) turn and Even rounds are AI (Player-2)'s
	public static int round;

	//available time to think and shoot
	public static float baseShootTime = 15;		//fixed shoot time for all players and AI
	private float p1ShootTime;					//additional time (based on the selected team) for p1
	private float p2ShootTime;					//additional time (based on the selected team) for p2 or AI
	public GameObject p1TimeBar;
	public GameObject p2TimeBar;
	private float p1TimeBarInitScale;
	private float p1TimeBarCurrentScale;
	private float p2TimeBarInitScale;
	private float p2TimeBarCurrentScale;

	//mamixmu distance that players can drag away from selected unit to shoot the ball (is in direct relation with shoot power)
	public static float maxDistance = 3.0f;

	//Turns in flags
	public static bool playersTurn;
	public static bool opponentsTurn;

	//After players did their shoots, the round changes after this amount of time.
	public static float timeStepToAdvanceRound = 3; 

	//Special occasions
	public static bool goalHappened;
	public static bool shootHappened;
	public static bool gameIsFinished;
	public static int goalLimit = 5; //To finish the game quickly, without letting the GameTime end.

	///Game timer vars
	public static float gameTimer; //in seconds
	private string remainingTime;
	private int seconds;
	private int minutes;

	//Game Status
	static int playerGoals;
	static int opponentGoals;
	public static float gameTime; //Main game timer (in seconds). Always fixed.

	//gameObject references
	private GameObject playerAIController;
	private GameObject opponentAIController;
	private GameObject ball;
	public GameObject goalPlane;

	//AudioClips
	public AudioClip startWistle;
	public AudioClip finishWistle;
	public AudioClip[] goalSfx;
	public AudioClip[] goalHappenedSfx;
	public AudioClip[] crowdChants;
	private bool canPlayCrowdChants;

	//Public references
	public GameObject gameStatusPlane;			//user to show win/lose result at the end of match
	public GameObject continueTournamentBtn;	//special tournament button to advance in tournament incase of win
	public GameObject statusTextureObject;
    public GameObject statusTexture_arab;//plane we use to show the result texture in 3d world
    public Texture2D[] statusModes;
    public Texture2D[] statusModes_arab; //Available status textures

    //*****************************************************************************
    // Init. 
    //*****************************************************************************
    void Awake (){	
		//init
		goalHappened = false;
		shootHappened = false;
		gameIsFinished = false;
		playerGoals = 0;
		opponentGoals = 0;
		gameTime = 0;
		round = 1;
		seconds = 0;
		minutes = 0;
		canPlayCrowdChants = true;

		//get additonal time for each player and AI
		p1ShootTime = baseShootTime + TeamsManager.getTeamSettings(PlayerPrefs.GetInt("PlayerFlag")).y;
		p2ShootTime = baseShootTime + TeamsManager.getTeamSettings(PlayerPrefs.GetInt("Player2Flag")).y;
		print ("P1 shoot time: " + p1ShootTime + " // " + "P2 shoot time: " + p2ShootTime);
		
		//hide gameStatusPlane
		gameStatusPlane.SetActive(false);
		continueTournamentBtn.SetActive(false);
		
		//Translate gameTimer index to actual seconds
		switch(PlayerPrefs.GetInt("GameTime")) {
			case 0:
				gameTimer = 180;
				break;
			case 1:
				gameTimer = 300;
				break;
			case 2:
				gameTimer = 480;
				break;
			
			//You can add more cases and options here.
		}

		//fill player shoot timer to full
		p1TimeBarInitScale = p1TimeBar.transform.localScale.x;
		p1TimeBarCurrentScale = p1TimeBar.transform.localScale.x;
		p2TimeBarInitScale = p2TimeBar.transform.localScale.x;
		p2TimeBarCurrentScale = p2TimeBar.transform.localScale.x;
		p1TimeBar.transform.localScale = new Vector3(1,1,1);
		p2TimeBar.transform.localScale = new Vector3(1,1,1);
		
		//Get Game Mode
		if(PlayerPrefs.HasKey("GameMode"))
			gameMode = PlayerPrefs.GetInt("GameMode");
		else
			gameMode = 0; // Deafault Mode (Player-1 vs AI)
		
		playerAIController = GameObject.FindGameObjectWithTag("playerAI");
		opponentAIController = GameObject.FindGameObjectWithTag("opponentAI");
		ball = GameObject.FindGameObjectWithTag("ball");
		
		manageGameModes();
	}
		

	//*****************************************************************************
	// We have all units inside the game scene by default, but at the start of the game,
	// we check which side in playing (should be active) and deactive the side that is
	// not playing by deactivating all it's units.
	//*****************************************************************************
	private GameObject[] player2Team;	//array of all player-2 units in the game
	private GameObject[] cpuTeam;		//array of all AI units in the game
	void manageGameModes (){
		switch(gameMode) {
			case 0:
				//find and deactive all player2 units. This is player-1 vs AI.
				player2Team = GameObject.FindGameObjectsWithTag("Player_2");
				foreach(GameObject unit in player2Team) {
					unit.SetActive(false);
				}
				break;
			
			case 1:
				//find and deactive all AI Opponent units. This is Player-1 vs Player-2.
				cpuTeam = GameObject.FindGameObjectsWithTag("Opponent");
				foreach(GameObject unit in cpuTeam) {
					unit.SetActive(false);
				}
				//deactive opponent's AI
				opponentAIController.SetActive(false);
				break;
		}
	}

	IEnumerator Start (){
		roundTurnManager();
		yield return new WaitForSeconds(1.5f);
		playSfx(startWistle);
	}

	//*****************************************************************************
	// FSM
	//*****************************************************************************
	void Update (){
        language();
		//check game finish status every frame
		if(!gameIsFinished) {
			manageGameStatus();
			updateTimeBars();
		}
		
		//every now and then, play some crowd chants
		StartCoroutine(playCrowdChants());
		
		//If you ever needed debug inforamtions:
		//print("GameRound: " + round + " & turn is for: " + whosTurn + " and GoalHappened is: " + goalHappened);
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
            }
            for (int i = 0; i < arab_text.Length; i++)
            {
                arab_text[i].SetActive(false);
            }            
        }
        else
        {
            for (int i = 0; i < eng_text.Length; i++)
            {
                eng_text[i].SetActive(false);
            }
            for (int i = 0; i < arab_text.Length; i++)
            {
                arab_text[i].SetActive(true);
            }
            if(gameMode == 0)
            {
                arab_text[2].SetActive(false);
            }
            else if (gameMode == 1)
            {
                arab_text[1].SetActive(false);
            }
        }

    }

    //*****************************************************************************
    // This function gives turn to players in the game.
    //*****************************************************************************
    public static string whosTurn;
	void roundTurnManager (){
		
		if(gameIsFinished || goalHappened)
			return;

		//reset shootHappened flag
		shootHappened = false;

		//fill time limit bars
		fillTimeBar(1);
		fillTimeBar(2);
		
		//if round number is odd, it's players turn, else it's AI or player-2 's turn
		int carry;
		carry = round % 2;
		if(carry == 1) {
			playersTurn = true;
			opponentsTurn = false;
			playerController.canShoot = true;
			OpponentAI.opponentCanShoot = false;
			whosTurn = "player";
		} else {
			playersTurn = false;
			opponentsTurn = true;
			playerController.canShoot = false;
			OpponentAI.opponentCanShoot = true;
			whosTurn = "opponent";
		}	
		
		//Override
		//for two player game, players can always shoot.
		//we override this because both human players play on the same device and must be able to shoot at every turn.
		//we just limit their actions to their own units.
		if(gameMode == 1)
			playerController.canShoot = true;		
	}


	//*****************************************************************************
	// Update timebars by changing their scale (x) over time.
	// If time ends, the turn will be transforred to opponent.
	//*****************************************************************************
	void updateTimeBars() {

		if(gameIsFinished || goalHappened || shootHappened)
			return;

		//limiters and turn change
		//Also change turns incase of time running out!
		if(p1TimeBarCurrentScale <= 0) {
			p1TimeBarCurrentScale = 0;
			setNewRound(1);
			return;
		}
		if(p2TimeBarCurrentScale <= 0) {
			p2TimeBarCurrentScale = 0;
			setNewRound(2);
			return;
		}
		

		if(playersTurn) {
			p1TimeBarCurrentScale -= Time.deltaTime / p1ShootTime;
			p1TimeBar.transform.localScale = new Vector3(p1TimeBarCurrentScale, p1TimeBar.transform.localScale.y, p1TimeBar.transform.localScale.z);
			fillTimeBar(2);
		} else {
			p2TimeBarCurrentScale -= Time.deltaTime / p2ShootTime;
			p2TimeBar.transform.localScale = new Vector3(p2TimeBarCurrentScale, p2TimeBar.transform.localScale.y, p2TimeBar.transform.localScale.z);
			fillTimeBar(1);
		}
	}


	void fillTimeBar(int _ID) {
		if(_ID == 1) {
			p1TimeBarCurrentScale = p1TimeBarInitScale;
			p1TimeBar.transform.localScale = new Vector3(1,1,1);
		} else {
			p2TimeBarCurrentScale = p2TimeBarInitScale;
			p2TimeBar.transform.localScale = new Vector3(1,1,1);
		}
	}

	void setNewRound(int _ID) {
		switch(_ID) {
		case 1:
			round = 2;
			break;		
		case 2:
			round = 1;
			break;	
		}	
		roundTurnManager();
	}

	//*****************************************************************************
	// What happens after a shoot is performed?
	//*****************************************************************************
	public IEnumerator managePostShoot ( string _shootBy  ){

		shootHappened = true;

		//get who is did the shoot
		//if we had a goal after the shoot was done and just before the round change, leave the process to other controllers.
		float t = 0;
		while(t < timeStepToAdvanceRound) {	
			t += Time.deltaTime;
			if(goalHappened) {
				yield break;
			} 		
			yield return 0;
		}
		
		//we had a simple shoot with no goal result
		if(t >= timeStepToAdvanceRound) {
			//add to round counters
			switch(_shootBy) {
				case "Player":
					round = 2;
					break;		
				case "Player_2":
					round = 1;
					break;	
				case "Opponent":
					round = 1;
					break;
			}	
			roundTurnManager(); //cycle again between players
		}
	}
			
	//*****************************************************************************
	// If we had a goal in this round, this is the function that manages all aspects of it.
	//*****************************************************************************								
	public IEnumerator managePostGoal ( string _goalBy  ){
		//get who did the goal.

		//avoid counting a goal as two or more
		if(goalHappened)
			yield break;
		
		//soft pause the game for reformation and other things...
		goalHappened = true;
		shootHappened = false;
		
		//add to goal counters
		switch(_goalBy) {
			case "Player":
				playerGoals++;
				round = 2; //goal by player-1 and opponent should start the next round
				break;
			case "Opponent":
				opponentGoals++;
				round = 1; //goal by opponent and player-1 should start the next round
				break;
		}
		
		//wait a few seconds to show the effects , and physics cooldown
		playSfx(goalSfx[Random.Range(0, goalSfx.Length)]);
		GetComponent<AudioSource>().PlayOneShot(goalHappenedSfx[Random.Range(0, goalHappenedSfx.Length)], 1);
		//yield return new WaitForSeconds(1);
		//activate the goal event plane
		GameObject gp = null;
		gp = Instantiate (goalPlane, new Vector3(30, 0, -2), Quaternion.Euler(0, 180, 0)) as GameObject;
		float t = 0;
		float speed = 2.0f;
		while(t < 1) {
			t += Time.deltaTime * speed;
			gp.transform.position = new Vector3(Mathf.SmoothStep(30, 0, t), 0, -2);
			yield return 0;
		}
		yield return new WaitForSeconds(0.75f);
		float t2 = 0;
		while(t2 < 1) {
			t2 += Time.deltaTime * speed;
			gp.transform.position = new Vector3(Mathf.SmoothStep(0, -30, t2), 0, -2);
			yield return 0;
		}
		Destroy(gp, 1.5f);
		
		//bring the ball back to it's initial position
		ball.GetComponent<TrailRenderer>().enabled = false;
		ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		ball.transform.position = new Vector3(0, -0.81f, -0.7f);
		yield return new WaitForSeconds(1);
		ball.GetComponent<TrailRenderer>().enabled = true;
		
		//*** reformation of units ***//
		//Reformation for player_1
		StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.playerTeam, PlayerPrefs.GetInt("PlayerFormation"), 0.6f, 1));
		
		//if this is player-1 vs player-2 match:
		if(GlobalGameManager.gameMode == 1) {
			StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.player2Team, PlayerPrefs.GetInt("Player2Formation"), 0.6f, -1));
		} else {	//if this is player-1 vs AI match:
			//get a new random formation everytime
			StartCoroutine(opponentAIController.GetComponent<OpponentAI>().changeFormation(Random.Range(0, FormationManager.formations), 0.6f));
		}
		
		yield return new WaitForSeconds(3);

		//check if the game is finished or not
		if(playerGoals > goalLimit || opponentGoals > goalLimit) {
			gameIsFinished = true;
			manageGameFinishState();
			yield break;
		} 
		
		//else, continue to the next round
		goalHappened = false;
		roundTurnManager();
		playSfx(startWistle);
	}

	//***************************************************************************//
	// Game status manager
	//***************************************************************************//
	public GameObject timeText;				//UI 3d text object
	public GameObject playerGoalsText;		//UI 3d text object
	public GameObject opponentGoalsText;	//UI 3d text object
	public GameObject playerOneName;		//UI 3d text object
	public GameObject playerTwoName;		//UI 3d text object
	void manageGameStatus (){
		seconds = Mathf.CeilToInt(gameTimer - Time.timeSinceLevelLoad) % 60;
		minutes = Mathf.CeilToInt(gameTimer - Time.timeSinceLevelLoad) / 60; 
		
		if(seconds == 0 && minutes == 0) {
			gameIsFinished = true;
			manageGameFinishState();
		}
		
		remainingTime = string.Format("{0:00} : {1:00}", minutes, seconds); 
		timeText.GetComponent<TextMesh>().text = remainingTime.ToString();

		playerGoalsText.GetComponent<TextMesh>().text = playerGoals.ToString();
		opponentGoalsText.GetComponent<TextMesh>().text = opponentGoals.ToString();

		if(gameMode == 0) {
			playerOneName.GetComponent<TextMesh>().text = player1Name;
			playerTwoName.GetComponent<TextMesh>().text = cpuName;
		} else if(gameMode == 1) {
			playerOneName.GetComponent<TextMesh>().text = player1Name;
			playerTwoName.GetComponent<TextMesh>().text = player2Name;
		} 
	}

	//*****************************************************************************
	// After the game is finished, this function handles the events.
	//*****************************************************************************
	void manageGameFinishState (){
		//Play gameFinish wistle
		playSfx(finishWistle);
		print("GAME IS FINISHED.");
		
		//show gameStatusPlane
		gameStatusPlane.SetActive(true);
		
		//for single player game, we should give the player some bonuses in case of winning the match
		if(gameMode == 0) {
			if(playerGoals > goalLimit || playerGoals > opponentGoals) {
				print("Player 1 is the winner!!");
				
				//set the result texture
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[0];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[0];

                int playerWins = PlayerPrefs.GetInt("PlayerWins");
				int playerMoney = PlayerPrefs.GetInt("PlayerMoney");
				
				PlayerPrefs.SetInt("PlayerWins", ++playerWins);			//add to wins counter
				PlayerPrefs.SetInt("PlayerMoney", playerMoney + 100);	//handful of coins as the prize!

				//if this is a tournament match, update it with win state and advance.
				if(PlayerPrefs.GetInt("IsTournament") == 1) {
					PlayerPrefs.SetInt("TorunamentMatchResult", 1);
					PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
					continueTournamentBtn.SetActive(true);
				}
				
			} else if(opponentGoals > goalLimit || opponentGoals > playerGoals) {
			
				print("CPU is the winner!!");
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[1];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[1];

                //if this is a tournament match, update it with lose state.
                if (PlayerPrefs.GetInt("IsTournament") == 1) {
					PlayerPrefs.SetInt("TorunamentMatchResult", 0);
					PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
					continueTournamentBtn.SetActive(true);
				}
				
			} else if(opponentGoals == playerGoals) {
			
				print("(Single Player) We have a Draw!");
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[4];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[4];

                //we count "draw" a lose in tournament mode.
                //if this is a tournament match, update it with lose state.
                if (PlayerPrefs.GetInt("IsTournament") == 1) {
					PlayerPrefs.SetInt("TorunamentMatchResult", 0);
					PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
					continueTournamentBtn.SetActive(true);
				}
			}	
		} else if(gameMode == 1) {
			if(playerGoals > opponentGoals) {
				print("Player 1 is the winner!!");
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[2];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[2];
            } else if(playerGoals == opponentGoals) {
				print("(Two-Player) We have a Draw!");
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[4];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[4];
            } else if(playerGoals < opponentGoals) {
				print("Player 2 is the winner!!");
				statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[3];
                statusTexture_arab.GetComponent<Renderer>().material.mainTexture = statusModes_arab[3];
            } 
		}
		admobdemo.mInstance.OnInterstitial ();
	}
	//*****************************************************************************
	// Play a random crown sfx every now and then to spice up the game
	//*****************************************************************************
	IEnumerator playCrowdChants (){
		if(canPlayCrowdChants) {
			canPlayCrowdChants = false;
			GetComponent<AudioSource>().PlayOneShot(crowdChants[Random.Range(0, crowdChants.Length)], 1);
			yield return new WaitForSeconds(Random.Range(15, 35));
			canPlayCrowdChants = true;
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

}