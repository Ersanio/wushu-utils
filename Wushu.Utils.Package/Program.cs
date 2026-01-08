using System.Text;
using Wushu.Utils.Package;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            Environment.Exit(-1);
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
                        return;
                    }
                    var unpacker = new Unpacker(args[1], args[2]);
                    unpacker.Unpack();
                    break;
                case "repack":
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
        catch
        {
            Console.WriteLine("An error occurred during the operation.");
            Environment.Exit(-1);
        }

        Environment.Exit(0);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Age of Wushu Package Utility");
        Console.WriteLine("Usage:");
        Console.WriteLine($"  unpack <source file> <destination folder>  - Unpack a package file into a folder");
        Console.WriteLine($"  repack <source folder> <destination file>  - Repack a folder into a package file");
        Console.WriteLine($"  help                                       - Show this help message");
    }
}