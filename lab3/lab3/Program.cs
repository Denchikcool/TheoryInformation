namespace lab3;

public class HuffmanNode : IComparable<HuffmanNode>
{
    public string Symbol { get; set; }
    public int Frequency { get; set; }
    public HuffmanNode Left { get; set; }
    public HuffmanNode Right { get; set; }

    public int CompareTo(HuffmanNode other)
    {
        return Frequency - other.Frequency;
    }
}

public class Huffman
{
    public static Dictionary<string, string> BuildHuffmanTree(Dictionary<string, int> frequencies)
    {
        var priorityQueue = new PriorityQueue<HuffmanNode>();

        foreach (var symbol in frequencies)
        {
            priorityQueue.Enqueue(new HuffmanNode() { Symbol = symbol.Key, Frequency = symbol.Value });
        }

        while (priorityQueue.Count > 1)
        {
            var left = priorityQueue.Dequeue();
            var right = priorityQueue.Dequeue();

            var parent = new HuffmanNode()
            {
                Symbol = null,
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            };

            priorityQueue.Enqueue(parent);
        }

        var root = priorityQueue.Dequeue();
        var codes = new Dictionary<string, string>();
        AssignCodes(root, "", codes);
        return codes;
    }

    private static void AssignCodes(HuffmanNode node, string code, Dictionary<string, string> codes)
    {
        if (node == null)
                return;

        if (node.Symbol != null)
        {
            codes[node.Symbol] = code;
        }

        AssignCodes(node.Left, code + "0", codes);
        AssignCodes(node.Right, code + "1", codes);
    }
}

public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> data;

    public PriorityQueue()
    {
        this.data = new List<T>();
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int ci = data.Count - 1;
        while (ci > 0)
        {
            int pi = (ci - 1) / 2;
            if (data[ci].CompareTo(data[pi]) >= 0)
                break;
            T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
            ci = pi;
        }
    }

    public T Dequeue()
    {
        int li = data.Count - 1;
        T frontItem = data[0];
        data[0] = data[li];
        data.RemoveAt(li);

        --li;
        int pi = 0;
        while (true)
        {
            int ci = pi * 2 + 1;
            if (ci > li) break;
            int rc = ci + 1;
            if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
                ci = rc;
            if (data[pi].CompareTo(data[ci]) <= 0) break;
            T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
            pi = ci;
        }
        return frontItem;
    }

    public int Count
    {
        get { return data.Count; }
    }
}
class Program
{
    static void Main(string[] args)
    {
        string[] files = {"file1.txt", "file2.txt", "file3.txt"};
        Console.WriteLine("Энтропия файла 1: ");
        Console.WriteLine("Оценка 1: " + CalculateEntropy("file1.txt", 1));
        Console.WriteLine("Оценка 2: " + CalculateEntropy("file1.txt", 2));
        Console.WriteLine("Оценка 3: " + CalculateEntropy("file1.txt", 3));

        Console.WriteLine("Энтропия файла 2: ");
        Console.WriteLine("Оценка 1: " + CalculateEntropy("file2.txt", 1));
        Console.WriteLine("Оценка 2: " + CalculateEntropy("file2.txt", 2));
        Console.WriteLine("Оценка 3: " + CalculateEntropy("file2.txt", 3));

        Console.WriteLine("Энтропия файла 3: ");
        Console.WriteLine("Оценка 1: " + CalculateEntropy("file3.txt", 1));
        Console.WriteLine("Оценка 2: " + CalculateEntropy("file3.txt", 2));
        Console.WriteLine("Оценка 3: " + CalculateEntropy("file3.txt", 3));

        foreach(var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            for (int blockSize = 1; blockSize <= 4; blockSize++)
            {
                BlockEncode(file, $"encoded_{fileName}_block{blockSize}.txt", blockSize);
                double avgCodeLength = CalculateAvgCodeLength(file, blockSize);
                double redundancy = CalculateRedundancy(file, blockSize);
                Console.WriteLine($"Средняя длина кодового слова в {file} для блока размером {blockSize}: {avgCodeLength}");
                Console.WriteLine($"Избыточность в {file} для блока размером {blockSize}: {redundancy}");
            }
            if(file == "file3.txt") break;
            Console.WriteLine($"\nТеоретическая избыточность для {file}:");
            for (int theoreticalBlockSize = 1; theoreticalBlockSize <= 4; theoreticalBlockSize++)
            {
                double theoreticalRedundancy = CalculateTheoreticalRedundancy(file, theoreticalBlockSize);
                Console.WriteLine($"  Блок {theoreticalBlockSize}: {theoreticalRedundancy}");
            }
        }
    }

    static double CalculateAvgCodeLength(string fileName, int blockSize)
    {
        string text = File.ReadAllText(fileName);
        var frequencies = new Dictionary<string, int>();

        for (int i = 0; i < text.Length - blockSize + 1; i += blockSize)
        {
            string block = text.Substring(i, Math.Min(blockSize, text.Length - i));
            if (frequencies.ContainsKey(block))
            {
                frequencies[block]++;
            }
            else
            {
                frequencies[block] = 1;
            }
        }

        var huffmanCodes = Huffman.BuildHuffmanTree(frequencies);

        double averageCodeLength = 0.0;
        int totalBlocks = 0;

        foreach (var freq in frequencies)
        {
            averageCodeLength += (double)freq.Value * huffmanCodes[freq.Key].Length;
            totalBlocks += freq.Value;
        }

        return averageCodeLength / totalBlocks;
    }

