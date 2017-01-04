using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
namespace admob
{
	public class Admob {
        public delegate void AdmobEventHandler(string eventName, string msg);

        public event AdmobEventHandler bannerEventHandler;
        public event AdmobEventHandler interstitialEventHandler;
        public event AdmobEventHandler rewardedVideoEventHandler;

		private static Admob _instance;	
	
		public static Admob Instance()
	    {
	        if(_instance == null)
	        {
	            _instance = new Admob();
				_instance.preInitAdmob ();
	        }
	        return _instance;
	    }
        
       
        #if UNITY_IOS
        internal delegate void AdmobAdCallBack(string adtype, string eventName, string msg);
        private void preInitAdmob()
        {

        }
        [DllImport("__Internal")]
        private static extern void _kminitAdmob(string bannerid, string fullid, AdmobAdCallBack callback);
        public void initAdmob(string bannerID, string fullID)
        {
            _kminitAdmob(bannerID, fullID, onAdmobEventCallBack);
        }

        [DllImport("__Internal")]
        private static extern void _kmshowBannerAbsolute(int width, int height, int x, int y);
        public void showBannerAbsolute(AdSize size, int x, int y)
        {
            _kmshowBannerAbsolute(size.Width, size.Height, x, y);
        }

        [DllImport("__Internal")]
        private static extern void _kmshowBannerRelative(int width, int height, int position, int marginY);
        public void showBannerRelative(AdSize size, int position, int marginY)
        {
            _kmshowBannerRelative(size.Width, size.Height, position, marginY);
        }

        [DllImport("__Internal")]
        private static extern void _kmremoveBanner();
        public void removeBanner()
        {
            _kmremoveBanner();
        }

        [DllImport("__Internal")]
        private static extern void _kmloadInterstitial();
        public void loadInterstitial()
        {
            _kmloadInterstitial();
        }

        [DllImport("__Internal")]
        private static extern bool _kmisInterstitialReady();
        public bool isInterstitialReady()
        {
            return _kmisInterstitialReady();
        }

        [DllImport("__Internal")]
        private static extern void _kmshowInterstitial();
        public void showInterstitial()
        {
            _kmshowInterstitial();
        }

         [DllImport("__Internal")]
        private static extern void _kmloadRewardedVideo(string rewardedVideoID);
        public void loadRewardedVideo(string rewardedVideoID)
        {
            _kmloadRewardedVideo(rewardedVideoID);
        }

        [DllImport("__Internal")]
        private static extern bool _kmisRewardedVideoReady();
        public bool isRewardedVideoReady()
        {
            return _kmisRewardedVideoReady();
        }

        [DllImport("__Internal")]
        private static extern void _kmshowRewardedVideo();
        public void showRewardedVideo()
        {
            _kmshowRewardedVideo();
        }

        [DllImport("__Internal")]
        private static extern void _kmsetTesting(bool v);
        public void setTesting(bool v)
        {
            _kmsetTesting(v);
        }

        [DllImport("__Internal")]
        private static extern void _kmsetForChildren(bool v);
        public void setForChildren(bool v)
        {
          //  Debug.Log("set for child in c#");
            _kmsetForChildren(v);
        }

