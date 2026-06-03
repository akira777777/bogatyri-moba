using UnityEngine;
using System.Collections.Generic;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Spatial hash grid for O(1) spatial queries.
    /// Replaces linear searches in AI and collision detection.
    /// </summary>
    public class SpatialHashGrid
    {
        private readonly float _cellSize;
        private readonly Dictionary<long, List<BrawlerController>> _grid;
        private readonly HashSet<BrawlerController> _allEntities;

        public SpatialHashGrid(float cellSize)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<long, List<BrawlerController>>();
            _allEntities = new HashSet<BrawlerController>();
        }

        public void Insert(BrawlerController entity)
        {
            if (entity == null) return;
            _allEntities.Add(entity);
            UpdateEntity(entity);
        }

        public void Remove(BrawlerController entity)
        {
            if (entity == null) return;
            _allEntities.Remove(entity);
            var key = GetKey(entity.transform.position);
            if (_grid.TryGetValue(key, out var list))
                list.Remove(entity);
        }

        public void UpdateEntity(BrawlerController entity)
        {
            if (entity == null) return;

            // Remove from old cell
            foreach (var kvp in _grid)
            {
                kvp.Value.Remove(entity);
            }

            // Insert into new cell
            var key = GetKey(entity.transform.position);
            if (!_grid.TryGetValue(key, out var list))
            {
                list = new List<BrawlerController>();
                _grid[key] = list;
            }
            list.Add(entity);
        }

        public List<BrawlerController> Query(Vector2 position, float radius)
        {
            var results = new List<BrawlerController>();
            float radiusSqr = radius * radius;
            int cellRadius = Mathf.CeilToInt(radius / _cellSize);
            Vector2Int centerCell = GetCell(position);

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    long key = GetKey(centerCell.x + x, centerCell.y + y);
                    if (_grid.TryGetValue(key, out var list))
                    {
                        foreach (var entity in list)
                        {
                            if (entity != null && Vector2.SqrMagnitude((Vector2)entity.transform.position - position) <= radiusSqr)
                                results.Add(entity);
                        }
                    }
                }
            }

            return results;
        }

        public BrawlerController FindNearest(Vector2 position, float maxRadius, System.Predicate<BrawlerController> filter = null)
        {
            BrawlerController nearest = null;
            float nearestDist = float.MaxValue;
            var candidates = Query(position, maxRadius);

            foreach (var entity in candidates)
            {
                if (filter != null && !filter(entity)) continue;
                float d = Vector2.SqrMagnitude((Vector2)entity.transform.position - position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = entity;
                }
            }

            return nearest;
        }

        public void Clear()
        {
            _grid.Clear();
            _allEntities.Clear();
        }

        private Vector2Int GetCell(Vector2 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / _cellSize),
                Mathf.FloorToInt(position.y / _cellSize)
            );
        }

        private long GetKey(Vector2 position)
        {
            var cell = GetCell(position);
            return GetKey(cell.x, cell.y);
        }

        private long GetKey(int x, int y)
        {
            // Cantor pairing function for 2D to 1D hash
            long a = (long)(x >= 0 ? 2 * x : -2 * x - 1);
            long b = (long)(y >= 0 ? 2 * y : -2 * y - 1);
            return (a + b) * (a + b + 1) / 2 + b;
        }
    }
}
