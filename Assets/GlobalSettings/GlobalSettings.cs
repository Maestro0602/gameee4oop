using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GlobalSettings
{
    public class GlobalSettings : MonoBehaviour
    {
        public static (GlobalSettings Runner, Coroutine Routine) StartLoad<T>(string fileName, Action<AsyncOperationHandle<T>?> onLoadStarted, Action<T> onComplete)
        {
            GlobalSettings component = new GameObject("GlobalSettings Loader " + fileName, new Type[]
            {
                typeof(GlobalSettings)
            }).GetComponent<GlobalSettings>();
            UnityEngine.Object.DontDestroyOnLoad(component);
            Coroutine routine = component.StartCoroutine(component.Load<T>(fileName, onLoadStarted, onComplete));
            return (component, routine);
        }

        private IEnumerator Load<T>(string fileName, Action<AsyncOperationHandle<T>?> onLoadStarted, Action<T> onComplete)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>("GlobalSettings/" + fileName + ".asset");
            int orderHandle;
            AsyncLoadOrderingManager.OnStartedLoad(asyncOperationHandle, out orderHandle);
            onLoadStarted(new AsyncOperationHandle<T>?(asyncOperationHandle));
            asyncOperationHandle.Completed += delegate (AsyncOperationHandle<T> handle)
            {
                AsyncLoadOrderingManager.OnCompletedLoad(handle, orderHandle);
                onComplete(handle.Result);
            };
            UnityEngine.Object.Destroy(base.gameObject);
            yield break;
        }
    }
}