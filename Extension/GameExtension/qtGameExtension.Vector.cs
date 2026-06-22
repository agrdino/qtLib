using System.Collections.Generic;
using UnityEngine;

namespace qtLib.Extension
{
    public partial class qtGameExtension
    {
        public static List<Vector3> GetVector3OnSegments(IEnumerable<Vector3> points, IEnumerable<(Vector3 left, Vector3 right)> segments, float epsilon = 0.01f)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (var point in points)
            {
                foreach (var segment in segments)
                {
                    if (IsPointOnSegment(point, segment.left, segment.right, epsilon))
                    {
                        result.Add(point);
                        break; // Không cần kiểm tra các đoạn còn lại
                    }
                }
            }

            return result;
        }
       
        public static bool IsVector3OnSegments(Vector3 point, IEnumerable<(Vector3 left, Vector3 right)> segments, float epsilon = 0.01f)
        {
            foreach (var segment in segments)
            {
                if (IsPointOnSegment(point, segment.left, segment.right, epsilon))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static List<Vector2> GetVector2OnSegments(IEnumerable<Vector2> points, IEnumerable<(Vector2 left, Vector2 right)> segments, float epsilon = 0.01f)
        {
            List<Vector2> result = new List<Vector2>();
        
            foreach (var point in points)
            {
                foreach (var segment in segments)
                {
                    if (IsPointOnSegment(point, segment.left, segment.right, epsilon))
                    {
                        result.Add(point);
                        break; // Không cần kiểm tra các đoạn còn lại
                    }
                }
            }
        
            return result;
        }
        
        public static bool IsVector2OnSegments(Vector2 point, IEnumerable<(Vector2 left, Vector2 right)> segments, float epsilon = 0.01f)
        {
            foreach (var segment in segments)
            {
                if (IsPointOnSegment(point, segment.left, segment.right, epsilon))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static bool IsPointOnSegment(Vector3 point, Vector3 left, Vector3 right, float epsilon = 0.01f)
        {
            Vector3 segment = right - left;
            Vector3 pointToLeft = point - left;

            // Kiểm tra xem C có nằm thẳng hàng với đoạn AB không
            float cross = Vector3.Cross(segment, pointToLeft).magnitude;
            if (cross > epsilon)
            {
                return false;
            }

            // Kiểm tra nếu C nằm giữa A và B
            float dot = Vector3.Dot(pointToLeft, segment);
            if (dot < 0)
            {
                return false;
            }

            if (dot > Vector3.Dot(segment, segment))
            {
                return false;
            }

            return true;
        }
    }
}