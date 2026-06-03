using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public class Safe : MonoBehaviour
    {
        [SerializeField] private int teamId;
        [SerializeField] private HeistMode heistMode;

        public void Configure(int ownerTeamId, HeistMode mode)
        {
            teamId = ownerTeamId;
            heistMode = mode;
        }

        public void TakeDamage(int damage)
        {
            if (heistMode != null)
                heistMode.DamageSafe(teamId, damage);
        }

        public int GetTeamId() => teamId;
    }
}
