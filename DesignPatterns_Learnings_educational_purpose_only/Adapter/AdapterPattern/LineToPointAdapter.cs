﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace AdapterPattern
{
    public class LineToPointAdapter : IEnumerable<Point>
    {
        static Dictionary<int, List<Point>> cache = new Dictionary<int, List<Point>>();
        private static int count = 0;
        private int hash;
        public LineToPointAdapter(Line line)
        {
            hash = line.GetHashCode();

            if (cache.ContainsKey(hash)) return;

            Console.WriteLine($"{++count}: Generating points for line [{line.Start.X},{line.Start.Y}]-[{line.End.X},{line.End.Y}] (no caching)");

            List<Point> points = new List<Point>();

            int left = Math.Min(line.Start.X, line.End.X);
            int right = Math.Max(line.Start.X, line.End.X);
            int top = Math.Min(line.Start.Y, line.End.Y);
            int bottom = Math.Max(line.Start.Y, line.End.Y);
            int dx = right - left;
            int dy = line.End.Y - line.Start.Y;

            if (dx == 0)
            {
                for (int y = top; y <= bottom; ++y)
                {
                    points.Add(new Point(left, y));
                }
            }
            else if (dy == 0)
            {
                for (int x = left; x <= right; ++x)
                {
                    points.Add(new Point(x, top));
                }
            }

            cache.Add(hash, points);
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return cache[hash].GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }




}
