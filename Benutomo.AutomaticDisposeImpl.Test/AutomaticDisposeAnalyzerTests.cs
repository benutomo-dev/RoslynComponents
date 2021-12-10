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
    [EnableAutomaticDispose]
    private Dummy _field = new();

    [EnableAutomaticDispose]
    private Dummy Property { get; } = new();

    [EnableAutomaticDispose]
    private TAsyncDisposable _field2 = default!;

    [EnableAutomaticDispose]
    private TAsyncDisposable Property2 { get; } = default!;

    [EnableAutomaticDispose]
    private TDummy _field3 = default!;

    [EnableAutomaticDispose]
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
                Verify.Diagnostic("SG0003").WithLocation(10, 19).WithArguments("_field", "A"),
                Verify.Diagnostic("SG0003").WithLocation(13, 19).WithArguments("Property", "A"),
                Verify.Diagnostic("SG0003").WithLocation(16, 30).WithArguments("_field2", "A"),
                Verify.Diagnostic("SG0003").WithLocation(19, 30).WithArguments("Property2", "A"),
                Verify.Diagnostic("SG0003").WithLocation(22, 20).WithArguments("_field3", "A"),
                Verify.Diagnostic("SG0003").WithLocation(25, 20).WithArguments("Property3", "A"),
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
    [EnableAutomaticDispose]
    private Dummy _field = new();

    [EnableAutomaticDispose]
    private Dummy Property { get; } = new();

    [EnableAutomaticDispose]
    private IAsyncDisposable _field2 = new Dummy();

    [EnableAutomaticDispose]
    private IAsyncDisposable Property2 { get; } = new Dummy();

    [EnableAutomaticDispose]
    private TAsyncDisposable _field3 = default!;

    [EnableAutomaticDispose]
    private TAsyncDisposable Property3 { get; } = default!;

    [EnableAutomaticDispose]
    private TDummy _field4 = default!;

    [EnableAutomaticDispose]
    private TDummy Property4 { get; } = default!;
}

