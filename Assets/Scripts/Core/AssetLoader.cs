namespace BogatyriMoba.Core
{
    /// <summary>
    /// Static accessor for the current IAssetLoader implementation.
    /// Falls back to ResourcesAssetLoader if none is registered in GameServices.
    /// </summary>
    public static class AssetLoader
    {
        private static IAssetLoader _fallback;

        public static IAssetLoader Default
        {
            get
            {
                var registered = GameServices.Get<IAssetLoader>();
                if (registered != null) return registered;
                if (_fallback == null) _fallback = new ResourcesAssetLoader();
                return _fallback;
            }
        }
    }
}
