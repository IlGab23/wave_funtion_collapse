
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WaveFunctionCollapse.Fnc;

using HttpClient httpClient = new HttpClient();

Stopwatch sw = new();
sw.Start();
int cicles = 0;
const int matrixSize = 31;

Console.WriteLine("--- Wave Function Collapse ---");
Console.WriteLine("Inizializzazione in corso...");

PriorityQueue<(int y, int x), int> positions = new();
Queue<(int y, int x)> collapsedCells = new();

List<char>[,] charMatrix = new List<char>[matrixSize, matrixSize];

(int y, int x) = UtilitiesFuncions.SetupEntropyMatrix(ref charMatrix, ref positions);
collapsedCells.Enqueue((y, x));

bool fullTiled = false;


while (true)
{
    Console.Clear();

    int posCount = positions.Count;
    cicles++;

    collapsedCells.TryDequeue(out var cell);
    UtilitiesFuncions.EditNearTiles(cell.y, cell.x, ref charMatrix, ref positions);


    UtilitiesFuncions.PrintTiles(ref charMatrix, true);

    Console.WriteLine($"Cicli per gen:{cicles}");
    Console.WriteLine($"N° of pos:{posCount}");

    fullTiled = UtilitiesFuncions.ForceCollapseNewTile(ref charMatrix, ref positions, ref collapsedCells);
    if (fullTiled) break;

    // Thread.Sleep(300);

}

sw.Stop();
Console.Clear();
UtilitiesFuncions.PrintTiles(ref charMatrix, true);

Console.WriteLine($"Cicli per gen:{cicles}");
Console.WriteLine($"Time of execution: {sw.Elapsed}");

Thread.Sleep(1000);
Console.WriteLine("Invio di info a Discord...");
Thread.Sleep(1000);

string webHookUrl = "https://discord.com/api/webhooks/1534525559655239680/JpOOCYlv7bA4rPhygdYgnWypNIQxKmiKkSSOlU0rwJjcN0aJzoiTT4aRGmYxHoqDFiMk";

var payload = new
{
    content = $"✅ **MATRICE GENERATA**\nDimensione Matrice: {matrixSize}x{matrixSize}\nCicli: {cicles}\nTempo di esecuzione : {sw.Elapsed}",
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
