using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        public static UniTask AsTask(this AsyncOperation operation)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            operation.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}