        [MonoPInvokeCallback(typeof(AdmobAdCallBack))]
        public static void onAdmobEventCallBack(string adtype, string eventName, string msg)
        {
         //   Debug.Log("c# receive callback " + adtype + "  " + eventName + "  " + msg);
            if (adtype == "banner")
            {
                Admob.Instance().bannerEventHandler(eventName, msg);
            }
            else if (adtype == "interstitial")
            {
                Admob.Instance().interstitialEventHandler(eventName, msg);
            }
            else if (adtype == "rewardedVideo")
            {
                Admob.Instance().rewardedVideoEventHandler(eventName, msg);
            }
        }
        
#elif UNITY_ANDROID
	private AndroidJavaObject jadmob;
         private void preInitAdmob(){
			if (jadmob == null) {
                AndroidJavaClass admobUnityPluginClass = new AndroidJavaClass("com.admob.plugin.AdmobUnityPlugin");
				jadmob = admobUnityPluginClass.CallStatic<AndroidJavaObject> ("getInstance");
                InnerAdmobListener innerlistener = new InnerAdmobListener();
                innerlistener.admobInstance = this;
                jadmob.Call("setListener", new AdmobListenerProxy(innerlistener));
			}
		}
		public void initAdmob(string bannerID,string fullID){
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject activy = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jadmob.Call ("initAdmob", new object[]{activy,bannerID,fullID});
		}
        public void showBannerRelative(AdSize size, int position,int marginY)
        {
            jadmob.Call("showBannerRelative", new object[] { size.Width,size.Height,position,marginY});
		}
        public void showBannerAbsolute(AdSize size, int x, int y)
        {
            jadmob.Call("showBannerAbsolute", new object[] { size.Width, size.Height,x,y });
        }
        public void removeBanner()
        {
            jadmob.Call("removeBanner");
        }


        public void loadInterstitial()
        {
            jadmob.Call("loadInterstitial");
        }
        public bool isInterstitialReady()
        {
            bool isReady = jadmob.Call<bool>("isInterstitialReady");
            return isReady;
        }
        public void showInterstitial()
        {
            jadmob.Call("showInterstitial");
        }


        public void loadRewardedVideo(string rewardedVideoID)
        {
            jadmob.Call("loadRewardedVideo", new object[] { rewardedVideoID });
        }
        public bool isRewardedVideoReady()
        {
            bool isReady = jadmob.Call<bool>("isRewardedVideoReady");
            return isReady;
        }
        public void showRewardedVideo()
        {
            jadmob.Call("showRewardedVideo");
        }



        public void setTesting(bool value)
        {
            jadmob.Call("setTesting",value);
        }
        public void setForChildren(bool value)
        {
            jadmob.Call("setForChildren",value);
        }
        class InnerAdmobListener : IAdmobListener
        {
            internal Admob admobInstance;
            public void onAdmobEvent(string adtype, string eventName, string paramString)
            {
                if (adtype == "banner")
                {
                    admobInstance.bannerEventHandler(eventName, paramString);
                }
                else if (adtype == "interstitial")
                {
                    admobInstance.interstitialEventHandler(eventName, paramString);
                }
                else if (adtype == "rewardedVideo")
                {
                    admobInstance.rewardedVideoEventHandler(eventName, paramString);
                }
            }
        }
#else
        private void preInitAdmob()
        {
           
        }
        
        public void initAdmob(string bannerID, string fullID)
        {
            Debug.Log("calling initAdmob");
        }

        
        public void showBannerAbsolute(AdSize size, int x, int y)
        {
            Debug.Log("calling showBannerAbsolute");
        }

        
        public void showBannerRelative(AdSize size, int position, int marginY)
        {
            Debug.Log("calling showBannerRelative");
        }

        
        public void removeBanner()
        {
            Debug.Log("calling removeBanner");
        }

        
        public void loadInterstitial()
        {
            Debug.Log("calling loadInterstitial");
        }

        
        public bool isInterstitialReady()
        {
            Debug.Log("calling isInterstitialReady");
        return false;
        }

        
        public void showInterstitial()
        {
            Debug.Log("calling showInterstitial");
        }

        public void loadRewardedVideo(string rewardedVideoID)
        {
            Debug.Log("calling loadRewardedVideo");
        }
        public bool isRewardedVideoReady()
        {
            Debug.Log("calling isRewardedVideoReady");
            return false;
        }
        public void showRewardedVideo()
        {
            Debug.Log("calling showRewardedVideo");
        }
        
        public void setTesting(bool v)
        {
            Debug.Log("calling setTesting");
        }

        
        public void setForChildren(bool v)
        {
            Debug.Log("calling setForChildren");
        }
        #endif

    }
}
