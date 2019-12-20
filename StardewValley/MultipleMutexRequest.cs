using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	internal class MultipleMutexRequest
	{
		protected int _reportedCount;

		protected List<NetMutex> _acquiredLocks;

		protected List<NetMutex> _mutexList;

		protected Action _onSuccess;

		protected Action _onFailure;

		public MultipleMutexRequest(List<NetMutex> mutexes, Action success_callback = null, Action failure_callback = null)
		{
			_onSuccess = success_callback;
			_onFailure = failure_callback;
			_acquiredLocks = new List<NetMutex>();
			_mutexList = new List<NetMutex>(mutexes);
			_RequestMutexes();
		}

		public MultipleMutexRequest(NetMutex[] mutexes, Action success_callback = null, Action failure_callback = null)
		{
			_onSuccess = success_callback;
			_onFailure = failure_callback;
			_acquiredLocks = new List<NetMutex>();
			_mutexList = new List<NetMutex>(mutexes);
			_RequestMutexes();
		}

		protected void _RequestMutexes()
		{
			if (_mutexList == null)
			{
				if (_onFailure != null)
				{
					_onFailure();
				}
				return;
			}
			if (_mutexList.Count == 0)
			{
				if (_onSuccess != null)
				{
					_onSuccess();
				}
				return;
			}
			for (int j = 0; j < _mutexList.Count; j++)
			{
				if (_mutexList[j].IsLocked())
				{
					if (_onFailure != null)
					{
						_onFailure();
					}
					return;
				}
			}
			for (int i = 0; i < _mutexList.Count; i++)
			{
				NetMutex mutex = _mutexList[i];
				mutex.RequestLock(delegate
				{
					_OnLockAcquired(mutex);
				}, delegate
				{
					_OnLockFailed(mutex);
				});
			}
		}

		protected void _OnLockAcquired(NetMutex mutex)
		{
			_reportedCount++;
			_acquiredLocks.Add(mutex);
			if (_reportedCount >= _mutexList.Count)
			{
				_Finalize();
			}
		}

		protected void _OnLockFailed(NetMutex mutex)
		{
			_reportedCount++;
			if (_reportedCount >= _mutexList.Count)
			{
				_Finalize();
			}
		}

		protected void _Finalize()
		{
			if (_acquiredLocks.Count < _mutexList.Count)
			{
				ReleaseLocks();
				_onFailure();
			}
			else
			{
				_onSuccess();
			}
		}

		public void ReleaseLocks()
		{
			for (int i = 0; i < _acquiredLocks.Count; i++)
			{
				_acquiredLocks[i].ReleaseLock();
			}
			_acquiredLocks.Clear();
		}
	}
}
