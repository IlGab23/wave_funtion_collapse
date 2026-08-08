using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WaveFunctionCollapse.Fnc;

using HttpClient httpClient = new HttpClient();
DotNetEnv.Env.Load();

Stopwatch sw = new();
sw.Start();
int cicles = 0;
const int matrixSize_y = 31;
const int matrixSize_x = 31;
int totalMatrixCells = matrixSize_y * matrixSize_x;

Console.WriteLine("--- Wave Function Collapse ---");
Console.WriteLine("Inizializzazione in corso...");

PriorityQueue<(int y, int x), double> positions = new();
Queue<(int y, int x)> collapsedCells = new();
Dictionary<char, int> biomesTileCount = new();

byte[,] charMatrix = new byte[matrixSize_y, matrixSize_x];
LinkedList<byte[,]> history = new();
int maxHistory = 50;
int fallbackCount = 0;

(int y, int x) = UtilitiesFuncions.SetupEntropyMatrix(ref charMatrix, ref positions, ref biomesTileCount);
collapsedCells.Enqueue((y, x));

bool fullTiled = false;


while (true)
{
    Console.Clear();

    cicles++;

    collapsedCells.TryDequeue(out var cell);
    bool isSuccess = UtilitiesFuncions.EditNearTiles(cell.y, cell.x, ref charMatrix, ref positions, ref biomesTileCount, totalMatrixCells);


    UtilitiesFuncions.PrintTiles(ref charMatrix, true);

    if (!isSuccess)
    {
        Console.WriteLine("CONTRADDIZIONE RILEVATA! Avvio Rollback...");
        fallbackCount++;

        if (history.Count == 0)
        {
            Console.WriteLine("Errore critico irrisolvibile (vicolo cieco troppo profondo). RIAVVIO!");
            Environment.Exit(1);
        }

        byte wrongBiome = charMatrix[cell.y, cell.x]; // Questo è il singolo bit rimasto
        charMatrix = history.Last.Value;
        history.RemoveLast();

        // Rimuoviamo il bioma sagliato spegnendo il bit corrispondente
        charMatrix[cell.y, cell.x] &= (byte)~wrongBiome;

        double totalWeight = UtilitiesFuncions.CalculateTotalWeight(charMatrix[cell.y, cell.x]);
        double newEntropy = UtilitiesFuncions.CalculateShannonEntropy(charMatrix[cell.y, cell.x], totalWeight, cell.y, cell.x);
        positions.Enqueue((cell.y, cell.x), newEntropy);
    }

    Console.WriteLine($"Cicli per gen:{cicles}");

    history.AddLast((byte[,])charMatrix.Clone());
    if (history.Count > maxHistory)
    {
        history.RemoveFirst();
    }

    fullTiled = UtilitiesFuncions.ForceCollapseNewTile(ref charMatrix, ref positions, ref collapsedCells, ref biomesTileCount);
    if (fullTiled) break;

    // Thread.Sleep(1300);

}

UtilitiesFuncions.CleanUpMap(ref charMatrix);

sw.Stop();
// Console.Clear();
UtilitiesFuncions.PrintTiles(ref charMatrix, true);

Console.WriteLine($"Cicli per gen:{cicles}");
Console.WriteLine($"Fallbacks:{fallbackCount}");
Console.WriteLine($"Time of execution: {sw.Elapsed}");

foreach (var ele in biomesTileCount)
{
    Console.Write($"{ele.Key} <-> {ele.Value} || ");
}

Thread.Sleep(1000);
Console.WriteLine("Invio di info a Discord...");
Thread.Sleep(1000);

string? webHookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");
if (string.IsNullOrEmpty(webHookUrl))
{
    Console.WriteLine("Discord webhook URL is not set in .env file.");
    return;
}

var payload = new
{
    content = $"✅ **MATRICE GENERATA**\nDimensione Matrice: {matrixSize_y}x{matrixSize_x}\nCicli: {cicles}\nTempo di esecuzione: {sw.Elapsed}\nError Fallback: {fallbackCount}",
    username = "WFC Engine - Map generator"
};

string jsonPayload = JsonSerializer.Serialize(payload);
using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

try
{
    HttpResponseMessage response = await httpClient.PostAsync(webHookUrl, content);

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Notifica inviata!");
    }
    else
    {
        Console.WriteLine($"Errore API Discord: {response.StatusCode}");
    }
}
catch (Exception e)
{
    Console.WriteLine($"Connection error on discord: {e.Message}");
}
