using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

using Verify = Benutomo.AutomaticDisposeImpl.Test.CustumAnalyzerTesting.CSharpAnalyzerWithSourceGenerationVerifier<Benutomo.AutomaticDisposeImpl.SourceGenerator.AutomaticDisposeAnalyzer, Benutomo.AutomaticDisposeImpl.SourceGenerator.AutomaticDisposeGenerator>;

namespace Benutomo.AutomaticDisposeImpl.Test
{
    public class AutomaticDisposeAnalyzerTests
    {
        [Fact]
        public async Task SG0001_partialキーワードが未指定()
        {
            //            await new CustumAnalyzerTesting.CSharpAnalyzerWithSourceGenerationTest<AutomaticDisposeAnalyzer, AutomaticDisposeGenerator>()
            //            {
            //                TestState =
            //                {
            //                    Sources =
            //                    {
            //@"
            //using System;
            //using System.Threading.Tasks;
            //using Benutomo;

            //[AutomaticDisposeImpl]
            //partial class A : System.IAsyncDisposable
            //{
            //}
            //"
            //                    },
            //                }
            //            }.RunAsync();

            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
class A : IAsyncDisposable {}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0001").WithLocation(7, 7),
                DiagnosticResult.CompilerError("CS0535").WithLocation(7, 11), // ソース生成がされないパターンのテストなのでDisposeAsync()の実装漏れでCS0535も発生
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0002_必須のインターフェイスが未実装()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A {}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0002").WithLocation(7, 15),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0003_DisposeAsyncでも破棄できるメンバがIDisposableのみを実装しているクラス内に存在している()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A<TAsyncDisposable, TDummy> : IDisposable where TAsyncDisposable : IDisposable, IAsyncDisposable where TDummy : Dummy
{
    private Dummy _field = new();
    private Dummy Property { get; } = new();
    private TAsyncDisposable _field2 = default!;
    private TAsyncDisposable Property2 { get; } = default!;
    private TDummy _field3 = default!;
    private TDummy Property3 { get; } = default!;
}

class Dummy : IDisposable, IAsyncDisposable
{
    public void Dispose() {}
    public ValueTask DisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0003").WithLocation(9, 19).WithArguments("_field", "A"),
                Verify.Diagnostic("SG0003").WithLocation(10, 19).WithArguments("Property", "A"),
                Verify.Diagnostic("SG0003").WithLocation(11, 30).WithArguments("_field2", "A"),
                Verify.Diagnostic("SG0003").WithLocation(12, 30).WithArguments("Property2", "A"),
                Verify.Diagnostic("SG0003").WithLocation(13, 20).WithArguments("_field3", "A"),
                Verify.Diagnostic("SG0003").WithLocation(14, 20).WithArguments("Property3", "A"),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0004_DisposeAsync以外で破棄できないメンバがIDisposableのみを実装しているクラス内に存在している()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A<TAsyncDisposable, TDummy> : IDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
{
    private Dummy _field = new();
    private Dummy Property { get; } = new();
    private IAsyncDisposable _field2 = new Dummy();
    private IAsyncDisposable Property2 { get; } = new Dummy();
    private TAsyncDisposable _field3 = default!;
    private TAsyncDisposable Property3 { get; } = default!;
    private TDummy _field4 = default!;
    private TDummy Property4 { get; } = default!;
}

class Dummy : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0004").WithLocation(9, 19).WithArguments("_field", "A"),
                Verify.Diagnostic("SG0004").WithLocation(10, 19).WithArguments("Property", "A"),
                Verify.Diagnostic("SG0004").WithLocation(11, 30).WithArguments("_field2", "A"),
                Verify.Diagnostic("SG0004").WithLocation(12, 30).WithArguments("Property2", "A"),
                Verify.Diagnostic("SG0004").WithLocation(13, 30).WithArguments("_field3", "A"),
                Verify.Diagnostic("SG0004").WithLocation(14, 30).WithArguments("Property3", "A"),
                Verify.Diagnostic("SG0004").WithLocation(15, 20).WithArguments("_field4", "A"),
                Verify.Diagnostic("SG0004").WithLocation(16, 20).WithArguments("Property4", "A"),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0005_AutomaticDisposeImpl属性を付与していないクラスのメンバに対してAutomaticDisposeImplMode属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
{
    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private Dummy _field = new();

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private Dummy Property { get; } = new();

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private IAsyncDisposable _field2 = new Dummy();

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private IAsyncDisposable Property2 { get; } = new Dummy();

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private TAsyncDisposable _field3 = default!;

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private TAsyncDisposable Property3 { get; } = default!;

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private TDummy _field4 = default!;

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Default)]
    private TDummy Property4 { get; } = default!;
}

class Dummy : IDisposable, IAsyncDisposable
{
    public void Dispose() {}
    public ValueTask DisposeAsync() => default;
}
";

