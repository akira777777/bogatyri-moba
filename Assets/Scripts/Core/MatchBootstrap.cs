using UnityEngine;

namespace BogatyriMoba.Core
{
    public class MatchBootstrap : MonoBehaviour
    {
        [SerializeField] private string defaultBrawlerResource = "Brawlers/Alesha";
        [SerializeField] private bool autoStartMatch = true;

        private void Start()
        {
            if (!autoStartMatch || GameManager.Instance == null)
                return;

            var data = Resources.Load<BrawlerData>(defaultBrawlerResource);
            if (data == null)
            {
                Debug.LogError($"MatchBootstrap: brawler not found at Resources/{defaultBrawlerResource}");
                return;
            }

            GameManager.Instance.StartMatch(data);
        }
    }
}
