namespace WaveFunctionCollapse.Domain;


public record Biome(string Nome, ConsoleColor Color, char[] NotAllowedBiomeChar);

public static class BiomeDef
{
    public static readonly Dictionary<char, Biome> mapBiome = new Dictionary<char, Biome>()
    {
        {'G', new Biome("Grass", ConsoleColor.Green, ['W', 'I'])},
        {'F', new Biome("Forest", ConsoleColor.DarkGreen, ['D', 'W', 'I'])},
        {'D', new Biome("Desert", ConsoleColor.Yellow, ['F', 'C'])},
        {'W', new Biome("Water", ConsoleColor.Blue, ['G', 'F', 'S'])},
        {'I', new Biome("Inferno", ConsoleColor.Red, ['G', 'F', 'C'])},
        {'C', new Biome("Ice", ConsoleColor.Cyan, ['I', 'D'])},
        {'S', new Biome("Desolate", ConsoleColor.DarkYellow, ['W'])}
    };

    public static char GetRandomTile()
    {
        char[] options = BiomeDef.mapBiome.Keys.ToArray();

        Random rnd = new Random();

        int pickedBiome = rnd.Next(options.Length);

        return options[pickedBiome];
    }

}
