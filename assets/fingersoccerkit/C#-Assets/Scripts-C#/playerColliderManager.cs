using UnityEngine;
using System.Collections;

public class playerColliderManager : MonoBehaviour {
		
	///*************************************************************************///
	/// Optional controller for collision of player units vs other items in the scene like ball or opponent units
	///*************************************************************************///

	public AudioClip unitsBallHit;		//units hits the ball sfx
	public AudioClip unitsGeneralHit;	//units general hit sfx (Not used)

	void OnCollisionEnter ( Collision other  ){
		switch(other.gameObject.tag) {
			case "Opponent":
				//PlaySfx(unitsGeneralHit);
				break;
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