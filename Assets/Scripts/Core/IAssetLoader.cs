using System.Threading.Tasks;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Abstraction for asset loading. Current implementation uses Resources.Load;
    /// future implementation will use Addressables.
    /// </summary>
    public interface IAssetLoader
    {
        T LoadAsset<T>(string key) where T : Object;
        Task<T> LoadAssetAsync<T>(string key) where T : Object;
        void ReleaseAsset(string key);
    }
}
