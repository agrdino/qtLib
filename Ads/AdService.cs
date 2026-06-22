namespace qtLib.Ads
{
    public abstract class AdService
    {
        protected IAdUnit _bannerAdUnit;
        protected IAdUnit _interstitalAdUnit;
        protected IAdUnit _rewardedVideoAdUnit;
        protected IAdUnit _rewardedInterstitialAdUnit;
        protected IAdUnit _appOpenAdUnit;
        protected IAdUnit _nativeOvlayAdUnit;
        
        protected bool _isInitialized;
        
        // public abstract void InitializeAds();
        //
        // public abstract void ShowAppOpenAds();
        //
        // public abstract void ShowBannerAds();
        //
        // public abstract bool IsInterstitialAdsReady();
        // public abstract void ShowInterstitialAds(Action callback);
        //
        // public abstract bool IsRewardedInterstitialAdsReady();
        // public abstract void ShowRewardedInterstitialAds(Action callback);
        //
        // public abstract bool IsRewardedVideoAdsReady();
        // public abstract void ShowRewardedVideoAds(Action callback);
        //
        // public abstract void RemoveAds();
    }
}