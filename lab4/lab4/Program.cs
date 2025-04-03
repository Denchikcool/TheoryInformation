// using System;
// using System.Collections.Generic;
// using System.IO;

// namespace lab4
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             string fileName = "matrix.txt";
//             int n = -1, m = -1;

//             try
//             {
//                 using (StreamReader reader = new StreamReader(fileName))
//                 {
//                     string line = reader.ReadLine();
//                     if (line != null)
//                     {
//                         string[] dimensions = line.Split(' ');
//                         n = int.Parse(dimensions[0]);
//                         m = int.Parse(dimensions[1]);
//                         if (n >= m)
//                         {
//                             Console.WriteLine("Ошибка: количество строк (n) должно быть меньше количества столбцов (m).");
//                             return;
//                         }
//                     }
//                 }

//                 if (n == -1 || m == -1)
//                 {
//                     Console.WriteLine("Не удалось прочитать размеры матрицы из файла.");
//                     return;
//                 }

//                 List<List<int>> matrix = new List<List<int>>();
//                 Random random = new Random();

//                 for (int i = 0; i < n; i++)
//                 {
//                     List<int> row = new List<int>();
//                     for (int j = 0; j < n; j++)
//                     {
//                         row.Add((i == j) ? 1 : 0);
//                     }
//                     for (int j = n; j < m; j++)
//                     {
//                         row.Add(random.Next(0, 2));
//                     }
//                     matrix.Add(row);
//                 }

//                 Console.WriteLine($"Порождающая матрица {n} на {m}:");
//                 for (int i = 0; i < n; i++)
//                 {
//                     Console.WriteLine(string.Join(" ", matrix[i]));
//                 }

//                 using (StreamWriter writer = new StreamWriter("output_matrix4.txt"))
//                 {
//                     writer.WriteLine($"{n} {m}");
//                     for (int i = 0; i < n; i++)
//                     {
//                         writer.WriteLine(string.Join(" ", matrix[i]));
//                     }
//                 }

//                 Console.WriteLine("Размерность кода: " + n);
//                 Console.WriteLine("Количество кодовых слов: " + Math.Pow(2, n));

