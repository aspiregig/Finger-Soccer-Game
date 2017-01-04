using UnityEngine;
using System.Collections;

public class OpponentUnitController : MonoBehaviour {

	///*************************************************************************///
	/// Unit controller class for AI units
	///*************************************************************************///

	internal int unitIndex;					//every AI unit has an index. this is for the AI controller to know which unit must be selected.
											//Indexes are given to units by the AIController itself.

	private bool  canShowSelectionCircle;	//if the turn is for AI, units can show the selection circles.
	public GameObject selectionCircle;		//reference to gameObject.

	public AudioClip unitsBallHit;			//units hits the ball sfx

	void Awake (){
		canShowSelectionCircle = true;
	}

	void Update (){	
		if(GlobalGameManager.opponentsTurn && canShowSelectionCircle && !GlobalGameManager.goalHappened)
			selectionCircle.GetComponent<Renderer>().enabled = true;
		else	
			selectionCircle.GetComponent<Renderer>().enabled = false;			
	}

	void OnCollisionEnter ( Collision other  ){
		switch(other.gameObject.tag) {
		case "ball":
			PlaySfx(unitsBallHit);
			break;
		}
	}
	
	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	void PlaySfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}
}