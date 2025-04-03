
// using System.Text;

// namespace lab5;

// class Program
// {
//     static void Main(string[] args)
//     {
//         string inputFile = "huffman_1_file1.txt";
//         double[] errorProbabilities = {0.0001, 0.001, 0.01, 0.1};
//         string binaryText = File.ReadAllText(inputFile);

//         foreach(double p in errorProbabilities)
//         {
//             Console.WriteLine($"Вероятность ошибки: {p}");

//             string hammingEncodedText = EncodeHamming(binaryText);
//             File.WriteAllText($"hamming_encoded_p_{p}.txt", hammingEncodedText);

//             string corruptedText = IntroduceErrors(hammingEncodedText, p);
//             File.WriteAllText($"corrupted_p_{p}.txt", corruptedText);

//             string decodedText = DecodeHamming(corruptedText);
//             File.WriteAllText($"decoded_p_{p}.txt", decodedText);

//             int errorCount = CompareFiles(binaryText, decodedText);
//             Console.WriteLine($"Количество ошибок после декодирования: {errorCount}");
//             Console.WriteLine();
//         }
//     }

//     private static string EncodeHamming(string binaryText)
//     {
//         StringBuilder encodedText = new StringBuilder();

//         for(int i = 0; i < binaryText.Length; i += 4)
//         {
//             string data = binaryText.Substring(i, Math.Min(4, binaryText.Length - i));
//             string encodedBlock = EncodeHammingBlock(data);
//             encodedText.Append(encodedBlock);
//         }
//         return encodedText.ToString();
//     }

//     private static string EncodeHammingBlock(string data)
//     {
//         if(data.Length < 4)
//         {
//             data = data.PadRight(4, '0');
//         }

//         int[] bits = data.Select(c => c - '0').ToArray();
//         int[] encodedBits = new int[7];

//         encodedBits[2] = bits[0];
//         encodedBits[4] = bits[1];
//         encodedBits[5] = bits[2];
//         encodedBits[6] = bits[3];

//         encodedBits[0] = encodedBits[2] ^ encodedBits[4] ^ encodedBits[6];
//         encodedBits[1] = encodedBits[2] ^ encodedBits[5] ^ encodedBits[6];
//         encodedBits[3] = encodedBits[4] ^ encodedBits[5] ^ encodedBits[6];

//         return string.Join("", encodedBits);
//     }

//     private static string IntroduceErrors(string text, double errorProbability)
//     {
//         Random rnd = new Random();
//         StringBuilder corruptedText = new StringBuilder();

//         foreach(char c in text)
//         {
//             if(rnd.NextDouble() < errorProbability)
//             {
//                 corruptedText.Append(c == '0' ? '1' : '0');
//             }
//             else
//             {
//                 corruptedText.Append(c);
//             }
//         }

//         return corruptedText.ToString();
//     }

//     private static string DecodeHamming(string encodedText)
//     {
//         StringBuilder decodedText = new StringBuilder();

//         for(int i = 0; i < encodedText.Length; i += 7)
//         {
//             string block = encodedText.Substring(i, Math.Min(7, encodedText.Length - i));
//             string decodedBlock = DecodeHammingBlock(block);
//             decodedText.Append(decodedBlock);
//         }

//         return decodedText.ToString();
//     }

//     private static string DecodeHammingBlock(string block)
//     {
//         int[] bits = block.Select(c => c - '0').ToArray();

//         int p1 = bits[0] ^ bits[2] ^ bits[4] ^ bits[6];
//         int p2 = bits[1] ^ bits[2] ^ bits[5] ^ bits[6];
//         int p3 = bits[3] ^ bits[4] ^ bits[5] ^ bits[6];

//         int errorPosition = p1 + p2 * 2 + p3 * 4;

//         if (errorPosition != 0)
//         {
//             bits[errorPosition - 1] ^= 1; // Исправление ошибки
//         }

//         return $"{bits[2]}{bits[4]}{bits[5]}{bits[6]}";
//     }

//     private static int CompareFiles(string original, string decoded)
//     {
//         int errorCount = 0;

//         for(int i = 0; i < Math.Min(original.Length, decoded.Length); i++)
//         {
//             if(original[i] != decoded[i])
//             {
//                 errorCount++;
//             }
//         }

