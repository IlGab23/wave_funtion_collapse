using WaveFunctionCollapse.Domain;

namespace WaveFunctionCollapse.Fnc;

public static class UtilitiesFuncions
{
    public static void EditNearTiles(int y, int x, ref List<char>[,] charMatrix)
    {
        (int dy, int dx)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        Queue<(int y, int x)> propagationQueue = new();
        propagationQueue.Enqueue((y, x));

        while (propagationQueue.Count > 0)
        {
            var currentCell = propagationQueue.Dequeue();
            int cy = currentCell.y;
            int cx = currentCell.x;

            foreach (var dir in directions)
            {
                int neighborY = cy + dir.dy;
                int neighborX = cx + dir.dx;

                if ((neighborY >= 0 && neighborY < charMatrix.GetLength(0)) &&
                    neighborX >= 0 && neighborX < charMatrix.GetLength(1))
                {
                    bool optionsRemoved = ReduceEntropy(cy, cx, neighborY, neighborX, ref charMatrix);

                    if (optionsRemoved)
                    {
                        propagationQueue.Enqueue((neighborY, neighborX));
                    }
                }
            }
        }
    }

    public static bool ReduceEntropy(int currentY, int currentX, int neighborY, int neighborX, ref List<char>[,] charMatrix)
    {
        if (charMatrix[currentY, currentX].Count != 1) return false;

        char currentCellChar = charMatrix[currentY, currentX][0];
        char[] notAllowed = BiomeDef.mapBiome[currentCellChar].NotAllowedBiomeChar;

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

    public static bool ForceCollapseNewTile(ref List<char>[,] charMatrix, ref List<(int y, int x)> pos)
    {
        // Heuristic Search /////////////////////////////////////////////
        int minEntropy = int.MaxValue;
        List<(int y, int x)> bestCells = new();

        for (int i = 0; i < charMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < charMatrix.GetLength(1); j++)
            {
                int entropy = charMatrix[i, j].Count;

                if (entropy <= 1) continue;

                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    bestCells.Clear();
                    bestCells.Add((i, j));
                }
                else if (entropy == minEntropy)
                {
                    bestCells.Add((i, j));
                }
            }
        }
        // Heuristic Search /////////////////////////////////////////////

        if (bestCells.Count == 0) return true;

        Random rnd = new Random();

        var pickedCell = bestCells[rnd.Next(bestCells.Count)];
        int posY = pickedCell.y;
        int posX = pickedCell.x;

        int pickedBiomeIndex = rnd.Next(minEntropy);
        char biomeSelected = charMatrix[posY, posX][pickedBiomeIndex];

        charMatrix[posY, posX].RemoveAll(bio => bio != biomeSelected);

        pos.Clear();
        pos.Add((posY, posX));

        return false;
    }

    public static void SetupEntropyMatrix(ref List<char>[,] charMatrix, ref List<(int y, int x)> pos)
    {
        int charMatrixY_Length = charMatrix.GetLength(0);
        int charMatrixX_Length = charMatrix.GetLength(1);

        for (int i = 0; i < charMatrixY_Length; i++)
        {
            for (int j = 0; j < charMatrixX_Length; j++)
            {
                charMatrix[i, j] = new();
                foreach (var item in BiomeDef.mapBiome)
                {
                    charMatrix[i, j].Add(item.Key);
                }
            }

        }

        int centerY = charMatrixY_Length / 2;
        int centerX = charMatrixX_Length / 2;

        pos.Add((centerY, centerX));

        char pickedBiome = BiomeDef.GetRandomTile();
        charMatrix[centerY, centerX].RemoveAll(b => b != pickedBiome);
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