//                 int minDistance = m + 1;
//                 for (int code = 0; code < matrix.Count; code++)
//                 {
//                     for (int i = code + 1; i < matrix.Count; i++)
//                     {
//                         int distance = 0;
//                         for (int j = 0; j < m; j++)
//                         {
//                             if (matrix[code][j] != matrix[i][j])
//                             {
//                                 distance++;
//                             }
//                         }
//                         if (distance < minDistance)
//                         {
//                             minDistance = distance;
//                         }
//                     }
//                 }
//                 Console.WriteLine("Минимальное кодовое расстояние: " + minDistance);
//             }
//             catch (FileNotFoundException)
//             {
//                 Console.WriteLine("Файл не найден: " + fileName);
//             }
//             catch (FormatException)
//             {
//                 Console.WriteLine("Неправильный формат данных в файле.");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Произошла ошибка: " + ex.Message);
//             }
//         }
//     }
// }

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static Random random = new Random();

    static int[] Num2Row(int num, int m)
    {
        return Convert.ToString(num, 2).PadLeft(m, '0').Select(c => c - '0').ToArray();
    }

    static int[] Mat2Num(int[][] mat)
    {
        return mat.Select(row => Convert.ToInt32(string.Join("", row), 2)).ToArray();
    }

    static Tuple<int, int, string, int[]> Gen(int n, int m)
    {
        if (m <= n) Environment.Exit(0); // Требуется матрица с избыточностью (m > n)
        
        int[][] mat = new int[n][];
        for (int row = 0; row < n; row++)
        {
            mat[row] = new int[m];
            for (int col = 0; col < m; col++)
            {
                mat[row][col] = (col == row) ? 1 : (col < n ? 0 : random.Next(0, 2));
            }
        }
        
        var G = new Tuple<int, int, string, int[]>(n, m, "G", Mat2Num(mat));
        return G;
    }

    static void PrintMat(Tuple<int, int, string, int[]> mat)
    {
        var (n, m, name, matrix) = mat;
        var it = matrix.GetEnumerator();
        it.MoveNext();
        Console.WriteLine($"{name} = [{string.Join(" ", Num2Row((int)it.Current, m))}]");
        while (it.MoveNext())
        {
            Console.WriteLine($"      [{string.Join(" ", Num2Row((int)it.Current, m))}]");
        }
    }

    static Tuple<int, int, string, int[]> G2H(Tuple<int, int, string, int[]> mat)
    {
        var (n, m, _, matrix) = mat;
        if (m <= n) Environment.Exit(0); // Требуется матрица с избыточностью (m > n)
        
        int m1 = m - 1;
        int[][] newMat = new int[m][];
        for (int row = 0; row < m; row++)
        {
            newMat[row] = new int[m - n];
            for (int col = n; col < m; col++)
            {
                if (row < n)
                {
                    newMat[row][col - n] = (matrix[row] >> (m1 - col)) & 1;
                }
                else
                {
                    newMat[row][col - n] = (col == row) ? 1 : 0;
                }
            }
        }
        
        var H = new Tuple<int, int, string, int[]>(m, m - n, "H", Mat2Num(newMat));
        return H;
    }

    static Tuple<int, int, string, int[]> Transpose(Tuple<int, int, string, int[]> mat)
    {
        var (n, m, name, matrix) = mat;
        int[][] transposed = new int[m][];
        for (int col = 0; col < m; col++)
        {
            transposed[col] = new int[n];
            for (int row = 0; row < n; row++)
            {
                transposed[col][row] = (matrix[row] >> (m - 1 - col)) & 1;
            }
        }
        
        var transposedMat = new Tuple<int, int, string, int[]>(m, n, "t" + name, Mat2Num(transposed));
        return transposedMat;
    }

    static string Mul(Tuple<int, int, string, int[]> mat, string code)
    {
        var (n, m, name, matrix) = mat;
        int res = 0;
        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] == '1')
            {
                res ^= matrix[i];
            }
        }
        return Convert.ToString(res, 2).PadLeft(m, '0');
    }

    static void Calculate(Tuple<int, int, string, int[]> G)
    {
        PrintMat(G);
        var H = G2H(G);
        PrintMat(H);
        var tH = Transpose(H);
        var (n, m, _, _) = G;
        int count = (int)Math.Pow(2, n);
        int minDist = int.MaxValue;
        
        for (int i = 0; i < count; i++)
        {
            string code = Convert.ToString(i, 2).PadLeft(n, '0');
            string encoded = Mul(G, code);
            
            int dist = encoded.Count(c => c == '1');
            if (dist > 0) minDist = Math.Min(minDist, dist);
            
            string checkedStr = Mul(H, encoded);
            if (i < 25 || i >= count - 25)
            {
                Console.WriteLine($"{code} {encoded} {checkedStr}");
            }
            else if (i == 25 && count > 50)
            {
                Console.WriteLine("...");
            }
        }
        
        Console.WriteLine($"Длина кода: {n}");
        Console.WriteLine($"Размерность кода: {m}");
        Console.WriteLine($"Количество кодовых слов: {count}");
        Console.WriteLine($"Минимальное кодовое расстояние: {minDist}");
    }

    static Tuple<int, int, string, int[]> Reader(string fname)
    {
        try
        {
            using (StreamReader file = new StreamReader(fname))
            {
                string[] header = file.ReadLine().Split(' ', 3);
                int n = int.Parse(header[0]);
                int m = int.Parse(header[1]);
                string name = header[2].Trim();
                
                int[][] mat = new int[n][];
                for (int i = 0; i < n; i++)
                {
                    mat[i] = file.ReadLine().Split(' ').Select(int.Parse).ToArray();
                }
                
                var G = new Tuple<int, int, string, int[]>(n, m, name, Mat2Num(mat));
                return G;
            }
        }
        catch (FileNotFoundException)
        {
            int n = random.Next(3, 11);
            int m = n + random.Next(2, 11);
            var G = Gen(n, m);
            
            using (StreamWriter file = new StreamWriter(fname))
            {
                file.WriteLine($"{n} {m} {G.Item3}");
                foreach (var row in G.Item4)
                {
                    file.WriteLine(string.Join(" ", Num2Row(row, m)));
                }
            }
            
            return G;
        }
    }

    static void Main(string[] args)
    {
        for (int i = 1; i <= 5; i++)
        {
            string name = $"lab4_mat{i}.txt";
            Console.WriteLine(new string('~', 33) + " " + name + " " + new string('~', 33));
            var G = Reader(name);
            Calculate(G);
        }
    }
}