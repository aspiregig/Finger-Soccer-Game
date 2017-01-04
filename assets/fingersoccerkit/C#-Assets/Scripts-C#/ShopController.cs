using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShopController : MonoBehaviour {

	///*************************************************************************///
	/// Main Shop Controller class.
	/// This class handles all touch events on buttons.
	/// It also checks if user has enough money to buy items, it items are already purchased,
	/// and saves the purchased items into playerprefs for further usage.
	///*************************************************************************///

	private float buttonAnimationSpeed = 9;	//speed on animation effect when tapped on button
	private bool  canTap = true;			//flag to prevent double tap

	public AudioClip coinsCheckout;			//buy sfx
	public GameObject playerMoney;			//Reference to 3d text
	private int availableMoney;

	public GameObject[] totalItemsForSale;	//Purchase status

    public GameObject[] eng_text = new GameObject[10];
    public GameObject[] arab_text = new GameObject[10];

    //*****************************************************************************
    // Init. 
    //*****************************************************************************
    void Awake (){
		admobdemo.mInstance.OnShowBanner ();
		//Updates 3d text with saved values fetched from playerprefs
		availableMoney = PlayerPrefs.GetInt("PlayerMoney");
		playerMoney.GetComponent<TextMesh>().text = "" + availableMoney;
		
		//check if we previously purchased these items.
		for(int i = 0; i < totalItemsForSale.Length; i++) {
			//format the correct string we use to store purchased items into playerprefs
			string shopItemName = "shopItem-" + totalItemsForSale[i].GetComponent<ShopItemProperties>().itemIndex.ToString();
			if(PlayerPrefs.GetInt(shopItemName) == 1) {
				//we already purchased this item
				totalItemsForSale[i].GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1); 	//Make it green
				totalItemsForSale[i].GetComponent<BoxCollider>().enabled = false;			//Not clickable anymore
			}
		}
	}

	//*****************************************************************************
	// FSM 
	//*****************************************************************************
	void Update (){
        language();
		if(canTap) {
			StartCoroutine(tapManager());
		}

		if(Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("Shop-c#");
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

    //*****************************************************************************
    // This function monitors player touches on menu buttons.
    // detects both touch and clicks and can be used with editor, handheld device and 
    // every other platforms at once.
    //*****************************************************************************
    private RaycastHit hitInfo;
	private Ray ray;
	private string saveName = "";
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
			
				case "shopItem_1":
					//if we have enough money, purchase this item and save the event
					if(availableMoney >= objectHit.GetComponent<ShopItemProperties>().itemPrice) {
						//animate the button
						StartCoroutine(animateButton(objectHit));
						
						//deduct the price from user money
						availableMoney -= objectHit.GetComponent<ShopItemProperties>().itemPrice;
						
						//save new amount of money
						PlayerPrefs.SetInt("PlayerMoney", availableMoney);
						
						//save the event of purchase
						saveName = "shopItem-" + objectHit.GetComponent<ShopItemProperties>().itemIndex.ToString();
						PlayerPrefs.SetInt(saveName, 1);
						
						//play sfx
						playSfx(coinsCheckout);
						
						//Wait
						yield return new WaitForSeconds(1.5f);
						
						//Reload the level
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					}
					break;
					
				case "shopItem_2":
					if(availableMoney >= objectHit.GetComponent<ShopItemProperties>().itemPrice) {
						StartCoroutine(animateButton(objectHit));
						availableMoney -= objectHit.GetComponent<ShopItemProperties>().itemPrice;
						PlayerPrefs.SetInt("PlayerMoney", availableMoney);
						saveName = "shopItem-" + objectHit.GetComponent<ShopItemProperties>().itemIndex.ToString();
						PlayerPrefs.SetInt(saveName, 1);
						playSfx(coinsCheckout);
						yield return new WaitForSeconds(1.5f);
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					}
					break;
					
				case "shopItem_3":
					if(availableMoney >= objectHit.GetComponent<ShopItemProperties>().itemPrice) {
						StartCoroutine(animateButton(objectHit));
						availableMoney -= objectHit.GetComponent<ShopItemProperties>().itemPrice;
						PlayerPrefs.SetInt("PlayerMoney", availableMoney);
						saveName = "shopItem-" + objectHit.GetComponent<ShopItemProperties>().itemIndex.ToString();
						PlayerPrefs.SetInt(saveName, 1);
						playSfx(coinsCheckout);
						yield return new WaitForSeconds(1.5f);
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					}
					break;
				
				case "Btn-Back":
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					SceneManager.LoadScene("Shop-c#");
					break;
			}	
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