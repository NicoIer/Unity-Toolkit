﻿using ConcaveHull;
using System;

namespace ConcaveHull
{
    /// <summary>
    /// 线段相交算法, 用于计算凹多边形
    /// 参考自 https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
    /// </summary>
    public static class LineIntersectionFunctions
    {
        // 计算p1q1和p2q2两条线段是否相交
        public static bool DoIntersect(Node p1, Node q1, Node p2, Node q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        private static bool OnSegment(Node p, Node q, Node r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
        private static int Orientation(Node p, Node q, Node r)
        {
            
            double val = (q.y - p.y) * (r.x - q.x) -
                         (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0; // colinear 

            return val > 0 ? 1 : 2; // clock or counterclock wise 
        }
    }
}