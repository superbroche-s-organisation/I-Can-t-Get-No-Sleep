using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ICGNS.Tests;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--graphics"))
            throw new ArgumentException("Specify the game installation directory and optionally --graphics.");
        string gamePath = Path.GetFullPath(args[0]);
        AssemblyLoadContext.Default.Resolving += (_, name) => Resolve(gamePath, name);
        try
        {
            Run(gamePath, args.Length == 2);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static Assembly? Resolve(string gamePath, AssemblyName name)
    {
        foreach (string directory in new[] { gamePath, Path.Combine(gamePath, "smapi-internal") })
        {
            string path = Path.Combine(directory, name.Name + ".dll");
            if (File.Exists(path))
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run(string gamePath, bool graphics) => RuntimeChecks.Run(gamePath, graphics);
}
