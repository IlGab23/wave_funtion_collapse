using WaveFunctionCollapse.Domain;

namespace WaveFunctionCollapse.Fnc;

public static class UtilitiesFuncions
{
    public static List<(int y, int x)> EditNearTiles(int y, int x, int startRow, ref List<char>[,] charMatrix)
    {
        List<(int y, int x)> pos = new();

        if (startRow > (y + 1)) return pos;
        int startPosX = x - 1;

        for (int i = startPosX; i < (startPosX + 3); i++)
        {
            if ((startRow >= 0 && startRow < charMatrix.GetLength(0)) && (i >= 0 && i < charMatrix.GetLength(1)))
            {
                if (startRow == y && i == x) continue;
                bool changes = GetTileEntropy(ref charMatrix, startRow, i);
                // charMatrix[startRow, i] = BiomeDef.GetRandomTile();

                if (charMatrix[startRow, i].Count < 4 && changes)
                {
                    pos.Add((startRow, i));
                }
            }
        }

        pos.AddRange(EditNearTiles(y, x, startRow + 1, ref charMatrix));

        return pos;
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
            for (int j = 0; j < charMatrix.GetLength(0); j++)
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
