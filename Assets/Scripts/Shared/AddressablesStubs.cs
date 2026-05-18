using System;

namespace UnityEngine.AddressableAssets
{
    public static class Addressables
    {
        public static UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> LoadAssetAsync<T>(string key)
        {
            return new UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T>();
        }
    }
}

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    public struct AsyncOperationHandle
    {
        private object _result;

        public object Result
        {
            get => _result;
            set => _result = value;
        }

        public event Action<AsyncOperationHandle> Completed;

        public bool IsValid()
        {
            return true;
        }

        public object WaitForCompletion()
        {
            return _result;
        }

        public void Release()
        {
        }

        public void InvokeCompleted()
        {
            Completed?.Invoke(this);
        }
    }

    public struct AsyncOperationHandle<T>
    {
        public T Result { get; set; }

        public event Action<AsyncOperationHandle<T>> Completed;

        public static implicit operator AsyncOperationHandle(AsyncOperationHandle<T> handle)
        {
            return new AsyncOperationHandle { Result = handle.Result };
        }

        public bool IsValid()
        {
            return true;
        }

        public T WaitForCompletion()
        {
            return Result;
        }

        public void Release()
        {
        }

        public void InvokeCompleted()
        {
            Completed?.Invoke(this);
        }
    }
}
