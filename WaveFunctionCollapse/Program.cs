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

char[,] matrix = new char[11, 11];

if ((pos.x == -1 && pos.y == -1) && entropy == 0)
{
    int centerY = matrix.GetLength(0) / 2;
    int centerX = matrix.GetLength(1) / 2;

    char[] options = mapBiome.Keys.ToArray();

    Random rnd = new Random();

    int pickedBiome = rnd.Next(options.Length);
    matrix[centerY, centerX] = options[pickedBiome];
}

for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        if (matrix[i, j] == '\0')
        {
            Console.Write("|-|");
        }
        else
        {
            Console.ForegroundColor = mapBiome[matrix[i, j]].Color;
            Console.Write($"|{matrix[i, j]}|");
            Console.ResetColor();
        }
    }

    Console.WriteLine();
}

// I record e le classi possono essere dichiarati tranquillamente nello stesso file delle top-level statements, 
// purché si trovino ALLA FINE del file (dopo tutto il codice eseguibile).
public record Biome(string Nome, ConsoleColor Color);
