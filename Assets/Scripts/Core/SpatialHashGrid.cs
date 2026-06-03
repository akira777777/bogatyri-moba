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
        // Tracks each entity's last known cell key so UpdateEntity is O(1) instead of O(cells)
        private readonly Dictionary<BrawlerController, long> _entityCell;
        private readonly List<BrawlerController> _queryResults = new List<BrawlerController>(16);

        public SpatialHashGrid(float cellSize)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<long, List<BrawlerController>>();
            _allEntities = new HashSet<BrawlerController>();
            _entityCell = new Dictionary<BrawlerController, long>();
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
            if (_entityCell.TryGetValue(entity, out var oldKey))
            {
                if (_grid.TryGetValue(oldKey, out var list))
                    list.Remove(entity);
                _entityCell.Remove(entity);
            }
        }

        public void UpdateEntity(BrawlerController entity)
        {
            if (entity == null) return;

            var newKey = GetKey(entity.transform.position);

            // Only move the entity if it changed cells
            if (_entityCell.TryGetValue(entity, out var oldKey) && oldKey == newKey)
                return;

            // Remove from old cell
            if (oldKey != 0 || _entityCell.ContainsKey(entity))
            {
                if (_grid.TryGetValue(oldKey, out var oldList))
                    oldList.Remove(entity);
            }

            // Insert into new cell
            if (!_grid.TryGetValue(newKey, out var newList))
            {
                newList = new List<BrawlerController>();
                _grid[newKey] = newList;
            }
            newList.Add(entity);
            _entityCell[entity] = newKey;
        }

        public List<BrawlerController> Query(Vector2 position, float radius)
        {
            _queryResults.Clear();
            QueryInto(position, radius, _queryResults);
            return _queryResults;
        }

        public void QueryInto(Vector2 position, float radius, List<BrawlerController> results)
        {
            if (results == null) return;
            results.Clear();

            float radiusSqr = radius * radius;
            int cellRadius = Mathf.CeilToInt(radius / _cellSize);
            Vector2Int centerCell = GetCell(position);

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    long key = GetKey(centerCell.x + x, centerCell.y + y);
                    if (!_grid.TryGetValue(key, out var list)) continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var entity = list[i];
                        if (entity != null && Vector2.SqrMagnitude((Vector2)entity.transform.position - position) <= radiusSqr)
                            results.Add(entity);
                    }
                }
            }
        }

        public BrawlerController FindNearest(Vector2 position, float maxRadius, System.Predicate<BrawlerController> filter = null)
        {
            BrawlerController nearest = null;
            float nearestDistSqr = maxRadius * maxRadius;
            int cellRadius = Mathf.CeilToInt(maxRadius / _cellSize);
            Vector2Int centerCell = GetCell(position);

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    long key = GetKey(centerCell.x + x, centerCell.y + y);
                    if (!_grid.TryGetValue(key, out var list)) continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var entity = list[i];
                        if (entity == null) continue;
                        if (filter != null && !filter(entity)) continue;

                        float d = Vector2.SqrMagnitude((Vector2)entity.transform.position - position);
                        if (d < nearestDistSqr)
                        {
                            nearestDistSqr = d;
                            nearest = entity;
                        }
                    }
                }
            }

            return nearest;
        }

        public void Clear()
        {
            _grid.Clear();
            _allEntities.Clear();
            _entityCell.Clear();
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