            var expected = new[]
            {
                DiagnosticResult.CompilerError("CS0535").WithLocation(6, 45), // ソース生成がされないパターンのテストなのでDispose()の実装漏れでCS0535も発生
                DiagnosticResult.CompilerError("CS0535").WithLocation(6, 58), // ソース生成がされないパターンのテストなのでDisposeAsync()の実装漏れでCS0535も発生
                Verify.Diagnostic("SG0005").WithLocation(9, 19),
                Verify.Diagnostic("SG0005").WithLocation(12, 19),
                Verify.Diagnostic("SG0005").WithLocation(15, 30),
                Verify.Diagnostic("SG0005").WithLocation(18, 30),
                Verify.Diagnostic("SG0005").WithLocation(21, 30),
                Verify.Diagnostic("SG0005").WithLocation(24, 30),
                Verify.Diagnostic("SG0005").WithLocation(27, 20),
                Verify.Diagnostic("SG0005").WithLocation(30, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0006_AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してUnmanagedResourceReleaseMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A
{
    [UnmanagedResourceReleaseMethod]
    void MyFinalize() {}
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0006").WithLocation(9, 10),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0016_UnmanagedResourceReleaseMethod属性を複数のメソッドに付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [UnmanagedResourceReleaseMethod]
    void MyFinalize1() {}

    [UnmanagedResourceReleaseMethod]
    void MyFinalize2() {}
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0016").WithLocation(10, 10),
                Verify.Diagnostic("SG0016").WithLocation(13, 10),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0007_IDisposableを実装していないクラスのメソッドに対してManagedObjectDisposeMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IAsyncDisposable
{
    [ManagedObjectDisposeMethod]
    void MyDispose() {}
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0007").WithLocation(10, 10),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0008_AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectDisposeMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A : IDisposable
{
    [ManagedObjectDisposeMethod]
    void MyDispose() {}

    public void Dispose() {}
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0008").WithLocation(9, 10),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0009_ManagedObjectDisposeMethod属性を一つのクラス内で複数のメソッドに付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [ManagedObjectDisposeMethod]
    void MyDispose1() {}

    [ManagedObjectDisposeMethod]
    void MyDispose2() {}
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0009").WithLocation(10, 10),
                Verify.Diagnostic("SG0009").WithLocation(13, 10),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0010_不適当なシグネチャのメソッドにManagedObjectDisposeMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [ManagedObjectDisposeMethod]
    int MyDispose() => 0;
}
";

            var expected1 = new[]
            {
                Verify.Diagnostic("SG0010").WithLocation(10, 9),
            };

            await Verify.VerifyAnalyzerAsync(source, expected1);


            var source2 = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [ManagedObjectDisposeMethod]
    void MyDispose(int n) {}
}
";
            const string genSourcePath = @"Benutomo.AutomaticDisposeImpl.SourceGenerator\Benutomo.AutomaticDisposeImpl.SourceGenerator.AutomaticDisposeGenerator\gen.A.AutomaticDisposeImpl.cs";

            var expected2 = new[]
            {
                Verify.Diagnostic("SG0010").WithLocation(10, 10),
                DiagnosticResult.CompilerError("CS7036").WithLocation(genSourcePath, 25, 26), // 生成されるコードでシグネチャ違反によるCS7036も発生
            };

            await Verify.VerifyAnalyzerAsync(source2, expected2);
        }










        [Fact]
        public async Task SG0011_IAsyncDisposableを実装していないクラスのメソッドに対してManagedObjectAsyncDisposeMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0011").WithLocation(10, 15),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0012_AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectAsyncDisposeMethod属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A : IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDisposeAsync() => default;

    public ValueTask DisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0012").WithLocation(9, 15),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0013_ManagedObjectAsyncDisposeMethod属性を一つのクラス内で複数のメソッドに付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDisposeAsync1() => default;

    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDisposeAsync2() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0013").WithLocation(10, 15),
                Verify.Diagnostic("SG0013").WithLocation(13, 15),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0014_不適当なシグネチャのメソッドにManagedObjectAsyncDisposeMethod属性を付与()
        {
            const string genSourcePath = @"Benutomo.AutomaticDisposeImpl.SourceGenerator\Benutomo.AutomaticDisposeImpl.SourceGenerator.AutomaticDisposeGenerator\gen.A.AutomaticDisposeImpl.cs";

            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    int MyDispose() => 0;
}
";

            var expected1 = new[]
            {
                Verify.Diagnostic("SG0014").WithLocation(10, 9),
                DiagnosticResult.CompilerError("CS1061").WithLocation(genSourcePath, 37, 40), // 生成されるコードでConfigureAwait()未定義によるCS1061も発生
            };

            await Verify.VerifyAnalyzerAsync(source, expected1);


            var source2 = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    Task MyDispose(int n) => null!;
}
";

            var expected2 = new[]
            {
                Verify.Diagnostic("SG0014").WithLocation(10, 10),
                DiagnosticResult.CompilerError("CS7036").WithLocation(genSourcePath, 37, 28), // 生成されるコードでシグネチャ違反によるCS7036も発生
            };

            await Verify.VerifyAnalyzerAsync(source2, expected2);

            var source3 = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDispose(int n) => default;
}
";

            var expected3 = new[]
            {
                Verify.Diagnostic("SG0014").WithLocation(10, 15),
                DiagnosticResult.CompilerError("CS7036").WithLocation(genSourcePath, 37, 28), // 生成されるコードでシグネチャ違反によるCS7036も発生
            };

            await Verify.VerifyAnalyzerAsync(source3, expected3);
        }

        [Fact]
        public async Task SG0015_非同期的に破棄された場合の処理と対に定義すべき同期的に破棄された場合の処理が未実装()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable, IAsyncDisposable
{
    [ManagedObjectAsyncDisposeMethod]
    ValueTask MyDisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0015").WithLocation(7, 15).WithArguments("MyDisposeAsync"), // クラスの識別子に対して指摘
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
