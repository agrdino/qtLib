using System;
using System.Threading;

namespace qtLib.Ads
{
    public abstract class AdUnit
    {
        protected Action _onCompleteCallback;
        protected Action _onCloseCallback;
        protected Action _onOpenCallback;

        protected AdsManager.EvtAdPaid _onAdPaid;

        protected string _adUnitID;
#if UNITY_EDITOR
        protected const int TimeTryToReload = 10000;
#else
        protected const int TimeTryToReload = 15000;
#endif
        protected const int MaxReloadTime = 3;
        protected int _countReload;

        protected (AdsManager.AdType adType, AdsManager.AdPosition adPosition) _adInfor;
        protected AdsManager.EvtAdReady _onAdReady;

        protected CancellationTokenSource _cancellationTokenSource;

        public AdUnit(string adUnitID, AdsManager.EvtAdPaid onAdPaid)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            _adUnitID = adUnitID;

            _onAdPaid = onAdPaid;
        }

        public AdUnit((AdsManager.AdType adType, AdsManager.AdPosition adPosition) adInfor, string adUnitID, 
            AdsManager.EvtAdReady onAdReady,
            AdsManager.EvtAdPaid onAdPaid)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            _adUnitID = adUnitID;
            _adInfor = adInfor;
            _onAdReady = onAdReady;
            _onAdPaid = onAdPaid;
        }

        public virtual void ShowAds(Action onCompleteCallback = null, Action onCloseCallback = null)
        {
            _onCompleteCallback = onCompleteCallback;
            _onCloseCallback = onCloseCallback;
        }

        public abstract void LoadAd();
    }
}