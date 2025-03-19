using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace lab2
{
    class Node
    {
        public string Symbol { get; set; }
        public int Frequency { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = { "file1.txt", "file2.txt", "file3.txt" };

            foreach (var file in files)
            {
                Console.WriteLine($"Processing {file}:");

                for (int i = 1; i <= 3; i++)
                {
                    var frequencies = GetFrequencies(file, i);

                    var huffmanCodes = HuffmanCoding(frequencies);
                    Console.WriteLine("Huffman codes: ");
                    //PrintCodes(huffmanCodes);
                    var shannonFanoCodes = ShannonFanoCoding(frequencies);
                    Console.WriteLine("ShennonFano codes: ");
                    //PrintCodes(shannonFanoCodes);

                    string huffmanEncodedText = EncodeText(File.ReadAllText(file), huffmanCodes, i);
                    string shannonFanoEncodedText = EncodeText(File.ReadAllText(file), shannonFanoCodes, i);

                    File.WriteAllText($"huffman_{i}_{file}", huffmanEncodedText);
                    File.WriteAllText($"shannon_fano_{i}_{file}", shannonFanoEncodedText);

                    double huffmanEntropy = CalculateEntropyForBinaryText(huffmanEncodedText, 1);
                    double shannonFanoEntropy = CalculateEntropyForBinaryText(shannonFanoEncodedText, 1);

                    double huffmanAvgLength = CalculateAverageCodeLength(frequencies, huffmanCodes);
                    double shannonFanoAvgLength = CalculateAverageCodeLength(frequencies, shannonFanoCodes);

                    Console.WriteLine($"For {i}-symbol groups:");
                    Console.WriteLine($"Huffman: Entropy = {huffmanEntropy}, Average Code Length = {huffmanAvgLength}, Redundancy = {huffmanAvgLength - huffmanEntropy}");
                    Console.WriteLine($"Shannon-Fano: Entropy = {shannonFanoEntropy}, Average Code Length = {shannonFanoAvgLength}, Redundancy = {shannonFanoAvgLength - shannonFanoEntropy}");
                    Console.WriteLine();
                }
            }
        }

        private static void PrintCodes(Dictionary<string, string> codes)
        {
            foreach (var entry in codes)
            {
                Console.WriteLine($"Symbol: {entry.Key}, Code: {entry.Value}");
            }
        }

        static Dictionary<string, int> GetFrequencies(string fileName, int n)
        {
            var text = File.ReadAllText(fileName);
            var frequencies = new Dictionary<string, int>();

            for (int i = 0; i < text.Length - n + 1; i++)
            {
                string sequence = text.Substring(i, n);
                if (frequencies.ContainsKey(sequence))
                {
                    frequencies[sequence]++;
                }
                else
                {
                    frequencies[sequence] = 1;
                }
            }

            return frequencies;
        }

        static Dictionary<string, string> HuffmanCoding(Dictionary<string, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<Node, int>();

            foreach (var kvp in frequencies)
            {
                priorityQueue.Enqueue(new Node { Symbol = kvp.Key, Frequency = kvp.Value }, kvp.Value);
            }

            while (priorityQueue.Count > 1)
            {
                var left = priorityQueue.Dequeue();
                var right = priorityQueue.Dequeue();

                var parent = new Node
                {
                    Symbol = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                priorityQueue.Enqueue(parent, parent.Frequency);
            }

            var root = priorityQueue.Dequeue();
            var codes = new Dictionary<string, string>();
            Traverse(root, "", codes);

            return codes;
        }

        static Dictionary<string, string> ShannonFanoCoding(Dictionary<string, int> frequencies)
        {
            var sortedFrequencies = frequencies.OrderByDescending(kvp => kvp.Value).ToList();
            var codes = new Dictionary<string, string>();
            ShannonFano(sortedFrequencies, 0, sortedFrequencies.Count - 1, "", codes);
            return codes;
        }

        static void ShannonFano(List<KeyValuePair<string, int>> frequencies, int start, int end, string code, Dictionary<string, string> codes)
        {
            if (start > end) return;

            if (start == end)
            {
                codes[frequencies[start].Key] = code;
                return;
            }

            int total = frequencies.Skip(start).Take(end - start + 1).Sum(kvp => kvp.Value);
            int sum = 0;
            int split = start;

            for (int i = start; i <= end; i++)
            {
                sum += frequencies[i].Value;
                if (sum >= total / 2)
                {
                    split = i;
                    break;
                }
            }

            ShannonFano(frequencies, start, split, code + "0", codes);
            ShannonFano(frequencies, split + 1, end, code + "1", codes);
        }

        static void Traverse(Node node, string code, Dictionary<string, string> codes)
        {
            if (node.Left == null && node.Right == null)
            {
                codes[node.Symbol] = code;
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }

        static string EncodeText(string text, Dictionary<string, string> codes, int n)
        {
            var encodedText = new StringBuilder();

            for (int i = 0; i < text.Length - n + 1; i++)
            {
                string sequence = text.Substring(i, n);
                encodedText.Append(codes[sequence]);
            }

            return encodedText.ToString();
        }

        static double CalculateAverageCodeLength(Dictionary<string, int> frequencies, Dictionary<string, string> codes)
        {
            int total = frequencies.Values.Sum();
            double avgLength = 0;

            foreach (var kvp in frequencies)
            {
                avgLength += (double)kvp.Value / total * codes[kvp.Key].Length;
            }

            return avgLength;
        }

        static double CalculateEntropyForBinaryText(string binaryText, int n)
        {
            var frequencies = new Dictionary<string, int>();

            for (int i = 0; i < binaryText.Length - n + 1; i++)
            {
                string sequence = binaryText.Substring(i, n);
                if (frequencies.ContainsKey(sequence))
                {
                    frequencies[sequence]++;
                }
                else
                {
                    frequencies[sequence] = 1;
                }
            }

            double entropy = 0.0;
            int total = binaryText.Length - n + 1;

            foreach (var freq in frequencies)
            {
                double probability = (double)freq.Value / total;
                entropy -= probability * Math.Log(probability, 2);
            }

            return entropy / n;
        }
    }
}