
using System.Diagnostics;
using WaveFunctionCollapse.Fnc;

Stopwatch sw = new();
sw.Start();
int cicles = 0;
const int matrixSize = 31;

Console.WriteLine("--- Wave Function Collapse ---");
Console.WriteLine("Inizializzazione in corso...");

List<(int y, int x)> positions = new();

// char[,] charMatrix = new char[11, 11];
int[,] entropyMatrix = new int[matrixSize, matrixSize];

List<char>[,] charMatrix = new List<char>[matrixSize, matrixSize];

UtilitiesFuncions.SetupEntropyMatrix(ref charMatrix, ref positions);

bool fullTiled = false;
int tilesAdded = 0;

int forceCollapseMethodCalls = 0;

while (true)
{
    Console.Clear();

    int posCount = positions.Count;
    // Console.WriteLine($"BEFORE FIRST DELAY: POS => {posCount}");
    // Thread.Sleep(1000);
    cicles++;

    UtilitiesFuncions.EditNearTiles(positions[0].y, positions[0].x, ref charMatrix);


    UtilitiesFuncions.PrintTiles(ref charMatrix, true);
    Console.WriteLine($"Tiles Added: {tilesAdded}");

    // Console.BackgroundColor = ConsoleColor.Cyan;
    // foreach (var pos in positions)
    // {
    //     Console.WriteLine(pos.ToString());
    // }
    // Console.ResetColor();

    Console.WriteLine($"Cicli per gen:{cicles}");
    Console.WriteLine($"N° of pos:{posCount}");
    Console.WriteLine("BEFORE SECOND DELAY");

    fullTiled = UtilitiesFuncions.ForceCollapseNewTile(ref charMatrix, ref positions);
    if (fullTiled) break;

    // Thread.Sleep(300);

}

Console.Clear();
UtilitiesFuncions.PrintTiles(ref charMatrix, true);
// Console.BackgroundColor = ConsoleColor.Cyan;
// foreach (var pos in positions)
// {
//     Console.WriteLine(pos.ToString());
// }
// Console.ResetColor();

Console.WriteLine($"Cicli per gen:{cicles}");
Console.WriteLine($"Chiamate al force collapse:{forceCollapseMethodCalls}");
sw.Stop();
Console.WriteLine($"Time of execution: {sw.Elapsed}");

// //TEMP PRINT
// for (int i = 0; i < entropyMatrix.GetLength(0); i++)
// {
//     for (int j = 0; j < entropyMatrix.GetLength(1); j++)
//     {
//         Console.ForegroundColor = ConsoleColor.White;
//         Console.Write($"|{entropyMatrix[i, j]}|");
//         Console.ResetColor();
//     }

//     Console.WriteLine();
// }