class Dummy : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0004").WithLocation(10, 19).WithArguments("_field", "A"),
                Verify.Diagnostic("SG0004").WithLocation(13, 19).WithArguments("Property", "A"),
                Verify.Diagnostic("SG0004").WithLocation(16, 30).WithArguments("_field2", "A"),
                Verify.Diagnostic("SG0004").WithLocation(19, 30).WithArguments("Property2", "A"),
                Verify.Diagnostic("SG0004").WithLocation(22, 30).WithArguments("_field3", "A"),
                Verify.Diagnostic("SG0004").WithLocation(25, 30).WithArguments("Property3", "A"),
                Verify.Diagnostic("SG0004").WithLocation(28, 20).WithArguments("_field4", "A"),
                Verify.Diagnostic("SG0004").WithLocation(31, 20).WithArguments("Property4", "A"),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0005_AutomaticDisposeImpl属性を付与していないクラスのメンバに対してEnableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
{

    [EnableAutomaticDispose]
    private Dummy _field = new();

    [EnableAutomaticDispose]
    private Dummy Property { get; } = new();

    [EnableAutomaticDispose]
    private IAsyncDisposable _field2 = new Dummy();

    [EnableAutomaticDispose]
    private IAsyncDisposable Property2 { get; } = new Dummy();

    [EnableAutomaticDispose]
    private TAsyncDisposable _field3 = default!;

    [EnableAutomaticDispose]
    private TAsyncDisposable Property3 { get; } = default!;

    [EnableAutomaticDispose]
    private TDummy _field4 = default!;

    [EnableAutomaticDispose]
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
                Verify.Diagnostic("SG0005").WithLocation(10, 19),
                Verify.Diagnostic("SG0005").WithLocation(13, 19),
                Verify.Diagnostic("SG0005").WithLocation(16, 30),
                Verify.Diagnostic("SG0005").WithLocation(19, 30),
                Verify.Diagnostic("SG0005").WithLocation(22, 30),
                Verify.Diagnostic("SG0005").WithLocation(25, 30),
                Verify.Diagnostic("SG0005").WithLocation(28, 20),
                Verify.Diagnostic("SG0005").WithLocation(31, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0017_AutomaticDisposeImpl属性を付与していないクラスのメンバに対してDisableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
{

    [DisableAutomaticDispose]
    private Dummy _field5 = new();

    [DisableAutomaticDispose]
    private Dummy Property5 { get; } = new();

    [DisableAutomaticDispose]
    private IAsyncDisposable _field6 = new Dummy();

    [DisableAutomaticDispose]
    private IAsyncDisposable Property6 { get; } = new Dummy();

    [DisableAutomaticDispose]
    private TAsyncDisposable _field7 = default!;

    [DisableAutomaticDispose]
    private TAsyncDisposable Property7 { get; } = default!;

    [DisableAutomaticDispose]
    private TDummy _field8 = default!;

    [DisableAutomaticDispose]
    private TDummy Property8 { get; } = default!;
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
                Verify.Diagnostic("SG0017").WithLocation(10, 19),
                Verify.Diagnostic("SG0017").WithLocation(13, 19),
                Verify.Diagnostic("SG0017").WithLocation(16, 30),
                Verify.Diagnostic("SG0017").WithLocation(19, 30),
                Verify.Diagnostic("SG0017").WithLocation(22, 30),
                Verify.Diagnostic("SG0017").WithLocation(25, 30),
                Verify.Diagnostic("SG0017").WithLocation(28, 20),
                Verify.Diagnostic("SG0017").WithLocation(31, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0018_EnableAutomaticDispose属性とDisableAutomaticDispose属性を同一メンバに同時に付与()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
{
    [EnableAutomaticDispose,DisableAutomaticDispose]
    private Dummy _field = new();

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private Dummy Property { get; } = new();

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private IAsyncDisposable _field2 = new Dummy();

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private IAsyncDisposable Property2 { get; } = new Dummy();

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private TAsyncDisposable _field3 = default!;

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private TAsyncDisposable Property3 { get; } = default!;

    [EnableAutomaticDispose,DisableAutomaticDispose]
    private TDummy _field4 = default!;

    [EnableAutomaticDispose,DisableAutomaticDispose]
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
                Verify.Diagnostic("SG0018").WithLocation(10, 19),
                Verify.Diagnostic("SG0018").WithLocation(13, 19),
                Verify.Diagnostic("SG0018").WithLocation(16, 30),
                Verify.Diagnostic("SG0018").WithLocation(19, 30),
                Verify.Diagnostic("SG0018").WithLocation(22, 30),
                Verify.Diagnostic("SG0018").WithLocation(25, 30),
                Verify.Diagnostic("SG0018").WithLocation(28, 20),
                Verify.Diagnostic("SG0018").WithLocation(31, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0019_自動実装がExplicitモードでEnableAutomaticDispose属性またはDisableAutomaticDispose属性が付与されていない()
        {
            {
                var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl]
partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
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

class Dummy : IDisposable, IAsyncDisposable
{
    public void Dispose() {}
    public ValueTask DisposeAsync() => default;
}
";

                var expected = new[]
                {
                    Verify.Diagnostic("SG0019").WithLocation(10, 19),
                    Verify.Diagnostic("SG0019").WithLocation(13, 19),
                    Verify.Diagnostic("SG0019").WithLocation(16, 30),
                    Verify.Diagnostic("SG0019").WithLocation(19, 30),
                    Verify.Diagnostic("SG0019").WithLocation(22, 30),
                    Verify.Diagnostic("SG0019").WithLocation(25, 30),
                    Verify.Diagnostic("SG0019").WithLocation(28, 20),
                    Verify.Diagnostic("SG0019").WithLocation(31, 20),
                };

                await Verify.VerifyAnalyzerAsync(source, expected);
            }

            {
                var source = @"
using System;
using System.Threading.Tasks;
using Benutomo;

[AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Explicit)]
partial class A<TAsyncDisposable, TDummy> : IDisposable, IAsyncDisposable where TAsyncDisposable : IAsyncDisposable where TDummy : Dummy
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

class Dummy : IDisposable, IAsyncDisposable
{
    public void Dispose() {}
    public ValueTask DisposeAsync() => default;
}
";

                var expected = new[]
                {
                    Verify.Diagnostic("SG0019").WithLocation(10, 19),
                    Verify.Diagnostic("SG0019").WithLocation(13, 19),
                    Verify.Diagnostic("SG0019").WithLocation(16, 30),
                    Verify.Diagnostic("SG0019").WithLocation(19, 30),
                    Verify.Diagnostic("SG0019").WithLocation(22, 30),
                    Verify.Diagnostic("SG0019").WithLocation(25, 30),
                    Verify.Diagnostic("SG0019").WithLocation(28, 20),
                    Verify.Diagnostic("SG0019").WithLocation(31, 20),
                };

                await Verify.VerifyAnalyzerAsync(source, expected);
            }
        }

        [Fact]
        public async Task SG0020_IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにEnableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [EnableAutomaticDispose]
    private object _field = new();

    [EnableAutomaticDispose]
    private object Property { get; } = new();
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0020").WithLocation(9, 20),
                Verify.Diagnostic("SG0020").WithLocation(12, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0021_IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにDisableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [DisableAutomaticDispose]
    private object _field = new();

    [DisableAutomaticDispose]
    private object Property { get; } = new();
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0021").WithLocation(9, 20),
                Verify.Diagnostic("SG0021").WithLocation(12, 20),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0022_staticメンバにEnableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [EnableAutomaticDispose]
    private static object _field = new();

    [EnableAutomaticDispose]
    private static object Property { get; } = new();
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0022").WithLocation(9, 27),
                Verify.Diagnostic("SG0022").WithLocation(12, 27),
            };

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task SG0023_staticメンバにDisableAutomaticDispose属性を付与()
        {
            var source = @"
using System;
using Benutomo;

[AutomaticDisposeImpl]
partial class A : IDisposable
{
    [DisableAutomaticDispose]
    private static object _field = new();

    [DisableAutomaticDispose]
    private static object Property { get; } = new();
}
";

            var expected = new[]
            {
                Verify.Diagnostic("SG0023").WithLocation(9, 27),
                Verify.Diagnostic("SG0023").WithLocation(12, 27),
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
