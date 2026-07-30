// In .NET (dalla versione 6 in poi), i "Top-Level Statements" sono lo standard moderno e consigliato per le applicazioni Console.
// Non è più necessario dichiarare esplicitamente la classe 'Program' e il metodo 'Main', riducendo il codice boilerplate.
// Dietro le quinte, il compilatore genera automaticamente il metodo Main per noi.

Console.WriteLine("--- Wave Function Collapse ---");
Console.WriteLine("Inizializzazione in corso...");

Dictionary<char, Biome> mapBiome = new Dictionary<char, Biome>()
{
    {'G', new Biome("Grass", ConsoleColor.Green)},
    {'F', new Biome("Forest", ConsoleColor.DarkGreen)},
    {'D', new Biome("Desert", ConsoleColor.Yellow)},
    {'W', new Biome("Water", ConsoleColor.Blue)}
};

(int x, int y) pos = (-1, -1);
int entropy = 0;

char[,] charMatrix = new char[11, 11];
int[,] entropyMatrix = new int[11, 11];

if ((pos.x == -1 && pos.y == -1) && entropy == 0)
{
    int centerY = charMatrix.GetLength(0) / 2;
    int centerX = charMatrix.GetLength(1) / 2;

    char[] options = mapBiome.Keys.ToArray();

    Random rnd = new Random();

    int pickedBiome = rnd.Next(options.Length);
    charMatrix[centerY, centerX] = options[pickedBiome];
}

for (int i = 0; i < entropyMatrix.GetLength(0); i++)
{
    for (int j = 0; j < entropyMatrix.GetLength(1); j++)
    {
        if (charMatrix[i, j] == '\0')
        {
            entropyMatrix[i, j] = 4;
        }
    }
}

for (int i = 0; i < charMatrix.GetLength(0); i++)
{
    for (int j = 0; j < charMatrix.GetLength(1); j++)
    {
        if (charMatrix[i, j] == '\0')
        {
            Console.Write("|-|");
        }
        else
        {
            Console.ForegroundColor = mapBiome[charMatrix[i, j]].Color;
            Console.Write($"|{charMatrix[i, j]}|");
            Console.ResetColor();
        }
    }

    Console.WriteLine();
}

for (int i = 0; i < entropyMatrix.GetLength(0); i++)
{
    for (int j = 0; j < entropyMatrix.GetLength(1); j++)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"|{entropyMatrix[i, j]}|");
        Console.ResetColor();
    }

    Console.WriteLine();
}

// I record e le classi possono essere dichiarati tranquillamente nello stesso file delle top-level statements, 
// purché si trovino ALLA FINE del file (dopo tutto il codice eseguibile).
public record Biome(string Nome, ConsoleColor Color);
