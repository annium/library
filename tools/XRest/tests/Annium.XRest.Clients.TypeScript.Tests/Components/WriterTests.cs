using System;
using System.IO;
using Annium.Testing;
using Annium.XRest.Clients.Shared.Components;
using Annium.XRest.Clients.TypeScript.Components.Implementations;
using Annium.XRest.Clients.TypeScript.Views;
using Xunit;

namespace Annium.XRest.Clients.TypeScript.Tests.Components;

public class WriterTests
{
    private static readonly ApiView _emptyApi = new([], []);

    [Fact]
    public void Write_FilesystemRoot_Refuses()
    {
        // arrange — the writer deletes the output directory recursively before generating
        var writer = new Writer(new ThrowingTemplateWriter());
        var root = Path.GetPathRoot(Directory.GetCurrentDirectory())!;

        // act
        var write = Wrap.It(() => writer.Write(root, _emptyApi));

        // assert
        write.Throws<InvalidOperationException>().Message.IsNotEmpty();
    }

    [Fact]
    public void Write_CurrentDirectory_Refuses()
    {
        // arrange
        var writer = new Writer(new ThrowingTemplateWriter());

        // act
        var write = Wrap.It(() => writer.Write(Directory.GetCurrentDirectory(), _emptyApi));

        // assert
        write.Throws<InvalidOperationException>().Message.IsNotEmpty();
    }

    [Fact]
    public void Write_RepositoryRoot_Refuses()
    {
        // arrange — a directory containing .git is a working tree, never a generation target
        var writer = new Writer(new ThrowingTemplateWriter());
        var target = Path.Combine(Path.GetTempPath(), $"xrest-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(target, ".git"));

        try
        {
            // act
            var write = Wrap.It(() => writer.Write(target, _emptyApi));

            // assert
            write.Throws<InvalidOperationException>().Message.IsNotEmpty();
            Directory.Exists(Path.Combine(target, ".git")).IsTrue();
        }
        finally
        {
            Directory.Delete(target, true);
        }
    }

    [Fact]
    public void Write_OrdinaryDirectory_Proceeds()
    {
        // arrange
        var writer = new Writer(new ThrowingTemplateWriter());
        var target = Path.Combine(Path.GetTempPath(), $"xrest-writer-{Guid.NewGuid():N}", "out");

        try
        {
            // act — an empty api writes no file, so the throwing template writer is never reached
            writer.Write(target, _emptyApi);

            // assert
            Directory.Exists(target).IsTrue();
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(target)!, true);
        }
    }

    private sealed class ThrowingTemplateWriter : ITemplateWriter
    {
        public string Write<T>(string template, T data)
            where T : class => throw new NotSupportedException("no template rendering expected in these tests");
    }
}
