using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
namespace lab1;

class Program
{
    static Random rnd = new Random();
    static void Main(string[] args)
    {
        GenerateRandomFile("file1.txt", 10000, new char[] {'A', 'B', 'C'});
        GenerateProbabilityFile("file2.txt", 10000, new char[] {'A', 'B', 'C'}, new double[] {0.5, 0.3, 0.2});
        (int, int, int) counter = CountSymbols("file2.txt");
        Console.WriteLine($"Кол-во символов А: {counter.Item1}; кол-во символов B: {counter.Item2}; кол-во символов C: {counter.Item3}.");
        TransformText("test.txt", "file3.txt");

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
    }

    private static (int, int, int) CountSymbols(string fileName)
    {
        var text = File.ReadAllText(fileName);
        (int, int, int) result = (0, 0, 0);
        foreach (char c in text)
        {
            if (c == 'A') result.Item1++;
            if (c == 'B') result.Item2++;
            if (c == 'C') result.Item3++;
        }
        return result;
    }

    static void GenerateRandomFile(string fileName, int length, char[] symbols){
        using (StreamWriter writer = new StreamWriter(fileName)){
            for(int i = 0; i < length; i++){
                char symbol = symbols[rnd.Next(symbols.Length)];
                writer.Write(symbol);
            }
        }
    }

    static void GenerateProbabilityFile(string fileName, int length, char[] symbols, double[] probabilities){
        using (StreamWriter writer = new StreamWriter(fileName)){
            for(int i = 0; i < length; i++){
                double rand = rnd.NextDouble();
                double sum = 0.0;
                for(int j = 0; j < probabilities.Length; j++){
                    sum += probabilities[j];
                    if(rand < sum){
                        writer.Write(symbols[j]);
                        break;
                    }
                }
            }
        }
    }

    static void TransformText(string sourceFile, string destinationFile){
        string text = File.ReadAllText(sourceFile);
        text = text.ToLower();

        var sb = new StringBuilder();
        foreach(char c in text){
            if(char.IsLetter(c) || c != ' '){
                sb.Append(c);
            }
        }
        text = sb.ToString();
        File.WriteAllText(destinationFile, text);
    }

    static double CalculateEntropy(string fileName, int n){
        string text = File.ReadAllText(fileName);
        var frequencies = new Dictionary<string, int>();

        for(int i = 0; i < text.Length - n + 1; i++){
            string sequence = text.Substring(i, n);
            if(frequencies.ContainsKey(sequence)){
                frequencies[sequence]++;
            }
            else{
                frequencies[sequence] = 1;
            }
        }

        double entropy = 0.0;
        int total = text.Length - n + 1;

        foreach(var freq in frequencies){
            double probability = (double)freq.Value / total;
            entropy -= probability * Math.Log(probability, 2);
        }

        return entropy / n;
    }
}
