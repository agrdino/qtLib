using System;

namespace qtLib.Ads
{
    public interface IAdUnit
    {
        public void LoadAd();
        public void ShowAds(Action onCompleteCallback = null, Action onCloseCallback = null);
        public void HideAds(Action callback = null);
        public bool IsReady();
    }
}