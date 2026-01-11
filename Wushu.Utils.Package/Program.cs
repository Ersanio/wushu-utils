using System.Text;
using Wushu.Utils.Package;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            Environment.Exit(1);
        }


        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            switch (args[0].ToLower())
            {
                case "unpack":
                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine($"Source file '{args[0]}' does not exist.");
                        Environment.Exit(1);
                    }
                    var unpacker = new Unpacker(args[1], args[2]);
                    unpacker.Unpack();
                    break;
                case "repack":
                    if (!Directory.Exists(args[1]))
                    {
                        Console.WriteLine($"Source directory '{args[1]}' does not exist.");
                        Environment.Exit(1);
                    }
                    var repacker = new Repacker(args[1], args[2]);
                    repacker.Repack();
                    break;
                case "help":
                    ShowHelp();
                    break;
                default:
                    ShowHelp();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during the operation.");
            Console.WriteLine("Details:");
            Console.WriteLine("Parameters: " + string.Join(", ", args));
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            Environment.Exit(1);
        }

        Environment.Exit(0);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Age of Wushu Package Utility");
        Console.WriteLine("Usage:");
        Console.WriteLine($"  unpack <source file> <destination directory>  - Unpack a package file into a directory");
        Console.WriteLine($"  repack <source directory> <destination file>  - Repack a directory into a package file");
        Console.WriteLine($"  help                                          - Show this help message");
    }
}