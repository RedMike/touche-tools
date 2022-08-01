using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ToucheTools.Web.Services;

public interface IImagePackageProcessingService
{
    Dictionary<int, Rgb24> CalculatePalette(Dictionary<string, Image<Rgb24>> gameImages, Image<Rgb24> background);
    Dictionary<string, Image<Rgb24>> ProcessImages(Dictionary<int, Rgb24> palette, Dictionary<string, Image<Rgb24>> images);
}

public class ImagePackageProcessingService : IImagePackageProcessingService
{
    private const int MaximumRedTolerance = 10;
    private const int MaximumGreenTolerance = 6;
    private const int MaximumBlueTolerance = 15;

    private const int BackgroundColourCount = 192; //not first (transparent), 64th (delimiter), 255th (delimiter), game
    private const int GameColourCount = 61; //193-254 (1-62)
    //TODO: some colours must be getting used for e.g. text colours, fonts for money?
    
    private readonly ILogger _logger;

    public ImagePackageProcessingService(ILogger<ImagePackageProcessingService> logger)
    {
        _logger = logger;
    }
    
    public Dictionary<int, Rgb24> CalculatePalette(Dictionary<string, Image<Rgb24>> gameImages, Image<Rgb24> background)
    {
        var palette = new Dictionary<int, Rgb24>();
        palette[0] = new Rgb24(0, 0, 0);
        palette[64] = new Rgb24(50, 50, 50);
        palette[255] = new Rgb24(100, 100, 100);

        //get the colours in the background
        var bgCols = GetColoursInImage(background);
        //get the colours in all the sprites
        var gameCols = gameImages.Values.Select(GetColoursInImage).SelectMany(l => l)
            .DistinctBy(c => $"{c.R}-{c.G}-{c.B}").ToList();
        
        _logger.Log(LogLevel.Information, "Started with {} bg and {} fg colours", bgCols.Count, gameCols.Count);

        while (gameCols.Count > GameColourCount || bgCols.Count > BackgroundColourCount)
        {
            _logger.Log(LogLevel.Information, "Currently at {} bg and {} fg colours", bgCols.Count, gameCols.Count);
            //until we have fewer than the right number of colours
            //merge the two closest colours
            List<(Rgb24, int)> closeColoursAndWeight;
            //if background is varied, start with background ones first
            if (bgCols.Count > BackgroundColourCount)
            {
                _logger.Log(LogLevel.Information, "Too many background colours: {}", bgCols.Count);
                var closeColours = new Dictionary<Rgb24, int>();
                foreach (var col1 in bgCols)
                {
                    foreach (var col2 in bgCols.Where(c => c != col1))
                    {
                        var diff = ColourDifference(col1, col2);
                        if (diff == null || diff == 0)
                        {
                            continue;
                        }

                        if (!closeColours.ContainsKey(col1) || closeColours[col1] > diff)
                        {
                            closeColours[col1] = diff.Value;
                        }

                        if (closeColours[col1] < 5)
                        {
                            break; //don't need to get too specific when it's such a close value, saves time
                        }
                    }

                    if (closeColours.Count(p => p.Value < 5) > 4)
                    {
                        break; //don't need to get too specific when there's so many values, saves time
                    }
                }

                closeColoursAndWeight = closeColours.Select(c => (c.Key, c.Value)).OrderBy(c => c.Value).ToList();
            }
            else
            {
                //otherwise try both background and game colours
                throw new NotImplementedException();
            }

            do
            {
                if (closeColoursAndWeight.Count == 0)
                {
                    //TODO: if no colours are close enough to merge, dithering, for now exception
                    throw new NotImplementedException();
                }
                var topMatch = closeColoursAndWeight.First();

                //check that all the images can still be converted within tolerances
                _logger.Log(LogLevel.Information, "Deciding if to remove {}", topMatch.Item1);
                var wasBg = bgCols.Remove(topMatch.Item1);
                var wasGame = gameCols.Remove(topMatch.Item1);

                var tempPalette = bgCols.Concat(gameCols).Distinct().ToList();
                var missingCol = false;
                if (!ValidateColourWithPaletteInTolerances(background, tempPalette))
                {
                    missingCol = true;
                }
                else
                {
                    foreach (var gameImg in gameImages)
                    {
                        if (!ValidateColourWithPaletteInTolerances(gameImg.Value, tempPalette))
                        {
                            missingCol = true;
                            break;
                        }
                    }
                }

                if (missingCol)
                {
                    _logger.Log(LogLevel.Information, "Cannot remove {} re-adding", topMatch.Item1);
                    if (wasBg)
                    {
                        bgCols.Add(topMatch.Item1);
                    }

                    if (wasGame)
                    {
                        gameCols.Add(topMatch.Item1);
                    }
                    continue;
                }

                //we got this far, so we found a value
                break;
            } while (true);
        }
        
        //TODO: sort colours to make the palette a bit more readable
        //bgCols.Sort((a, b) => String.Compare($"{a.R}-{a.G}-{a.B}", $"{b.R}-{b.G}-{b.B}", StringComparison.Ordinal));
        //gameCols.Sort((a, b) => String.Compare($"{a.R}-{a.G}-{a.B}", $"{b.R}-{b.G}-{b.B}", StringComparison.Ordinal));

        for (var i = 0; i < GameColourCount; i++)
        {
            var col = new Rgb24(255, 0, 255);
            if (i < gameCols.Count)
            {
                col = gameCols[i];
            }

            palette[193 + i] = col;
        }

        var curColIdx = 0;
        for (var i = 0; i < 192; i++)
        {
            if (i == 0 || i == 64)
            {
                continue;
            }
            var col = new Rgb24(255, 100, 255);
            if (curColIdx < bgCols.Count)
            {
                col = bgCols[curColIdx];
            }

            curColIdx++;
            palette[i] = col;
        }
        
        return palette;
    }

