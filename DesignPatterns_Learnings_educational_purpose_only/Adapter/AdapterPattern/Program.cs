using System;
using System.Collections.Generic;

namespace AdapterPattern
{
    class Program
    {
        private static readonly List<VectorObject> vectorObjects = new List<VectorObject>
        {
            new VectorRectangle(1, 1, 10, 10),
            new VectorRectangle(3, 3, 6, 6)
        };

        private static void Draw()
        {
            foreach (var vo in vectorObjects)
            {
                foreach (var line in vo)
                {
                    var adapter = new LineToPointAdapter(line);
                    foreach (var point in adapter)
                    {
                        DrawPoint(point);
                    }
                }
            }
        }

        private static void DrawPoint(Point p)
        {
            Console.Write(".");
        }

        static void Main(string[] args)
        {
            //Draw();
            //Draw();

            Vector2i v = Vector2i.Create(1, 2);
            Vector3f vv = Vector3f.Create(3f, 22f, 2);
            var result = v + vv;
        }
    }

    public class Vector3f : VectorOfFloat<Vector3f, Dimensions.Three>
    {

    }

    public class Vector2i : VectorOfInt<Vector2i, Dimensions.Two>
    {
        
    }
}
