using System;

namespace qtLib.Ads
{
    public interface IAdService
    {
        public void Initialize(AdConfigModel ads, AdsManager.EvtAdReady onAdReady = null, AdsManager.EvtAdPaid onAdPaid = null);
        public void ShowAppOpenAd();
        public bool IsNativeAdsReady(AdsManager.AdPosition adPosition);
        public void ShowNativeOverlayAd(AdsManager.AdPosition adPosition);
        public void HideNativeOverlayAd(AdsManager.AdPosition adPosition);
        
        public void ShowBannerAd();
        public void HideBannerAd();
        
        public bool IsAdReady(AdsManager.AdType adType, AdsManager.AdPosition adPosition);
        public void ShowAd(AdsManager.AdType adType, AdsManager.AdPosition adPosition, Action onCompleteCallback = null, Action onCloseCallback = null);
        
        public void RemoveAds();
    }
}