    public Dictionary<string, Image<Rgb24>> ProcessImages(Dictionary<int, Rgb24> palette, Dictionary<string, Image<Rgb24>> images)
    {
        var processedImages = new Dictionary<string, Image<Rgb24>>();
        foreach (var pair in images)
        {
            processedImages[pair.Key] = ProcessImageWithPalette(pair.Value, palette);
        }

        return processedImages;
    }

    private int? ColourDifference(Rgb24 a, Rgb24 b)
    {
        if (a.R == b.R && a.G == b.G && a.B == b.B)
        {
            return 0;
        }
        if (Math.Abs(a.R - b.R) > MaximumRedTolerance ||
            Math.Abs(a.G - b.G) > MaximumGreenTolerance ||
            Math.Abs(a.B - b.B) > MaximumBlueTolerance
            )
        {
            //too different, don't even try
            return null;
        }

        var dr = Math.Abs(a.R - b.R);
        var dg = Math.Abs(a.G - b.G);
        var db = Math.Abs(a.B - b.B);
        return Math.Max(dr, Math.Max(dg, db));
    }

    private Image<Rgb24> ProcessImageWithPalette(Image<Rgb24> image, Dictionary<int, Rgb24> palette)
    {
        var clonedImage = image.Clone();
        var w = image.Width;
        var h = image.Height;
        Rgb24 transparentCol = new Rgb24(0, 0, 0);
        clonedImage.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < h; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                if (i == 0)
                {
                    transparentCol = row[0];
                }
                for (var j = 0; j < w; j++)
                {
                    var col = row[j];
                    if (col == transparentCol)
                    {
                        row[j] = palette[0];
                        continue;
                    }
                    var closestCol = palette.Select(c => (c.Key, ColourDifference(c.Value, col))).Where(c => c.Item2 != null).MinBy(c => c.Item2);
                    row[j] = new Rgb24((byte)closestCol.Key, (byte)closestCol.Key, (byte)closestCol.Key);
                }
            }
        });
        return clonedImage;
    }

    private List<Rgb24> GetColoursInImage(Image<Rgb24> image)
    {
        var w = image.Width;
        var h = image.Height;
        var cols = new HashSet<Rgb24>();
        Rgb24 transparentCol = new Rgb24(0, 0, 0);
        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < h; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                if (i == 0)
                {
                    transparentCol = row[0];
                }
                for (var j = 0; j < w; j++)
                {
                    var col = row[j];
                    if (col == transparentCol)
                    {
                        continue;
                    }
                    cols.Add(col);
                }
            }
        });
        //distinct check is probably not strictly necessary
        return cols.DistinctBy(c => $"{c.R}-{c.G}-{c.B}").ToList();
    }

    private bool ValidateColourWithPaletteInTolerances(Image<Rgb24> image, List<Rgb24> palette)
    {
        var w = image.Width;
        var h = image.Height;
        HashSet<Rgb24> missingCols = new HashSet<Rgb24>();
        Dictionary<Rgb24, bool> checkedCols = new Dictionary<Rgb24, bool>();
        Rgb24 transparentCol = new Rgb24(0, 0, 0);
        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < h; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                if (i == 0)
                {
                    transparentCol = row[0];
                }

                for (var j = 0; j < w; j++)
                {
                    var col = row[j];
                    if (col == transparentCol)
                    {
                        continue;
                    }
                    if (checkedCols.ContainsKey(col))
                    {
                        continue;
                    }

                    checkedCols[col] = true;
                    if (palette.Select(c => ColourDifference(c, col)).All(c => c == null))
                    {
                        _logger.Log(LogLevel.Information, "Missing colour from image: {}", col);
                        missingCols.Add(col);
                    }
                }
            }
        });
        return missingCols.Count == 0;
    }
}