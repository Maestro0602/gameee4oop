using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GlobalSettings
{

    public abstract class GlobalSettingsBase<T> : ScriptableObject where T : GlobalSettingsBase<T>
    {

        protected static T Get(string fileName)
        {
            if (!GlobalSettingsBase<T>._foundInstance)
            {
                if (GlobalSettingsBase<T>._delayedLoader.Item2 != null)
                {
                    if (GlobalSettingsBase<T>._delayedLoader.Item1)
                    {
                        GlobalSettingsBase<T>._delayedLoader.Item1.StopCoroutine(GlobalSettingsBase<T>._delayedLoader.Item2);
                        UnityEngine.Object.Destroy(GlobalSettingsBase<T>._delayedLoader.Item1.gameObject);
                    }
                    GlobalSettingsBase<T>._delayedLoader.Item2 = null;
                    GlobalSettingsBase<T>._delayedLoader.Item1 = null;
                }
                AsyncOperationHandle<T> value;
                if (GlobalSettingsBase<T>._loadHandle == null)
                {
                    GlobalSettingsBase<T>._loadHandle = new AsyncOperationHandle<T>?(Addressables.LoadAssetAsync<T>("GlobalSettings/" + fileName + ".asset"));
                    AsyncOperationHandle loadHandle = GlobalSettingsBase<T>._loadHandle.Value;
                    AsyncLoadOrderingManager.OnStartedLoad(loadHandle, out GlobalSettingsBase<T>._orderHandle);
                    value = GlobalSettingsBase<T>._loadHandle.Value;
                    value.Completed += delegate (AsyncOperationHandle<T> handle)
                    {
                        AsyncOperationHandle completedHandle = handle;
                        AsyncLoadOrderingManager.OnCompletedLoad(completedHandle, GlobalSettingsBase<T>._orderHandle);
                    };
                }
                AsyncOperationHandle currentHandle = GlobalSettingsBase<T>._loadHandle.Value;
                AsyncLoadOrderingManager.CompleteUpTo(currentHandle, GlobalSettingsBase<T>._orderHandle);
                value = GlobalSettingsBase<T>._loadHandle.Value;
                GlobalSettingsBase<T>._instance = value.WaitForCompletion();
                if (!GlobalSettingsBase<T>._instance)
                {
                    GlobalSettingsBase<T>._instance = ScriptableObject.CreateInstance<T>();
                }
                GlobalSettingsBase<T>._foundInstance = true;
            }
            return GlobalSettingsBase<T>._instance;
        }

        protected static void StartPreloadAddressable(string fileName)
        {
            if (GlobalSettingsBase<T>._loadHandle != null || GlobalSettingsBase<T>._delayedLoader.Item2 != null)
            {
                return;
            }
            GlobalSettingsBase<T>._delayedLoader = GlobalSettings.StartLoad<T>(fileName, delegate (AsyncOperationHandle<T>? value)
            {
                GlobalSettingsBase<T>._loadHandle = value;
            }, delegate (T value)
            {
                GlobalSettingsBase<T>._instance = value;
                GlobalSettingsBase<T>._foundInstance = true;
                GlobalSettingsBase<T>._delayedLoader.Item2 = null;
                GlobalSettingsBase<T>._delayedLoader.Item1 = null;
            });
        }


        protected static void StartUnload()
        {
            if (GlobalSettingsBase<T>._loadHandle == null)
            {
                return;
            }
            GlobalSettingsBase<T>._loadHandle.Value.Release();
            GlobalSettingsBase<T>._loadHandle = null;
            GlobalSettingsBase<T>._foundInstance = false;
            GlobalSettingsBase<T>._instance = default(T);
        }


        private void OnDestroy()
        {
            if (GlobalSettingsBase<T>._instance == this)
            {
                GlobalSettingsBase<T>._foundInstance = false;
                GlobalSettingsBase<T>._instance = default(T);
            }
        }
        private static int _orderHandle;
        private static bool _foundInstance;
        private static T _instance;
        private static AsyncOperationHandle<T>? _loadHandle;
        private static (GlobalSettings Runner, Coroutine Routine) _delayedLoader;
    }
}