    static void BlockEncode(string inputFile, string outputFile, int blockSize)
    {
        string text = File.ReadAllText(inputFile);
        var frequencies = new Dictionary<string, int>();

        for (int i = 0; i < text.Length - blockSize + 1; i += blockSize)
        {
            string block = text.Substring(i, blockSize);
            if (frequencies.ContainsKey(block))
            {
                frequencies[block]++;
            }
            else
            {
                frequencies[block] = 1;
            }
        }

        var huffmanCodes = Huffman.BuildHuffmanTree(frequencies);

        using (var writer = new StreamWriter(outputFile))
        {
            for (int i = 0; i < text.Length - blockSize + 1; i += blockSize)
            {
                string block = text.Substring(i, blockSize);
                writer.Write(huffmanCodes[block]);
            }
        }
    }

    static double CalculateRedundancy(string inputFile, int blockSize)
    {
        string text = File.ReadAllText(inputFile);
        var frequencies = new Dictionary<string, int>();

        for (int i = 0; i < text.Length - blockSize + 1; i += blockSize)
        {
            string block = text.Substring(i, blockSize);
            if (frequencies.ContainsKey(block))
            {
                frequencies[block]++;
            }
            else
            {
                frequencies[block] = 1;
            }
        }

        var huffmanCodes = Huffman.BuildHuffmanTree(frequencies);

        double averageCodeLength = 0.0;
        int total = text.Length - blockSize + 1;

        foreach (var freq in frequencies)
        {
            double probability = (double)freq.Value / total;
            averageCodeLength += probability * huffmanCodes[freq.Key].Length;
        }

        double entropy = CalculateEntropy(inputFile, blockSize);
        return (averageCodeLength - entropy) / entropy;
    }

    static double CalculateEntropy(string fileName, int n){
        string text = File.ReadAllText(fileName);
        var frequencies = new Dictionary<string, int>();
        int blockCount = 0;

        for (int i = 0; i < text.Length - n + 1; i += n)
        {
            string sequence = text.Substring(i, Math.Min(n, text.Length - i));
            if (frequencies.ContainsKey(sequence))
            {
                frequencies[sequence]++;
            }
            else
            {
                frequencies[sequence] = 1;
            }
            blockCount++;
        }

        double entropy = 0.0;

        foreach (var freq in frequencies)
        {
            double probability = (double)freq.Value / blockCount;
            entropy -= probability * Math.Log(probability, 2);
        }

        return entropy / n;
    }

    static double CalculateTheoreticalRedundancy(string inputFile, int blockSize)
    {
        string text = File.ReadAllText(inputFile);
        var symbolFrequencies = new Dictionary<char, double>();

        foreach (char c in text)
        {
            if (symbolFrequencies.ContainsKey(c))
            {
                symbolFrequencies[c]++;
            }
            else
            {
                symbolFrequencies[c] = 1;
            }
        }

        double totalSymbols = text.Length;
        foreach (var key in symbolFrequencies.Keys.ToList())
        {
            symbolFrequencies[key] /= totalSymbols;
        }

        var blockFrequencies = new Dictionary<string, double>();

        GenerateBlocks(symbolFrequencies, "", blockSize, blockFrequencies);

        var huffmanCodes = BuildHuffmanTreeFromProbabilities(blockFrequencies);

        double averageCodeLength = 0.0;
        foreach (var block in blockFrequencies)
        {
            averageCodeLength += block.Value * huffmanCodes[block.Key].Length;
        }

        double entropy = 0.0;
        foreach (var block in blockFrequencies)
        {
            entropy -= block.Value * Math.Log(block.Value, 2);
        }

        return (averageCodeLength - entropy) / entropy;
    }

    static void GenerateBlocks(Dictionary<char, double> symbolFrequencies, string currentBlock, int blockSize, Dictionary<string, double> blockFrequencies)
    {
        if (currentBlock.Length == blockSize)
        {
            double blockProbability = 1.0;
            foreach (char c in currentBlock)
            {
                blockProbability *= symbolFrequencies[c];
            }
            blockFrequencies[currentBlock] = blockProbability;
            return;
        }

        foreach (var symbol in symbolFrequencies)
        {
            GenerateBlocks(symbolFrequencies, currentBlock + symbol.Key, blockSize, blockFrequencies);
        }
    }

    static Dictionary<string, string> BuildHuffmanTreeFromProbabilities(Dictionary<string, double> probabilities)
    {
        var priorityQueue = new PriorityQueue<HuffmanNode>();

        foreach (var symbol in probabilities)
        {
            priorityQueue.Enqueue(new HuffmanNode() { Symbol = symbol.Key, Frequency = (int)(symbol.Value * 100000) });
        }

        while (priorityQueue.Count > 1)
        {
            var left = priorityQueue.Dequeue();
            var right = priorityQueue.Dequeue();

            var parent = new HuffmanNode()
            {
                Symbol = null,
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            };

            priorityQueue.Enqueue(parent);
        }

        var root = priorityQueue.Dequeue();
        var codes = new Dictionary<string, string>();
        AssignCodes(root, "", codes);
        return codes;
    }

    private static void AssignCodes(HuffmanNode node, string code, Dictionary<string, string> codes)
    {
        if (node == null)
                return;

        if (node.Symbol != null)
        {
            codes[node.Symbol] = code;
        }

        AssignCodes(node.Left, code + "0", codes);
        AssignCodes(node.Right, code + "1", codes);
    }
}
