using UnityEngine;
using System.Collections;
using admob;
public class admobdemo : MonoBehaviour
{
    public static admobdemo mInstance;
    // Use this for initialization

    public float timer1 = 300f;

    void Awake() {
        DontDestroyOnLoad(transform.gameObject);
        mInstance = this;
    }

    void Start()
    {
        Admob.Instance().bannerEventHandler += onBannerEvent;
        Admob.Instance().interstitialEventHandler += onInterstitialEvent;
        Admob.Instance().rewardedVideoEventHandler += onRewardedVideoEvent;

        onInitAdmob();
    }

    public void onInitAdmob()
    {
        Debug.Log("Initing Admob");
        Admob ad = Admob.Instance();

#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
		ad.initAdmob("ca-app-pub-5498938664783906/8473715872", "ca-app-pub-5498938664783906/9950449079");
#elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
		ad.initAdmob("ca-app-pub-5498938664783906/5015878679", "ca-app-pub-5498938664783906/7969345077");
#else
            string adUnitId = "unexpected_platform";
#endif

        ad.loadInterstitial();
    }

    public void OnInterstitial()
    {
        if (PlayerPrefs.GetInt("noads", 0) == 1)
        {
            return;
        }
        Admob ad = Admob.Instance();
        if (ad.isInterstitialReady())
        {
            ad.showInterstitial();
        }
    }

    public void OnShowBanner() {
        if (PlayerPrefs.GetInt("noads", 0) == 1)
        {
            return;
        }
        Debug.Log("ShowBanner");
        Admob.Instance().showBannerRelative(AdSize.Banner, AdPosition.BOTTOM_CENTER, 0);
    }

    public void OnHideBanner() {
        if (PlayerPrefs.GetInt("noads", 0) == 1)
        {
            return;
        }
        Debug.Log("HideBanner");
        Admob.Instance().removeBanner();
    }

    // Update is called once per frame
    void Update()
    {
//        timer1 -= Time.deltaTime;
//        if (timer1 <= 0) {
//            timer1 = 300f;
//            OnInterstitial();
//        }
    }
    void OnGUI2()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "initadmob"))
        {
            Admob ad = Admob.Instance();

            //   ad.setTesting(true);
        }
        if (GUI.Button(new Rect(120, 0, 100, 60), "showInterstitial"))
        {
            Admob ad = Admob.Instance();
            if (ad.isInterstitialReady())
            {
                ad.showInterstitial();
            }
            else
            {
                ad.loadInterstitial();
            }
        }
        if (GUI.Button(new Rect(240, 0, 100, 60), "showRewardVideo"))
        {
            Admob ad = Admob.Instance();
            if (ad.isRewardedVideoReady())
            {
                ad.showRewardedVideo();
            }
            else
            {
                ad.loadRewardedVideo("ca-app-pub-3940256099942544/xxxxxxxxxxx");
            }
        }
        if (GUI.Button(new Rect(240, 100, 100, 60), "showbanner"))
        {
            Admob.Instance().showBannerRelative(AdSize.Banner, AdPosition.BOTTOM_CENTER, 0);
        }
        if (GUI.Button(new Rect(240, 200, 100, 60), "showbannerABS"))
        {
            Admob.Instance().showBannerAbsolute(AdSize.Banner, 0, 30);
        }
        if (GUI.Button(new Rect(240, 300, 100, 60), "hidebanner"))
        {
            Admob.Instance().removeBanner();
        }
    }


    void onInterstitialEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobEvent---" + eventName + "   " + msg);
        if (eventName == AdmobEvent.onAdClosed)
        {
            Admob.Instance().loadInterstitial();
        }
    }

    void onBannerEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobBannerEvent---" + eventName + "   " + msg);
    }
    void onRewardedVideoEvent(string eventName, string msg)
    {
        Debug.Log("handler onRewardedVideoEvent---" + eventName + "   " + msg);
    }
}
