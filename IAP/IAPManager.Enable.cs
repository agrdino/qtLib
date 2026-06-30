#if ENABLE_IAP

using System;
using System.Collections.Generic;
using System.Linq;
using qtLib.Helper;
using _Scripts.UI.Popup.LoadingPopup;
using Cysharp.Threading.Tasks;
using qtLib.UI.Base;
using UnityEngine.Purchasing;

namespace qtLib.IAP
{
    public partial class IAPManager
    {
        #region ----- Component Config -----
        
        private StoreController _storeController;
        private List<ProductDefinition> _productDefinitions;
        private Action<bool> _purchasedCallback;
        private Action<bool> _restorePurchaseCallback;
        private bool _isInProcess;
        private bool _isRestoring;
        
        private Action _hideLoadingPopup;

        private bool _isInitSuccess;
        
        #endregion
        
        #region ----- Public Function -----

        public string GetProductPrice(string productId)
        {
            Product temp = _storeController.GetProductById(productId);
            if (temp == null)
            {
                return _GetOfflineProductPrice(productId);
            }
            
            return temp.metadata.localizedPriceString;
        }

        public void PurchaseItem(string productId, Action<string, bool> callback = null)
        {
            _StartPurchase(productId, isSuccess =>
            {
                _HideLoadingPayment();
                callback?.Invoke(productId, isSuccess);   
            });
        }

        public async UniTaskVoid Restore(Action<bool> callback = null)
        {
            qtDebug.Log("IAPManager: Restore");
            if (!_isInitSuccess || _isRestoring)
            {
                qtDebug.Log("IAPManager: Restore fail: init not success");
                return;
            }
#if UNITY_ANDROID
            return;
#elif UNITY_IOS
            _isRestoring = true;
            
            await _ShowLoadingPayment();
            _restorePurchaseCallback = callback;
            _storeController.FetchPurchases();
#endif
        }

        #endregion

        #region ----- Private Function -----
        
        protected override async void _Init()
        {
            qtDebug.Log("IAPManager: Init");
            _storeController = UnityIAPServices.StoreController();
            
            _storeController.OnProductsFetched += _OnProductsFetched;
            _storeController.OnProductsFetchFailed += _OnProductsFetchFailed;
            
            _storeController.OnPurchasePending += _OnPurchasePending;
            _storeController.OnPurchaseDeferred += _OnPurchaseDeferred;
            _storeController.OnPurchaseConfirmed += _OnPurchaseConfirmed;
            _storeController.OnPurchaseFailed += _OnPurchaseFailed;
            
            _storeController.OnPurchasesFetched += _OnPurchasesFetched;
            _storeController.OnPurchasesFetchFailed += _OnPurchasesFetchFailed;
            
            _storeController.OnStoreDisconnected += _OnStoreDisconnected;
            _storeController.OnCheckEntitlement += _OnCheckEntitlement;
            
            qtDebug.Log("IAPManager: Connecting...");
            await _storeController.Connect();
            
            qtDebug.Log("IAPManager: Connected");
            _isInitSuccess = true;

            _productDefinitions = new List<ProductDefinition>()
            {
                new(IAPProduct.Shop_RemoveAds, ProductType.NonConsumable),
                new(IAPProduct.Shop_KeepPlayingPack, ProductType.NonConsumable),
            };
            
            _storeController.FetchProducts(_productDefinitions);
        }
        
