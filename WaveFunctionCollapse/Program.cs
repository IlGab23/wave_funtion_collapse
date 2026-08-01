
using WaveFunctionCollapse.Domain;
using WaveFunctionCollapse.Fnc;

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

while (true)
{
    int tilesAdded = 0;
    Console.Clear();

    int posCount = positions.Count;
    // Console.WriteLine($"BEFORE FIRST DELAY: POS => {posCount}");
    // Thread.Sleep(1000);
    cicles++;

    for (int i = 0; i < posCount; i++)
    {
        var newTiles = UtilitiesFuncions.EditNearTiles(positions[i].y, positions[i].x, positions[i].y - 1, ref charMatrix);

        if (newTiles.Count == 0)
        {
            // positions.Clear();
            // Console.WriteLine($"SEARCH FOR NEW TILE FLAG INSIDE FOR ==> {searchForNewTile}");
            // Thread.Sleep(5000);
            break;
        }

        foreach (var tile in newTiles)
        {
            // Verifichiamo se il tile è già presente nella lista per evitare duplicati
            if (!positions.Contains(tile))
            {
                positions.Add(tile);
                tilesAdded++;
                continue;
            }
        }
    }


    UtilitiesFuncions.PrintTiles(ref charMatrix, true);
    Console.WriteLine($"Tiles Added: {tilesAdded}");

    // Console.BackgroundColor = ConsoleColor.Cyan;
    // foreach (var pos in positions)
    // {
    //     Console.WriteLine(pos.ToString());
    // }
    // Console.ResetColor();

    /// <summary>
    /// L'ARLGORITMO SI BLOCCA SU UN TILE DURANTE IL PROCESSO: DA CAPIRE SE E' PER UN ERRORE LOGICO OPPURE BISOGNA AGGIUNGERE
    /// UNO SBLOCCO SELEZIONANDO UN'ALTRA CASELLA DISPOLIBILE PER IL COLLASSO
    /// </summary>

    Console.WriteLine($"Cicli per gen:{cicles}");
    Console.WriteLine($"N° of pos:{posCount}");
    Console.WriteLine("BEFORE SECOND DELAY");

    if (tilesAdded == 0)
    {
        fullTiled = UtilitiesFuncions.ForceCollapseNewTile(ref charMatrix, ref positions);
        if (fullTiled) break;
        // break;
    }
    else
    {
        positions.RemoveRange(0, posCount);
    }
    Thread.Sleep(100);

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
