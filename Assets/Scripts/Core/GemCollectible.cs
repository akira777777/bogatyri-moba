using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    public class GemCollectible : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<BrawlerController>();
            if (player != null && !player.IsDead)
            {
                var gameMode = FindObjectOfType<GemGrabMode>();
                if (gameMode != null)
                {
                    gameMode.CollectGem(player);
                    Destroy(gameObject);
                }
            }
        }
    }
}
