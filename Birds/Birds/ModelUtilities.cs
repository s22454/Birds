namespace Birds;

public class ModelUtilities
{
    public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
    {
        // get all files from folder 
        var files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);

        foreach (var file in files)
        {
            // if file is not an image skip it 
            if (Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".png")
                continue;

            // select file label
            var label = Path.GetFileName(file);

            if (useFolderNameAsLabel)
                label = Directory.GetParent(file).Name;
            else
            {
                for (int i = 0; i < label.Length; i++)
                {
                    if (!char.IsLetter(label[i]))
                    {
                        label = label.Substring(0, i);
                        break;
                    }
                }
            }

            // return new ImageData instantiation 
            yield return new ImageData()
            {
                ImagePath = file,
                Label = label
            };
        }
    }
}