#if !ENABLE_IAP

using System;
using Cysharp.Threading.Tasks;
using qtLib.Helper;

namespace qtLib.IAP
{
    public partial class IAPManager
    {
        #region ----- Public Function -----

        public string GetProductPrice(string productId)
        {
            return string.Empty;
        }

        public void PurchaseItem(string productId, Action<string, bool> callback = null)
        {
        }

        public void Restore(Action<bool> callback = null)
        {

        }

        #endregion

        #region ----- Private Function -----

        protected override void _Init()
        {

        }


        private string _GetOfflineProductPrice(string productId)
        {
            return string.Empty;
        }

        #endregion
    }
}
#endif