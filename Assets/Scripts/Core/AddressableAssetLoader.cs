using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Asset loader backed by Unity Addressables.
    /// Activate by registering this instance in GameServices instead of ResourcesAssetLoader.
    /// </summary>
    public class AddressableAssetLoader : IAssetLoader
    {
        private readonly System.Collections.Generic.Dictionary<string, AsyncOperationHandle> _handles = new();

        public T LoadAsset<T>(string key) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            _handles[key] = handle;
            return handle.WaitForCompletion();
        }

        public async Task<T> LoadAssetAsync<T>(string key) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            _handles[key] = handle;
            return await handle.Task;
        }

        public void ReleaseAsset(string key)
        {
            if (_handles.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(key);
            }
        }
    }
}
