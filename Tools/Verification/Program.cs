using ACaldeira.Simulation;
using ACaldeira.Pooling;
using ACaldeira.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

static class Program
{
    private sealed class Actor : PooledBehaviour { }
    static int Main(string[] args)
    {
        string root = Path.GetFullPath(args.Length > 0 ? args[0] : "../..");
        int failures = 0, checks = 0;
        void Check(bool value, string name) { checks++; if (!value) { failures++; Console.WriteLine("FAIL " + name); } }
        string[] files = Directory.GetFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            foreach (var diagnostic in tree.GetDiagnostics())
                if (diagnostic.Severity == DiagnosticSeverity.Error) { failures++; Console.WriteLine(file + ": " + diagnostic); }
            if (!file.Contains(Path.DirectorySeparatorChar + "Scripts" + Path.DirectorySeparatorChar)) continue;
            foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                string method = call.Expression is MemberAccessExpressionSyntax m ? m.Name.Identifier.Text : call.Expression.ToString();
                Check(method != "Instantiate" && method != "Destroy" && method != "DestroyImmediate", "No runtime creation/deletion: " + file);
            }
        }
        var grid = new SpatialGrid(1200, 48, 36, -48, -36, 2);
        Check(grid.Column(-0.01f) == 23 && grid.Column(0) == 24, "floor negative coordinates");
        Check(grid.Column(-1000) == 0 && grid.Row(1000) == 35, "clamped edges");
        for (int i = 0; i < 1200; i++) grid.Insert(i, 0, 0);
        int count = 0; for (int i = grid.Head(24, 18); i >= 0; i = grid.Next(i)) count++;
        Check(count == 1200, "dense bucket preserves all entries");
        grid.Clear(); Check(grid.Head(24, 18) == -1, "clear resets buckets");
        var random = new Random(1234); float[] xs = new float[1200], ys = new float[1200];
        for (int i = 0; i < 1200; i++) { xs[i] = random.NextSingle() * 90 - 45; ys[i] = random.NextSingle() * 68 - 34; grid.Insert(i, xs[i], ys[i]); }
        for (int trial = 0; trial < 100; trial++)
        {
            float x = random.NextSingle()*80-40, y = random.NextSingle()*60-30, radius = 5;
            int brute = 0, indexed = 0;
            for (int i = 0; i < 1200; i++) if ((xs[i]-x)*(xs[i]-x)+(ys[i]-y)*(ys[i]-y) <= radius*radius) brute++;
            for (int row = grid.Row(y-radius); row <= grid.Row(y+radius); row++)
                for (int col = grid.Column(x-radius); col <= grid.Column(x+radius); col++)
                    for (int i = grid.Head(col,row); i >= 0; i = grid.Next(i))
                        if ((xs[i]-x)*(xs[i]-x)+(ys[i]-y)*(ys[i]-y) <= radius*radius) indexed++;
            Check(indexed == brute, "grid versus brute force " + trial);
        }
        var key = new PoolKeySO(); var a = new Actor(); var b = new Actor();
        var bank = new[] { a, b }; var pool = new GenericObjectPool<Actor>(key, bank); bank[1] = null;
        Check(pool.TryRent(default, default, out var first) && first == b, "caller cannot mutate pool storage");
        Check(pool.TryRent(default, default, out var second) && second == a, "unique rents");
        Check(!pool.TryRent(default, default, out _), "strict exhaustion");
        Check(pool.Return(first) && !pool.Return(first), "double return rejected");
        var foreignPool = new GenericObjectPool<Actor>(key, new[] { new Actor() });
        foreignPool.TryRent(default, default, out var foreign);
        Check(!pool.Return(foreign) && foreign.IsSpawned, "same key different owner rejected");
        pool.ReturnAll(); Check(pool.Available == 2 && !second.IsSpawned, "return all restores capacity");
        bool rejected = false; var unowned = new Actor();
        try { new GenericObjectPool<Actor>(key, new[] { unowned, unowned }); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "duplicate registration rejected");
        new GenericObjectPool<Actor>(key, new[] { unowned });
        for (int i = 0; i < 10000; i++) { pool.TryRent(default, default, out var actor); pool.Return(actor); }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int t = 0; t < 1000; t++)
        {
            grid.Clear();
            for (int i = 0; i < 1200; i++) grid.Insert(i, xs[i], ys[i]);
            pool.TryRent(default, default, out var actor); pool.Return(actor);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0, "isolated pool/grid steady-state allocation");
        Console.WriteLine($"Parsed {files.Length} C# files. Checks: {checks}; failures: {failures}; isolated pool/grid GC bytes: {allocated}.");
        Console.WriteLine("Unity APIs, Editor generation, engine GC and frame rate NOT validated by this harness.");
        return failures == 0 ? 0 : 1;
    }
}
