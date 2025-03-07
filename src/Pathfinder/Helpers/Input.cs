using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;

namespace Pathfinder.Helpers;

public class Input
{
    /// <summary>
    /// Lukee kartan tiedostosta, jonka muoto on se mitä käytetään Moving AI Lab sivun pikselikartoissa: https://www.movingai.com/benchmarks/grids.html
    /// </summary>
    /// <param name="file">Polku tiedostoon. Suhteellinen ja absoluuttinen polku käy.</param>
    /// <returns>Pikselikartta 2d int taulukkona</returns>
    public static int[,] ReadMapFromFile(string file)
    {
        var lines = File.ReadAllLines(FixPathFormatting(file)).Where(line => line.Contains('.') || line.Contains('@')).ToArray();

        var width = lines[0].Length;
        var height = lines.Length;

        var map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var mapPiece = lines[y][x];

                map[x, y] = mapPiece == '.' ? 0 : 1;
            }
        }

        return map;
    }

    /// <summary>
    /// Lukee kartan mustavalkoisesta kuvasta, jossa vaaleat sävyt (pikselin arvo alle 128) on kuljettavia pisteistä ja tummat sävyt (pikselin arvo vähintään 128) on seiniä.
    /// </summary>
    /// <param name="file">Polku tiedostoon. Suhteellinen ja absoluuttinen polku käy.</param>
    /// <returns>Pikselikartta 2d int taulukkona</returns>
    public static int[,] ReadMapFromImage(string file)
    {
        using var image = Image.Load<L8>(FixPathFormatting(file));

        int width = image.Width;
        int height = image.Height;

        var map = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte intensity = image[x, y].PackedValue;

                map[x, y] = intensity < 128 ? 1 : 0;
            }
        }

        return map;
    }

    /// <summary>
    /// Yrittää lukea kartan joko tiedostosta tai kuvasta. Palauttaa true jos onnistui ja asettaa map arvoksi luettu kartta.
    /// </summary>
    /// <param name="path">Polku tiedostoon. Suhteellinen ja absoluuttinen polku käy.</param>
    /// <param name="map">Kartta</param>
    /// <returns></returns>
    public static bool TryReadMap(string path, out int[,] map)
    {
        try
        {
            map = ReadMapFromImage(path);
            return true;
        }
        catch { }

        try
        {
            map = ReadMapFromFile(path);
            return true;
        }
        catch { }

        map = new int[0,0];
        return false;
    }

    /// <summary>
    /// Korjaa polun muotoilun, jotta se toimii vaikka kopioidussa polussa olisi lainausmerkit.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string FixPathFormatting(string path)
    {
        return path.Replace("\"", "").Trim();
    }
}
