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
            float y = startPos.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
            transform.position = new Vector3(startPos.x, y, startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<BrawlerController>();
            if (player == null || player.IsDead) return;

            OnCollected?.Invoke(player);
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterGem(this);
            Destroy(gameObject);
        }
    }
}
