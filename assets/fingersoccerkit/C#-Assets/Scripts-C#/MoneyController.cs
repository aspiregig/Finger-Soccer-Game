using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing;

public class MoneyController : MonoBehaviour, IStoreListener  {

	///*************************************************************************///
	/// Main CoinPack purchase Controller.
	/// This class handles all touch events on coin packs.
	/// You can easily integrate your own (custom) IAB system to deliver a nice 
	/// IAP options to the player.
	///*************************************************************************///

	private float buttonAnimationSpeed = 9;	//speed on animation effect when tapped on button
	private bool  canTap = true;			//flag to prevent double tap
	public AudioClip coinsCheckout;				//purchase sound
	public AudioClip tapSfx;					//purchase sound

	//Reference to GameObjects
	public GameObject playerMoney;			//UI 3d text object
	private int availableMoney;				//UI 3d text object

    public GameObject[] eng_text = new GameObject[10];
    public GameObject[] arab_text = new GameObject[10];

    //*****************************************************************************
    // Init. Updates the 3d texts with saved values fetched from playerprefs.
    //*****************************************************************************
    void Awake (){
		//admobdemo.mInstance.OnShowBanner ();
		availableMoney = PlayerPrefs.GetInt("PlayerMoney");
		playerMoney.GetComponent<TextMesh>().text = "" + availableMoney;
	}

	//////IAP
	private static IStoreController m_StoreController;          // The Unity Purchasing system.
	private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

	void Start()
	{
		if (m_StoreController == null) {
			InitializePurchasing ();
		}
	}

	//--------------------------------- IAP
	public void InitializePurchasing() 
	{
		if (IsInitialized())
		{
			return;
		}

		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		builder.AddProduct("com.200coins", ProductType.Consumable);
		builder.AddProduct("com.500coins", ProductType.Consumable);
		builder.AddProduct("com.2500coins", ProductType.Consumable);

		UnityPurchasing.Initialize(this, builder);
	}

	private bool IsInitialized()
	{
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}

	public void BuyProduct(string productID)
	{
		if (IsInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = m_StoreController.products.WithID(productID);

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}

	public void BuyUnlimited()
	{
		if (IsInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = m_StoreController.products.WithID("com.silver.ninjaspin.unlimited");

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}

	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases()
	{
		// If Purchasing has not yet been set up ...
		if (!IsInitialized())
		{
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			Debug.Log("RestorePurchases FAIL. Not initialized.");
			return;
		}

		// If we are running on an Apple device ... 
		if (Application.platform == RuntimePlatform.IPhonePlayer || 
			Application.platform == RuntimePlatform.OSXPlayer)
		{
			// ... begin restoring purchases
			Debug.Log("RestorePurchases started ...");

			// Fetch the Apple store-specific subsystem.
			var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions((result) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
				Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
		}
		else
		{
			// We are not running on an Apple device. No work is necessary to restore purchases.
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}

	//  
	// --- IStoreListener
	//

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");

		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}


	public void OnInitializeFailed(InitializationFailureReason error)
	{
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}


	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
	{
		// A consumable product has been purchased by this user.
		if (string.Equals(args.purchasedProduct.definition.id, "com.200coins", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
			//add the purchased coins to the available user money
			availableMoney += 200;
			playerMoney.GetComponent<TextMesh>().text = "" + availableMoney;
			//save new amount of money
			PlayerPrefs.SetInt("PlayerMoney", availableMoney);
		}
		else if (string.Equals(args.purchasedProduct.definition.id, "com.500coins", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.			//add the purchased coins to the available user money

			availableMoney += 500;
			playerMoney.GetComponent<TextMesh>().text = "" + availableMoney;
			PlayerPrefs.SetInt("PlayerMoney", availableMoney);
		}
		else if (string.Equals(args.purchasedProduct.definition.id, "com.2500coins", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
			//add the purchased coins to the available user money
			availableMoney += 2500;
			playerMoney.GetComponent<TextMesh>().text = "" + availableMoney;
			//save new amount of money
			PlayerPrefs.SetInt("PlayerMoney", availableMoney);
		}
		else 
		{
			Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
		}

		// Return a flag indicating whether this product has completely been received, or if the application needs 
		// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
		// saving purchased products to the cloud, and when that save is delayed. 
		return PurchaseProcessingResult.Complete;
	}


	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}

	//-------------------------------------------------------------------


	//*****************************************************************************
	// FSM
	//*****************************************************************************
	void Update (){
        language();
		if(canTap) {
			StartCoroutine(tapManager());
		}
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
			
			case "coinPack_1":
				//animate the button
				StartCoroutine (animateButton (objectHit));					
				BuyProduct ("com.200coins");		
				//play sfx
				playSfx(coinsCheckout);
				//Reload the level
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				//Wait
				yield return new WaitForSeconds(1.5f);				
				break;
					
			case "coinPack_2":
				StartCoroutine (animateButton (objectHit));
				BuyProduct ("com.500coins");
				//play sfx
				playSfx(coinsCheckout);
				//Reload the level
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				yield return new WaitForSeconds(1.5f);
				break;
					
			case "coinPack_3":
				StartCoroutine (animateButton (objectHit));
				BuyProduct ("com.2500coins");
				//play sfx
				playSfx(coinsCheckout);
				//Reload the level
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				yield return new WaitForSeconds(1.5f);
				break;
				
			case "Btn-Back":
				StartCoroutine(animateButton(objectHit));
				playSfx(tapSfx);
				yield return new WaitForSeconds(1.0f);
				SceneManager.LoadScene("Menu-c#");
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