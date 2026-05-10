using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AsyncLoadOrderingManager
{

	public static void OnStartedLoad(AsyncOperationHandle loadQueueItem, out int loadHandle)
	{
		AsyncLoadOrderingManager._lastLoadHandle++;
		loadHandle = AsyncLoadOrderingManager._lastLoadHandle;
		if (AsyncLoadOrderingManager._orderedLoads == null)
		{
			AsyncLoadOrderingManager._orderedLoads = new List<ValueTuple<int, AsyncOperationHandle>>();
		}
		AsyncLoadOrderingManager._orderedLoads.Add(new ValueTuple<int, AsyncOperationHandle>(loadHandle, loadQueueItem));
	}

	public static void CompleteUpTo(AsyncOperationHandle loadQueueItem, int loadHandle)
	{
		if (AsyncLoadOrderingManager._orderedLoads == null)
		{
			return;
		}
		if (AsyncLoadOrderingManager._tempList == null)
		{
			AsyncLoadOrderingManager._tempList = new List<ValueTuple<int, AsyncOperationHandle>>(AsyncLoadOrderingManager._orderedLoads.Count);
		}
		foreach (ValueTuple<int, AsyncOperationHandle> valueTuple in AsyncLoadOrderingManager._orderedLoads)
		{
			int item = valueTuple.Item1;
			AsyncOperationHandle item2 = valueTuple.Item2;
			if (item == loadHandle)
			{
				break;
			}
			AsyncLoadOrderingManager._tempList.Add(new ValueTuple<int, AsyncOperationHandle>(item, item2));
		}
		foreach (ValueTuple<int, AsyncOperationHandle> valueTuple2 in AsyncLoadOrderingManager._tempList)
		{
			AsyncOperationHandle item3 = valueTuple2.Item2;
			item3.WaitForCompletion();
		}
		AsyncLoadOrderingManager._tempList.Clear();
	}


	public static void OnCompletedLoad(AsyncOperationHandle loadQueueItem, int loadHandle)
	{
		if (AsyncLoadOrderingManager._orderedLoads == null)
		{
			return;
		}
		for (int i = AsyncLoadOrderingManager._orderedLoads.Count - 1; i >= 0; i--)
		{
			if (AsyncLoadOrderingManager._orderedLoads[i].Item1 == loadHandle)
			{
				AsyncLoadOrderingManager._orderedLoads.RemoveAt(i);
			}
		}
		if (AsyncLoadOrderingManager._orderedLoads.Count != 0)
		{
			return;
		}
		AsyncLoadOrderingManager._orderedLoads = null;
		if (AsyncLoadOrderingManager._onLoadsCompleteActionQueue == null)
		{
			return;
		}
		Action action;
		while (AsyncLoadOrderingManager._onLoadsCompleteActionQueue.TryDequeue(out action))
		{
			action();
		}
		AsyncLoadOrderingManager._onLoadsCompleteActionQueue = null;
	}

	public static void DoActionAfterAllLoadsComplete(Action action)
	{
		List<ValueTuple<int, AsyncOperationHandle>> orderedLoads = AsyncLoadOrderingManager._orderedLoads;
		if (orderedLoads == null || orderedLoads.Count <= 0)
		{
			action();
			return;
		}
		if (AsyncLoadOrderingManager._onLoadsCompleteActionQueue == null)
		{
			AsyncLoadOrderingManager._onLoadsCompleteActionQueue = new Queue<Action>();
		}
		AsyncLoadOrderingManager._onLoadsCompleteActionQueue.Enqueue(action);
	}

	private static List<ValueTuple<int, AsyncOperationHandle>> _orderedLoads;


	private static List<ValueTuple<int, AsyncOperationHandle>> _tempList;


	private static int _lastLoadHandle;


	private static Queue<Action> _onLoadsCompleteActionQueue;
}
