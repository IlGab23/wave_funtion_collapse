using System.Net;
using System.Numerics;
using WaveFunctionCollapse.Domain;

namespace WaveFunctionCollapse.Fnc;

public static class UtilitiesFuncions
{
    // Ritorna quanti bit sono "accesi" (quante opzioni rimangono)
    public static int GetOptionsCount(byte mask)
    {
        return BitOperations.PopCount(mask);
    }

    // Dato un byte con 1 solo bit acceso, ritorna il carattere corrispondente
    public static char GetCharFromMask(byte mask)
    {
        foreach (var kvp in BiomeDef.mapBiome)
        {
            if (kvp.Value.BitMask == mask) return kvp.Key;
        }
        return '\0';
    }

    public static bool EditNearTiles(int y, int x, ref byte[,] charMatrix, ref PriorityQueue<(int y, int x), double> pos, ref Dictionary<char, int> biomesTileCount, int matrixTotalLength)
    {
        (int dy, int dx)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        Queue<(int y, int x)> propagationQueue = new();
        propagationQueue.Enqueue((y, x));

        while (propagationQueue.Count > 0)
        {
            var currentCell = propagationQueue.Dequeue();
            int cy = currentCell.y;
            int cx = currentCell.x;

            byte currentMask = charMatrix[cy, cx];
            byte allowedByCurrent = 0;
            
            // Calcolo ultraveloce di cosa è permesso: l'unione di tutti gli AllowedMask
            // dei biomi ancora possibili in questa cella
            foreach (var kvp in BiomeDef.mapBiome)
            {
                if ((currentMask & kvp.Value.BitMask) != 0)
                {
                    allowedByCurrent |= kvp.Value.AllowedMask;
                }
            }

            foreach (var dir in directions)
            {
                int neighborY = cy + dir.dy;
                int neighborX = cx + dir.dx;

                if ((neighborY >= 0 && neighborY < charMatrix.GetLength(0)) &&
                    neighborX >= 0 && neighborX < charMatrix.GetLength(1))
                {
                    bool optionsRemoved = ReduceEntropy(neighborY, neighborX, ref charMatrix, allowedByCurrent);

                    if (optionsRemoved)
                    {
                        byte neighborMask = charMatrix[neighborY, neighborX];
                        int count = GetOptionsCount(neighborMask);

                        if (count == 0) return false;

                        if (count == 1)
                        {
                            char c = GetCharFromMask(neighborMask);
                            biomesTileCount[c]++;
                        }

                        propagationQueue.Enqueue((neighborY, neighborX));
                        double totalWeight = CalculateTotalWeight(neighborMask);
                        double newEntropy = CalculateShannonEntropy(neighborMask, totalWeight, neighborY, neighborX);
                        pos.Enqueue((neighborY, neighborX), newEntropy);
                    }
                }
            }
        }

        return true;
    }

    public static bool ReduceEntropy(int neighborY, int neighborX, ref byte[,] charMatrix, byte allowedByCurrent)
    {
        byte oldMask = charMatrix[neighborY, neighborX];
        
        // Intersezione immediata bit a bit: spegne tutti i bit non permessi!
        charMatrix[neighborY, neighborX] &= allowedByCurrent;
        
        return oldMask != charMatrix[neighborY, neighborX];
    }

    public static Dictionary<char, int> GetNearCollapsed(ref byte[,] charMatrix, int y, int x)
    {
        (int dy, int dx)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };
        Dictionary<char, int> nearBiomes = new();

        foreach (var dir in directions)
        {
            int ny = y + dir.dy;
            int nx = x + dir.dx;

            if (ny >= 0 && ny < charMatrix.GetLength(0) && nx >= 0 && nx < charMatrix.GetLength(1))
            {
                byte neighborMask = charMatrix[ny, nx];
                if (GetOptionsCount(neighborMask) == 1)
                {
                    char collapsedBiome = GetCharFromMask(neighborMask);
                    if (!nearBiomes.ContainsKey(collapsedBiome)) nearBiomes[collapsedBiome] = 0;

                    nearBiomes[collapsedBiome]++;
                }
            }
        }

