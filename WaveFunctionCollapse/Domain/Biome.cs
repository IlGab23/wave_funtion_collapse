namespace WaveFunctionCollapse.Domain;

public record Biome(char CharID, string Nome, ConsoleColor Color, char[] NotAllowedBiomeChar, int Weight, byte BitMask)
{
    public byte AllowedMask { get; set; }
}

public static class BiomeDef
{
    public static readonly Dictionary<char, Biome> mapBiome = new Dictionary<char, Biome>()
    {
        {'G', new Biome('G', "Grass", ConsoleColor.Green, ['W', 'I', 'C'], 20, 1)},
        {'F', new Biome('F', "Forest", ConsoleColor.DarkGreen, ['D', 'W', 'I', 'S'], 10, 2)},
        {'D', new Biome('D', "Desert", ConsoleColor.Yellow, ['F', 'C'], 10, 4)},
        {'W', new Biome('W', "Water", ConsoleColor.Blue, ['G', 'F', 'S'], 20, 8)},
        {'I', new Biome('I', "Inferno", ConsoleColor.Red, ['G', 'F', 'C'], 5, 16)},
        {'C', new Biome('C', "Ice", ConsoleColor.Cyan, ['I', 'D', 'G', 'S'], 10, 32)},
        {'S', new Biome('S', "Desolate", ConsoleColor.DarkYellow, ['W', 'F', 'C'], 15, 64)}
    };

    static BiomeDef()
    {
        foreach (var biome in mapBiome.Values)
        {
            byte allowed = 0;
            foreach (var other in mapBiome.Values)
            {
                if (!biome.NotAllowedBiomeChar.Contains(other.CharID))
                {
                    allowed |= other.BitMask;
                }
            }
            biome.AllowedMask = allowed;
        }
    }

    public static char GetRandomTile()
    {
        char[] options = BiomeDef.mapBiome.Keys.ToArray();

        Random rnd = new Random();

        int pickedBiome = rnd.Next(options.Length);

        return options[pickedBiome];
    }
}

