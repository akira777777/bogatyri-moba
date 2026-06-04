using System.Threading.Tasks;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Asset loader backed by Unity Resources folder.
    /// </summary>
    public class ResourcesAssetLoader : IAssetLoader
    {
        public T LoadAsset<T>(string key) where T : Object
        {
            return Resources.Load<T>(key);
        }

        public Task<T> LoadAssetAsync<T>(string key) where T : Object
        {
            var request = Resources.LoadAsync<T>(key);
            var tcs = new TaskCompletionSource<T>();
            request.completed += _ =>
            {
                var result = request.asset as T;
                if (result != null)
                    tcs.SetResult(result);
                else
                    tcs.SetResult(null);
            };
            return tcs.Task;
        }

        public void ReleaseAsset(string key)
        {
            // Resources.UnloadAsset is scene-dependent; we leave cleanup to scene unload.
        }
    }
}