        return nearBiomes;
    }

    public static bool ForceCollapseNewTile(ref byte[,] charMatrix, ref PriorityQueue<(int y, int x), double> pos, ref Queue<(int y, int x)> collapsedCells, ref Dictionary<char, int> biomesTileCount)
    {
        int y = 0; int x = 0;
        bool foundCellToCollapse = false;
        double totalWeight = 0;
        while (pos.Count > 0)
        {
            bool dequeueSuccess = pos.TryDequeue(out var cell, out double recordedEntropy);

            byte cellMask = charMatrix[cell.y, cell.x];
            if (GetOptionsCount(cellMask) <= 1) continue;

            totalWeight = CalculateTotalWeight(cellMask);
            double realEntropy = CalculateShannonEntropy(cellMask, totalWeight, cell.y, cell.x);

            if (!realEntropy.IsNearlyEqual(recordedEntropy)) continue;

            y = cell.y;
            x = cell.x;
            foundCellToCollapse = true;
            break;
        }

        if (!foundCellToCollapse) return true;

        Random rnd = new Random();
        Dictionary<char, int> nearBiomes = GetNearCollapsed(ref charMatrix, y, x);
        Dictionary<char, double> localWeights = new();
        int totalMatrixCells = charMatrix.GetLength(0) * charMatrix.GetLength(1);
        int limit = totalMatrixCells / 2;

        const int adjacencyMultiplier = 5;
        totalWeight = 0;
        
        byte mask = charMatrix[y, x];
        foreach (var kvp in BiomeDef.mapBiome)
        {
            if ((mask & kvp.Value.BitMask) != 0)
            {
                char b = kvp.Key;
                double currentWeight = kvp.Value.Weight;
                if (nearBiomes.TryGetValue(b, out int count))
                {
                    currentWeight *= (1 + (count * adjacencyMultiplier));
                }

                int currentCount = biomesTileCount.ContainsKey(b) ? biomesTileCount[b] : 0;
                if (currentCount >= limit)
                {
                    currentWeight = 0.01;
                }
                else
                {
                    double factor = 1.0 - ((double)currentCount / limit);
                    currentWeight *= factor;
                }

                localWeights[b] = currentWeight;
                totalWeight += currentWeight;
            }
        }

        double randomWeight = rnd.NextDouble() * totalWeight;

        char biomeSelected = '\0';
        foreach (var kvp in BiomeDef.mapBiome)
        {
            if ((mask & kvp.Value.BitMask) != 0)
            {
                char bio = kvp.Key;
                randomWeight -= localWeights[bio];
                if (randomWeight < 0)
                {
                    biomeSelected = bio;
                    break;
                }
            }
        }
        
        if (biomeSelected == '\0') // Fallback sicurezza
        {
            foreach(var kvp in BiomeDef.mapBiome) { if ((mask & kvp.Value.BitMask) != 0) { biomeSelected = kvp.Key; break; } }
        }

        charMatrix[y, x] = BiomeDef.mapBiome[biomeSelected].BitMask;
        biomesTileCount[biomeSelected]++;

        collapsedCells.Enqueue((y, x));

        return false;
    }

    public static (int y, int x) SetupEntropyMatrix(ref byte[,] charMatrix, ref PriorityQueue<(int y, int x), double> pos, ref Dictionary<char, int> biomesTileCount)
    {
        int charMatrixY_Length = charMatrix.GetLength(0);
        int charMatrixX_Length = charMatrix.GetLength(1);
        Random rnd = new();

        byte allBiomesMask = 0;
        foreach (var item in BiomeDef.mapBiome)
        {
            allBiomesMask |= item.Value.BitMask;
            if (biomesTileCount.Count != BiomeDef.mapBiome.Count) biomesTileCount.Add(item.Key, 0);
        }

        for (int i = 0; i < charMatrixY_Length; i++)
        {
            for (int j = 0; j < charMatrixX_Length; j++)
            {
                charMatrix[i, j] = allBiomesMask;
                double startEntropy = CalculateShannonEntropy(charMatrix[i, j], CalculateTotalWeight(charMatrix[i, j]), i, j);
                pos.Enqueue((i, j), startEntropy);
            }
        }
        
        int posY = rnd.Next(charMatrixY_Length);
        int posX = rnd.Next(charMatrixX_Length);

        char pickedBiome = BiomeDef.GetRandomTile();
        charMatrix[posY, posX] = BiomeDef.mapBiome[pickedBiome].BitMask;
        biomesTileCount[pickedBiome]++;

        return (posY, posX);
    }

    public static double CalculateShannonEntropy(byte mask, double totalWeight, int y, int x)
    {
        int count = GetOptionsCount(mask);
        if (count <= 1) return 0.0;

        double entropy = 0;
        foreach (var kvp in BiomeDef.mapBiome)
        {
            if ((mask & kvp.Value.BitMask) != 0)
            {
                double p = kvp.Value.Weight / totalWeight;
                entropy -= p * Math.Log2(p);
            }
        }

        Random cellRnd = new Random((y * 100000) + x);
        double noise = cellRnd.NextDouble() * 0.000001;

        return entropy + noise;
    }

    public static double CalculateTotalWeight(byte mask)
    {
        double totalWeight = 0;
        foreach (var kvp in BiomeDef.mapBiome)
        {
            if ((mask & kvp.Value.BitMask) != 0)
            {
                totalWeight += kvp.Value.Weight;
            }
        }

        return totalWeight;
    }

    public static void CleanUpMap(ref byte[,] charMatrix)
    {
        int matrixLength_Y = charMatrix.GetLength(0);
        int matrixLength_X = charMatrix.GetLength(1);
        (int dy, int dx)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        for (int i = 0; i < matrixLength_Y; i++)
        {
            for (int j = 0; j < matrixLength_X; j++)
            {
                Dictionary<char, int> nearBiomes = GetNearCollapsed(ref charMatrix, i, j);

                if (nearBiomes.Count > 1)
                {
                    char currentC = GetCharFromMask(charMatrix[i, j]);
                    int tileCount = nearBiomes.ContainsKey(currentC) ? nearBiomes[currentC] : 0;

                    if (tileCount >= 1) continue;
                }

                // Unione dei divieti logici
                byte forbiddenByNeighborsMask = 0;
                foreach (var dir in directions)
                {
                    int ny = i + dir.dy;
                    int nx = j + dir.dx;
                    if (ny >= 0 && ny < matrixLength_Y && nx >= 0 && nx < matrixLength_X)
                    {
                        if (GetOptionsCount(charMatrix[ny, nx]) == 1)
                        {
                            char neighborBiome = GetCharFromMask(charMatrix[ny, nx]);
                            // Se un vicino VIETA qualcosa, il bit corrispondente non e' nel suo AllowedMask.
                            // Quindi i divieti del vicino sono ~AllowedMask.
                            forbiddenByNeighborsMask |= (byte)~BiomeDef.mapBiome[neighborBiome].AllowedMask;
                        }
                    }
                }

                char newBiome = '\0';
                foreach (var kvp in nearBiomes.OrderByDescending(kv => kv.Value))
                {
                    // Controlla se il bioma (kvp.Key) è vietato (cioe' se il suo BitMask e' nei divieti)
                    if ((BiomeDef.mapBiome[kvp.Key].BitMask & forbiddenByNeighborsMask) == 0)
                    {
                        newBiome = kvp.Key;
                        break;
                    }
                }

                if (newBiome != '\0')
                {
                    charMatrix[i, j] = BiomeDef.mapBiome[newBiome].BitMask;
                }
            }
        }
    }

    public static void PrintTiles(ref byte[,] charMatrix, bool onlyTiled = false)
    {
        for (int i = 0; i < charMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < charMatrix.GetLength(1); j++)
            {
                byte mask = charMatrix[i, j];
                int count = GetOptionsCount(mask);

                if (onlyTiled)
                {
                    if (count == 1)
                    {
                        char c = GetCharFromMask(mask);
                        Console.ForegroundColor = BiomeDef.mapBiome[c].Color;
                        Console.Write($"|{c}|");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("|-|");
                    }

                    continue;
                }

                // If not onlyTiled, print up to 4 options
                int printed = 0;
                foreach(var kvp in BiomeDef.mapBiome)
                {
                    if ((mask & kvp.Value.BitMask) != 0)
                    {
                        Console.ForegroundColor = kvp.Value.Color;
                        Console.Write($"|{kvp.Key}|");
                        Console.ResetColor();
                        printed++;
                        if (printed == 4) break;
                    }
                }
                
                for(int k = printed; k < 4; k++)
                {
                    Console.Write("|-|");
                }

                Console.Write("--");
            }
            Console.WriteLine();
        }
    }
}
