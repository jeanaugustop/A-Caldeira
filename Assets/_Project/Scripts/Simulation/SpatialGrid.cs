using System;

namespace ACaldeira.Simulation
{
    // Rebuilt each simulation tick. Fixed world bounds; linked buckets never truncate dense cells.
    public sealed class SpatialGrid
    {
        private readonly int[] heads;
        private readonly int[] next;
        private readonly int columns;
        private readonly int rows;
        private readonly float originX, originY, inverseCell;
        public SpatialGrid(int capacity, int columns, int rows, float originX, float originY, float cellSize)
        {
            if (capacity < 1 || columns < 1 || rows < 1 || cellSize <= 0) throw new ArgumentOutOfRangeException();
            this.columns = columns; this.rows = rows;
            this.originX = originX; this.originY = originY; inverseCell = 1f / cellSize;
            heads = new int[columns * rows]; next = new int[capacity]; Clear();
        }
        public int Column(float x) => Math.Max(0, Math.Min(columns - 1, (int)Math.Floor((x - originX) * inverseCell)));
        public int Row(float y) => Math.Max(0, Math.Min(rows - 1, (int)Math.Floor((y - originY) * inverseCell)));
        public void Clear() { for (int i = 0; i < heads.Length; i++) heads[i] = -1; }
        public void Insert(int index, float x, float y)
        {
            int cell = Row(y) * columns + Column(x);
            next[index] = heads[cell]; heads[cell] = index;
        }
        public int Head(int column, int row) => heads[row * columns + column];
        public int Next(int index) => next[index];
    }
}
