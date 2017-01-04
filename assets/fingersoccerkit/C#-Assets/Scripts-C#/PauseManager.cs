using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {
		
	//***************************************************************************//
	// This class manages pause and unpause states.
	//***************************************************************************//

	//static bool  soundEnabled;
	internal bool isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;

	enum Page {
		PLAY, PAUSE
	}
	private Page currentPage = Page.PLAY;

	//*****************************************************************************
	// Init.
	//*****************************************************************************
	void Awake (){		
		//soundEnabled = true;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 
	}

	//*****************************************************************************
	// FSM
	//*****************************************************************************
	void Update (){

		//touch control
		touchManager();
		
		//optional pause in Editor & Windows (just for debug)
		if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape)) {
			//PAUSE THE GAME
			switch (currentPage) {
	            case Page.PLAY: 
	            	PauseGame(); 
	            	break;
	            case Page.PAUSE: 
	            	UnPauseGame(); 
	            	break;
	            default: 
	            	currentPage = Page.PLAY;
	            	break;
	        }
		}
		
		//debug restart
		if(Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	//*****************************************************************************
	// This function monitors player touches on menu buttons.
	// detects both touch and clicks and can be used with editor, handheld device and 
	// every other platforms at once.
	//*****************************************************************************
	void touchManager (){
		if(Input.GetMouseButtonDown(0)) {
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hitInfo)) {
				string objectHitName = hitInfo.transform.gameObject.name;
				switch(objectHitName) {
					case "PauseBtn":
						switch (currentPage) {
				            case Page.PLAY: 
				            	PauseGame();
				            	break;
				            case Page.PAUSE: 
				            	UnPauseGame(); 
				            	break;
				            default: 
				            	currentPage = Page.PLAY;
				            	break;
				        }
						break;
					
					case "ResumeBtn":
						switch (currentPage) {
				            case Page.PLAY: 
				            	PauseGame();
				            	break;
				            case Page.PAUSE: 
				            	UnPauseGame(); 
				            	break;
				            default: 
				            	currentPage = Page.PLAY;
				            	break;
				        }
						break;
					
					case "RestartBtn":
						UnPauseGame();
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
						
					case "MenuBtn":
						UnPauseGame();
						SceneManager.LoadScene("Menu-c#");
						break;

					//if tournament mode is on
					case "ContinueTournamentBtn":
						UnPauseGame();
						SceneManager.LoadScene("Tournament-c#");
						break;
				}
			}
		}
	}

	void PauseGame (){
		print("Game in Paused...");
		isPaused = true;
		savedTimeScale = Time.timeScale;
	    Time.timeScale = 0;
	    AudioListener.volume = 0;
	    if(pausePlane)
	    	pausePlane.SetActive(true);
	    currentPage = Page.PAUSE;
	}

	void UnPauseGame (){
		print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
	    AudioListener.volume = 1.0f;
		if(pausePlane)
	    	pausePlane.SetActive(false);   
	    currentPage = Page.PLAY;
	}
}