//         return errorCount;
//     }
// }

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static Random random = new Random();

    static ((int, Func<string, string>), (int, Func<string, string>)) CoderGenerator(int serviceBits)
    {
        if (serviceBits < 2)
        throw new ArgumentException("Service bits must be at least 2");

        int bits = (1 << serviceBits) - 1;
        int commonBits = bits - serviceBits;

        // Формирование массива связей битов
        var arr = new List<object>();
        int common = 0;
        for (int i = 0; i < bits; i++)
        {
            bool isServiceBit = (i & (i + 1)) == 0; // Проверка на степень двойки минус 1
            if (isServiceBit)
            {
                int n = (int)Math.Log(i + 1, 2) + 1;
                int start = 1 << n;
                int count = bits - start;
                
                // Добавляем проверку на неотрицательное количество
                var indices = count > 0 
                    ? Enumerable.Range(start, count)
                        .Where(idx => ((idx + 1) >> n & 1) == 1)
                        .ToArray()
                    : Array.Empty<int>();
                
                arr.Add(indices);
            }
            else
            {
                arr.Add(common);
                common++;
            }
        }

        // Преобразование индексов
        for (int i = 0; i < arr.Count; i++)
        {
            if (arr[i] is int[])
            {
                var oldIndices = (int[])arr[i];
                var newIndices = oldIndices.Select(idx => (int)arr[idx]).ToArray();
                arr[i] = newIndices;
            }
        }

        // Функция кодирования
        Func<string, string> encoder = code =>
        {
            if (code.Length != commonBits)
                throw new ArgumentException($"Длина кодируемого блока (={code.Length}) не соответствует {commonBits}");

            int[] ints = code.Select(c => c - '0').ToArray();
            var res = new StringBuilder();
            foreach (var item in arr)
            {
                int bit;
                if (item is int[])
                {
                    bit = 0;
                    foreach (int idx in (int[])item)
                        bit ^= ints[idx];
                }
                else
                {
                    bit = ints[(int)item];
                }
                res.Append(bit);
            }
            return res.ToString();
        };

        var commonIdxs = arr.Select((item, index) => new { item, index })
                           .Where(x => x.item is int)
                           .Select(x => x.index)
                           .ToArray();

        var checkIdxs = Enumerable.Range(0, serviceBits)
            .Select(i => (
                num: 1 << i,
                indices: Enumerable.Range(0, bits)
                    .Where(num => ((num + 1) >> i & 1) == 1)
                    .ToArray()
            ))
            .ToArray();

        // Функция декодирования
        Func<string, string> decoder = code =>
        {
            if (code.Length != bits)
                throw new ArgumentException($"Длина декодируемого блока (={code.Length}) не соответствует {bits}");

            int check = 0;
            foreach (var (num, indices) in checkIdxs)
            {
                if (indices.Sum(idx => code[idx] - '0') % 2 == 1)
                    check += num;
            }

            if (check > 0)
            {
                check--;
                if (check < code.Length)
                {
                    char flipped = code[check] == '0' ? '1' : '0';
                    code = code.Substring(0, check) + flipped + code.Substring(check + 1);
                }
            }

            return new string(commonIdxs.Select(idx => code[idx]).ToArray());
        };

        return ((commonBits, encoder), (bits, decoder));
    }

    static void EncodeFile(TextReader input, TextWriter output, Func<string, string> encoder, int commonBits)
    {
        char[] buffer = new char[commonBits];
        while (true)
        {
            int read = input.Read(buffer, 0, commonBits);
            if (read == 0) break;

            string data = new string(buffer, 0, read);
            if (read < commonBits)
                data += new string('0', commonBits - read);

            output.Write(encoder(data));
        }
    }

    static void BreakFile(TextReader input, TextWriter output, double p)
    {
        int threshold = (int)Math.Round(1 / p);
        Console.WriteLine($"1 к {threshold}");

        while (true)
        {
            int c = input.Read();
            if (c == -1) break;

            char ch = (char)c;
            if (random.Next(1, threshold + 1) == 1)
                ch = ch == '0' ? '1' : '0';

            output.Write(ch);
        }
    }

    static void DecodeFile(TextReader input, TextWriter output, Func<string, string> decoder, int bits)
    {
        char[] buffer = new char[bits];
        while (true)
        {
            int read = input.Read(buffer, 0, bits);
            if (read == 0) break;

            string data = new string(buffer, 0, read);
            output.Write(decoder(data));
        }
    }

    static int CheckFiles(TextReader file1, TextReader file2)
    {
        int errors = 0;
        while (true)
        {
            int c1 = file1.Read();
            int c2 = file2.Read();

            if (c1 == -1 || c2 == -1) break;
            if (c1 != c2) errors++;
        }
        return errors;
    }

    static void FinalSolve(string name, int serviceBits)
    {
        Directory.CreateDirectory("encoded");
        var ((commonBits, encoder), (bits, decoder)) = CoderGenerator(serviceBits);

        string name2 = $"encoded/sb{serviceBits}.txt";
        if (!File.Exists(name2))
        {
            using (var input = new StreamReader(name))
            using (var output = new StreamWriter(name2))
            {
                EncodeFile(input, output, encoder, commonBits);
            }
        }

        double[] probabilities = { 0.0001, 0.001, 0.01, 0.1, 0.25, 0.5, 1 };
        foreach (double p in probabilities)
        {
            string name3 = $"encoded/sb{serviceBits}_p{p}.txt";
            if (!File.Exists(name3))
            {
                using (var input = new StreamReader(name2))
                using (var output = new StreamWriter(name3))
                {
                    BreakFile(input, output, p);
                }
            }
        }

        var results = new List<int>();
        string originalContent = File.ReadAllText(name);
        using (var originalReader = new StringReader(originalContent))
        {
            foreach (double p in probabilities)
            {
                string name3 = $"encoded/sb{serviceBits}_p{p}.txt";
                var output = new StringWriter();
                using (var input = new StreamReader(name3))
                {
                    DecodeFile(input, output, decoder, bits);
                }

                int errors = CheckFiles(new StringReader(originalContent), new StringReader(output.ToString()));
                Console.WriteLine($"{name3}  | errors: {errors}");
                results.Add(errors);
            }
        }

        Console.WriteLine(string.Join("\t", results) + "\n");
    }

    static void Main(string[] args)
    {
        try
        {
            FinalSolve("huffman_1_file1.txt", 2);
            FinalSolve("huffman_1_file1.txt", 3);
            FinalSolve("huffman_1_file1.txt", 4);
            FinalSolve("huffman_1_file1.txt", 5);
            FinalSolve("huffman_1_file1.txt", 6);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }
}