        private async UniTaskVoid _StartPurchase(string productId, Action<bool> callback = null)
        {
            qtDebug.Log($"IAPManager: StartPurchase {productId}");
            if (_isInProcess)
            {
                qtDebug.Log($"IAPManager: Purchase {productId} fail: Another product still processing!");
                callback?.Invoke(false);
                return;
            }

            if (_storeController != null)
            {
                var product = _storeController.GetProductById(productId);
                if (product is { availableToPurchase: true })
                {
                    await _ShowLoadingPayment();
                    _storeController.CheckEntitlement(product);
                    _isInProcess = true;

                    _purchasedCallback = callback;
                }
                else
                {
                    callback?.Invoke(false);
                    qtDebug.LogError("IAPManager: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                callback?.Invoke(false);
                qtDebug.LogError($"IAPManager: Not IsInitialized");
            }
        }
        
        private async UniTask _ShowLoadingPayment()
        {
            qtDebug.Log("IAPManager: Show loading payment");
            await qtUiFlow.Request<LoadingPopupMediator>().BeforeUIShow((ui, logic, mediator) =>
            {
                _hideLoadingPopup = ui.ControllerHide;
                ui.uiResult = new UniTaskCompletionSource<ParamOutput>();
                return UniTask.CompletedTask;
            })
                .AfterUIShow((ui, logic, mediator) =>
                {
                    ui.uiResult.TrySetResult(new NoOutput());
                    return UniTask.CompletedTask;
                }).Move<NoOutput>();
            await UniTask.Delay(100);
        }

        private void _HideLoadingPayment()
        {
            qtDebug.Log("IAPManager: Hide loading payment");
            _hideLoadingPopup?.Invoke();
            _hideLoadingPopup = null;
        }
        
        private string _GetOfflineProductPrice(string productId)
        {
            var catalog = ProductCatalog.LoadDefaultCatalog();
            var item = catalog.allProducts.FirstOrDefault(x => x.id == productId);
            if (item == null)
            {
                return "--";
            }

            return item.pricingTemplateID;
        }

        #endregion

        #region ----- Callback -----
        
        private void _OnStoreDisconnected(StoreConnectionFailureDescription storeConnectionFailureDescription)
        {
            //Todo: kiểm tra retry để retry
            _isInitSuccess = false;
            qtDebug.Log($"IAPManager: _OnStoreDisconnected: {storeConnectionFailureDescription.message} - {storeConnectionFailureDescription.IsRetryable}");
        }

        private void _OnProductsFetched(List<Product> products)
        {
            qtDebug.Log($"IAPManager: _OnProductsFetched: {products.Count}");
            foreach (var product in products)
            {
                qtDebug.Log($"{product.definition.id} - {product.metadata.localizedPriceString} - {product.availableToPurchase}");
            }
        }
        
        private void _OnProductsFetchFailed(ProductFetchFailed productFetchFailed)
        {
            qtDebug.Log($"IAPManager: _OnProductsFetchFailed: {productFetchFailed.FailureReason}");
            foreach (var purchasedProductInfo in productFetchFailed.FailedFetchProducts)
            {
                qtDebug.Log(purchasedProductInfo.id);
            }
        }

        private void _OnCheckEntitlement(Entitlement entitlement)
        {
            qtDebug.Log($"IAPManager: _OnCheckEntitlement: {entitlement.Status} : {entitlement.ErrorMessage}");
            switch (entitlement.Status)
            {
                case EntitlementStatus.NotEntitled:
                {         
                    if (entitlement.Product != null)
                    {
                        qtDebug.Log($"IAPManager: Purchasing product asynchronously: '{entitlement.Product.definition.storeSpecificId}'");
                        _storeController.PurchaseProduct(entitlement.Product);
                    }

                    break;
                }
                default:
                {
                    _purchasedCallback?.Invoke(false);
                    _purchasedCallback = null;

                    _isInProcess = false;
                    _HideLoadingPayment();
                    break;
                }
            }
        }

        private void _OnPurchaseConfirmed(Order order)
        {
            qtDebug.Log($"IAPManager: _OnPurchaseConfirmed: {order.Info.TransactionID}");
            foreach (var purchasedProductInfo in order.Info.PurchasedProductInfo)
            {
                qtDebug.Log(purchasedProductInfo.productId);
            }

            _isInProcess = false;
            _purchasedCallback?.Invoke(true);
            _purchasedCallback = null;
        }
        
        private void _OnPurchaseDeferred(DeferredOrder deferredOrder)
        {
            qtDebug.Log($"IAPManager: _OnPurchaseDeferred: {deferredOrder.Info.TransactionID}");
            foreach (var purchasedProductInfo in deferredOrder.Info.PurchasedProductInfo)
            {
                qtDebug.Log(purchasedProductInfo.productId);
            }
        }
        
        private void _OnPurchaseFailed(FailedOrder failedOrder)
        {
            qtDebug.Log($"IAPManager: _OnPurchaseFailed: {failedOrder.FailureReason}");
            foreach (var purchasedProductInfo in failedOrder.Info.PurchasedProductInfo)
            {
                qtDebug.Log(purchasedProductInfo.productId);
            }
            
            _isInProcess = false;
            _purchasedCallback?.Invoke(false);
            _purchasedCallback = null;
        }
        
        private void _OnPurchasePending(PendingOrder pendingOrder)
        {
            qtDebug.Log($"IAPManager: _OnPurchasePending: {pendingOrder.Info.TransactionID}");
            foreach (var purchasedProductInfo in pendingOrder.Info.PurchasedProductInfo)
            {
                qtDebug.Log(purchasedProductInfo.productId);
            }
            
            _storeController.ConfirmPurchase(pendingOrder);
        }
        
        private void _OnPurchasesFetched(Orders orders)
        {
            qtDebug.Log($"IAPManager: _OnPurchasesFetched");
            qtDebug.Log($"ConfirmedOrders: {orders.ConfirmedOrders.Count}");
            qtDebug.Log($"DeferredOrders: {orders.DeferredOrders.Count}");
            qtDebug.Log($"PendingOrders: {orders.PendingOrders.Count}");
            
            foreach (var objConfirmedOrder in orders.ConfirmedOrders)
            {
                foreach (var purchasedProductInfo in objConfirmedOrder.Info.PurchasedProductInfo)
                {
                    qtDebug.Log($"IAPManager restore succeeded: {purchasedProductInfo.productId}");
                    MessageDispatcher.SendMessage(MessageDispatcher.EEvent.IAPPurchaseSucceeded, new MessageDispatcher.IAPPurchaseSucceededMessage()
                    {
                        productId = purchasedProductInfo.productId,
                    });
                }
            }
            
            if (_isRestoring)
            {
                _isRestoring = false;
                _restorePurchaseCallback?.Invoke(true);
                _restorePurchaseCallback = null;
                _HideLoadingPayment();
            }
        }

        private void _OnPurchasesFetchFailed(PurchasesFetchFailureDescription purchasesFetchFailureDescription)
        {
            qtDebug.Log($"IAPManager: _OnPurchasesFetchFailed {purchasesFetchFailureDescription.FailureReason}");
            if (_isRestoring)
            {
                _isRestoring = false;
                _restorePurchaseCallback?.Invoke(false);
                _restorePurchaseCallback = null;
                _HideLoadingPayment();
            }
        }

        #endregion
    }
}
#endif