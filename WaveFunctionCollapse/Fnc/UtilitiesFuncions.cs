using System.Net;
using WaveFunctionCollapse.Domain;

namespace WaveFunctionCollapse.Fnc;

public static class UtilitiesFuncions
{
    public static void EditNearTiles(int y, int x, ref List<char>[,] charMatrix, ref PriorityQueue<(int y, int x), int> pos)
    {
        (int dy, int dx)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        Queue<(int y, int x)> propagationQueue = new();
        propagationQueue.Enqueue((y, x));

        while (propagationQueue.Count > 0)
        {
            var currentCell = propagationQueue.Dequeue();
            int cy = currentCell.y;
            int cx = currentCell.x;

            List<char> notAllowed = BiomeDef.mapBiome[charMatrix[cy, cx].FirstOrDefault()].NotAllowedBiomeChar.ToList();
            for (int i = 1; i < charMatrix[cy, cx].Count; i++)
            {
                if (notAllowed.Count == 0) break;

                var currentSet = new HashSet<char>(BiomeDef.mapBiome[charMatrix[cy, cx][i]].NotAllowedBiomeChar);
                notAllowed.RemoveAll(bio => !currentSet.Contains(bio));
            }

            foreach (var dir in directions)
            {
                int neighborY = cy + dir.dy;
                int neighborX = cx + dir.dx;

                if ((neighborY >= 0 && neighborY < charMatrix.GetLength(0)) &&
                    neighborX >= 0 && neighborX < charMatrix.GetLength(1))
                {
                    bool optionsRemoved = ReduceEntropy(neighborY, neighborX, ref charMatrix, notAllowed);

                    if (optionsRemoved)
                    {
                        propagationQueue.Enqueue((neighborY, neighborX));
                        pos.Enqueue((neighborY, neighborX), charMatrix[neighborY, neighborX].Count);
                    }
                }
            }
        }
    }

    public static bool ReduceEntropy(int neighborY, int neighborX, ref List<char>[,] charMatrix, List<char> notAllowed)
    {
        int biomesRemoved = charMatrix[neighborY, neighborX].RemoveAll(bio => notAllowed.Contains(bio));

        return biomesRemoved > 0;
    }


    public static bool GetTileEntropy(ref List<char>[,] charMatrix, int y, int x)
    {
        bool changes = false;
        int startPosX = x - 1;
        int startPosY = y - 1;

        char[] nearTiles = new char[8];

        for (int i = startPosY; i < (startPosY + 3); i++)
        {
            for (int j = startPosX; j < (startPosX + 3); j++)
            {
                if ((i >= 0 && i < charMatrix.GetLength(0)) && (j >= 0 && j < charMatrix.GetLength(1)))
                {
                    // if (flatIndex > nearTiles.Length - 1) continue;
                    if (i == y && j == x) continue;

                    if (charMatrix[i, j].Count == 1)
                    {
                        char tileChar = charMatrix[i, j][0];
                        char[] notAllowed = BiomeDef.mapBiome[tileChar].NotAllowedBiomeChar;
                        // bool elementRemoved = charMatrix[y, x].Remove(BiomeDef.mapBiome.Keys.FirstOrDefault(key => key == tileChar));

                        int elementsRemoved = charMatrix[y, x].RemoveAll(bio => notAllowed.Contains(bio));

                        if (elementsRemoved == 0 && !changes) continue;
                        changes = true;
                    }
                }
            }
        }

        return changes;
    }

    public static bool ForceCollapseNewTile(ref List<char>[,] charMatrix, ref PriorityQueue<(int y, int x), int> pos, ref Queue<(int y, int x)> collapsedCells)
    {
        int y = 0; int x = 0;
        bool posNotEmpty = false;
        while (pos.Count > 0)
        {
            posNotEmpty = pos.TryDequeue(out var cell, out int recordedEntropy);

            int realEntropy = charMatrix[cell.y, cell.x].Count;

            if (realEntropy == 1 || realEntropy != recordedEntropy) continue;

            y = cell.y;
            x = cell.x;
            break;
        }

        if (!posNotEmpty) return true;

        Random rnd = new Random();

        int pickedBiomeIndex = rnd.Next(charMatrix[y, x].Count);
        char biomeSelected = charMatrix[y, x][pickedBiomeIndex];

        charMatrix[y, x].RemoveAll(bio => bio != biomeSelected);

        collapsedCells.Enqueue((y, x));

        return false;
    }

    public static (int y, int x) SetupEntropyMatrix(ref List<char>[,] charMatrix, ref PriorityQueue<(int y, int x), int> pos)
    {
        int charMatrixY_Length = charMatrix.GetLength(0);
        int charMatrixX_Length = charMatrix.GetLength(1);
        Random rnd = new();

        for (int i = 0; i < charMatrixY_Length; i++)
        {
            for (int j = 0; j < charMatrixX_Length; j++)
            {
                charMatrix[i, j] = new();
                foreach (var item in BiomeDef.mapBiome)
                {
                    charMatrix[i, j].Add(item.Key);
                }
                pos.Enqueue((i, j), charMatrix[i, j].Count);
            }

        }
        int posY = rnd.Next(charMatrixY_Length);
        int posX = rnd.Next(charMatrixX_Length);

        char pickedBiome = BiomeDef.GetRandomTile();
        charMatrix[posY, posX].RemoveAll(bio => bio != pickedBiome);

        return (posY, posX);
    }

    public static void PrintTiles(ref List<char>[,] charMatrix, bool onlyTiled = false)
    {

        for (int i = 0; i < charMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < charMatrix.GetLength(1); j++)
            {

                if (onlyTiled)
                {
                    if (charMatrix[i, j].Count == 1)
                    {
                        Console.ForegroundColor = BiomeDef.mapBiome[charMatrix[i, j][0]].Color;
                        Console.Write($"|{charMatrix[i, j][0]}|");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("|-|");
                    }

                    continue;
                }

                for (int k = 0; k < 4; k++)
                {
                    if (charMatrix[i, j].Count >= (k + 1))
                    {
                        Console.ForegroundColor = BiomeDef.mapBiome[charMatrix[i, j][k]].Color;
                        Console.Write($"|{charMatrix[i, j][k]}|");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("|-|");
                    }
                }

                Console.Write("--");


            }

            Console.WriteLine();

        }
    }
}
