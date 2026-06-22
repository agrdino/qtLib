using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        /// <summary>
        /// 1000 value
        /// </summary>
        private static List<int> _pool = new List<int>();

        private static Dictionary<int, List<int>> _table = new Dictionary<int, List<int>>();
        
        private static Dictionary<int, List<int>> _percentPool = new Dictionary<int, List<int>>();

        private static System.Random _systemRandom = new System.Random();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns>index</returns>
        public static int RandomValue(params int[] param)
        {
            _table.Clear();
            _pool = Enumerable.Range(0, 1000).ToList();

            int randomIndex = 0;
            for (int i = 0; i < param.Length; i++)
            {
                List<int> target = new List<int>();
                int amount = param[i] * 10;
                for (int j = 0; j < amount; j++)
                {
                    randomIndex = Random.Range(i + j, _pool.Count);
                    target.Add(_pool[randomIndex]);
                    (_pool[i + j], _pool[randomIndex]) = (_pool[randomIndex], _pool[i + j]);
                }
                _table.Add(i, target);
            }

            int luckyNumber = Random.Range(0, 1000);
            foreach (var keyValuePair in _table)
            {
                if (keyValuePair.Value.Contains(luckyNumber))
                {
                    return keyValuePair.Key;
                }
            }

            return 0;
        }

        public static bool RandomPercent(int percent)
        {
            if (_percentPool.ContainsKey(percent))
            {
                return _percentPool[percent].Contains(Random.Range(0, 100));
            }
            
            //pick lucky number
            List<int> luckyNumbers = new List<int>();
            List<int> pool = Enumerable.Range(0, 100).ToList();
            for (int i = 0; i < 100; i++)
            {
                pool.Add(i);
            }
            for (int i = 0; i < percent; i++)
            {
                int randomIndex = Random.Range(i, pool.Count);
                luckyNumbers.Add(pool[i]);
                (pool[i], pool[randomIndex]) = (pool[randomIndex], pool[i]);
            }
            
            _percentPool.Add(percent, luckyNumbers);
            return luckyNumbers.Contains(Random.Range(0, 100));
        }

        public static bool LuckyRoll(int ratio)
        {
            return Random.Range(0, 100) <= ratio;
        }
        
        public static Vector3 RandomVector(Vector3[] vector3)
        {
            if (vector3.Length == 0)
            {
                return Vector3.zero;
            }
            
            float totalDistance = 0f;
            for (var i = 0; i < vector3.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                totalDistance += Vector3.Distance(vector3[i - 1], vector3[i]);
            }

            float randomValue = Random.Range(0, totalDistance);

            for (int i = 0; i < vector3.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                float tempDistance = Vector3.Distance(vector3[i - 1], vector3[i]);
                if (randomValue <= tempDistance)
                {
                    return Vector3.Lerp(vector3[i - 1], vector3[i], randomValue / tempDistance);
                }
                else
                {
                    randomValue -= tempDistance;
                }
            }

            return vector3[^1];
        }
        
        public static Vector3 RandomVector(Vector3 left, Vector3 right)
        {
            float x = Random.Range(0, 1f);
            return Vector3.Lerp(left, right, x);
        }

        public static Vector3 RandomVectorInZone(Vector3 leftTop, Vector3 rightBottom)
        {
            float x = Random.Range(leftTop.x, rightBottom.x);
            float y = Random.Range(leftTop.y, rightBottom.y);
            return new Vector3(x, y);
        }

        public static Vector3 RandomVector(IEnumerable<(Vector3 left, Vector3 right)> vectors)
        {
            (Vector3 left, Vector3 right) randomValue = vectors.ElementAt(Random.Range(0, vectors.Count()));
            return RandomVector(randomValue.left, randomValue.right);
        }
        
        public static Vector3 RandomVector(IEnumerable<(Vector2 left, Vector2 right)> vectors)
        {
            (Vector3 left, Vector3 right) randomValue = vectors.ElementAt(Random.Range(0, vectors.Count()));
            return RandomVector(randomValue.left, randomValue.right);
        }
        
        public static Vector3 RandomVector(List<(Vector3 p1, Vector3 p2)> path)
        {
            if (path.Count == 0)
            {
                return Vector3.zero;
            }

            (Vector3 p1, Vector3 p2) selected = path[Random.Range(0, path.Count)];
            float randomValue = Random.Range(0, 1f);
            return Vector3.Lerp(selected.p1, selected.p2, randomValue);
        }

        public static Vector3 RandomPointInCircleXY(Vector3 center, float radius)
        {
            Vector2 random2D = Random.insideUnitCircle * radius;
            return center + new Vector3(random2D.x, random2D.y, 0f);
        }
        
        public static int SystemRandom(int min, int max)
        {
            return _systemRandom.Next(min, max);
        }
        
        public static Vector2[] GetEllipsePoints2D(Vector2 center, float radiusWidth, float radiusHeight, int amount)
        {
            Vector2[] points = new Vector2[amount];
            float angle = 360f / amount;
            for (int i = 0; i < amount; i++)
            {
                float angleDeg = angle * i;
                float angleRad = Mathf.Deg2Rad * angleDeg;
                float x = Mathf.Cos(angleRad) * radiusWidth;
                float y = Mathf.Sin(angleRad) * radiusHeight;
                points[i] = center + new Vector2(x, y);
            }
            return points;
        }
    }
}