using UnityEngine;

namespace BogatyriMoba.Core
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobAmount = 0.2f;

        private Vector3 startPos;
        private float bobOffset;
        public System.Action<BrawlerController> OnCollected;

        private void Start()
        {
            startPos = transform.position;
            bobOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            var pos = transform.position;
            pos.y = startPos.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
            transform.position = pos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<BrawlerController>();
            if (player == null || player.IsDead) return;

            OnCollected?.Invoke(player);
            SpawnManager.Instance?.UnregisterGem(this);
            Destroy(gameObject);
        }
    }
}
