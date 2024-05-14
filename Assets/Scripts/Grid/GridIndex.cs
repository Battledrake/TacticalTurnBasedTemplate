using System;
using Unity.VisualScripting;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [System.Serializable]
    public struct GridIndex : IEquatable<GridIndex>
    {
        public int x;
        public int z;

        public GridIndex(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public GridIndex Abs()
        {
            return new GridIndex(Mathf.Abs(x), Mathf.Abs(z));
        }

        /// <summary>
        /// Returns a GridIndex with x and z being equal to int.MinValue. Use for ensuring GridIndex is not on grid.
        /// </summary>
        /// <returns></returns>
        public static GridIndex Invalid()
        {
            return new GridIndex(int.MinValue, int.MinValue);
        }

        public static bool operator ==(GridIndex a, GridIndex b)
        {
            return a.x == b.x && a.z == b.z;
        }

        public static bool operator !=(GridIndex a, GridIndex b)
        {
            return !(a == b);
        }

        public static GridIndex operator +(GridIndex a, GridIndex b)
        {
            return new GridIndex(a.x + b.x, a.z + b.z);
        }

        public static GridIndex operator +(GridIndex a, Vector2Int b)
        {
            return new GridIndex(a.x + b.x, a.z + b.y);
        }

        public static GridIndex operator -(GridIndex a, GridIndex b)
        {
            return new GridIndex(a.x - b.x, a.z - b.z);
        }

        public static GridIndex operator -(GridIndex a, Vector2Int b)
        {
            return new GridIndex(a.x - b.x, a.z - b.y);
        }

        public static Vector2 operator *(GridIndex a, Vector2 b)
        {
            return new Vector2(a.x * b.y, a.z * b.y);
        }

        public static GridIndex RoundToInt(Vector2 v)
        {
            return new GridIndex(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        public static implicit operator Vector2(GridIndex index)
        {
            return new Vector2(index.x, index.z);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, z);
        }

        public override bool Equals(object obj)
        {
            return obj is GridIndex gridIndex &&
                x == gridIndex.x &&
                z == gridIndex.z;
        }

        public bool Equals(GridIndex other)
        {
            return this == other;
        }

        public override string ToString()
        {
            return $"{x},{z}";
        }
    }
}
