using System;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Components.Forms.Tests
{
    public class ComplexTest : TestBase
    {
        [Fact]
        public void Init_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();

            // act
            var state = factory.Create(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Author).At(x => x.Name).Value.IsEqual(initialValue.Author.Name);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();
            state.IsStatus(Status.None).IsTrue();
            state.HasStatus(Status.None).IsTrue();
        }

        [Fact]
        public void Set_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var otherValue = ArrangeOther();
            var state = factory.Create(initialValue);

            // act
            state.Set(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(otherValue);

            // assert
            state.Value.IsEqual(otherValue);
            state.At(x => x.Name).Value.IsEqual(otherValue.Name);
            state.At(x => x.Author).Value.IsEqual(otherValue.Author);
            state.At(x => x.Messages).Value.IsEqual(otherValue.Messages);
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
            state.At(x => x.Author).Value.IsEqual(initialValue.Author);
            state.At(x => x.Messages).Value.IsEqual(initialValue.Messages);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Reset_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var otherValue = ArrangeOther();
            var state = factory.Create(initialValue);

            // act
            state.Set(otherValue);
            state.At(x => x.Name).SetStatus(Status.Validating);

            // assert
            state.Value.IsEqual(otherValue);
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();
            state.IsStatus(Status.None, Status.Validating).IsTrue();
            state.HasStatus(Status.Validating).IsTrue();

            // act
            state.Reset();

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Messages).At(x => x[0]).At(x => x.Text).Value.IsEqual(initialValue.Messages.At(0).Text);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();
            state.IsStatus(Status.None).IsTrue();
            state.HasStatus(Status.None).IsTrue();
        }

        [Fact]
        public void Status_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.At(x => x.Name).SetStatus(Status.Validating);

            // assert
            state.IsStatus(Status.None, Status.Validating).IsTrue();
            state.IsStatus(Status.Validating).IsFalse();
            state.HasStatus(Status.None, Status.Validating).IsTrue();
            state.HasStatus(Status.None, Status.Error).IsTrue();
            state.HasStatus(Status.Error).IsFalse();
        }

        [Fact]
        public void Mutation_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.At(x => x.Name).SetStatus(Status.Validating);

            // assert
            state.IsStatus(Status.None, Status.Validating).IsTrue();
            state.IsStatus(Status.Validating).IsFalse();
            state.HasStatus(Status.None, Status.Validating).IsTrue();
            state.HasStatus(Status.None, Status.Error).IsTrue();
            state.HasStatus(Status.Error).IsFalse();
        }

        private Blog Arrange() => new Blog
        {
            Name = "Sample",
            Author = new User { Name = "Max" },
            Messages = new[] { new Message { Text = "one", IsRead = true }, new Message { Text = "two" } },
        };

        private Blog ArrangeOther() => new Blog
        {
            Name = "Demo",
            Author = new User { Name = "Lex" },
            Messages = new[] { new Message { Text = "three", IsRead = true }, new Message { Text = "four" }, new Message { Text = "five", IsRead = true } },
        };

        private class Blog
        {
            public string Name { get; set; } = string.Empty;
            public User Author { get; set; } = default!;
            public IEnumerable<Message> Messages { get; set; } = Array.Empty<Message>();
        }

        private class User
        {
            public string Name { get; set; } = string.Empty;
        }

        private class Message
        {
            public string Text { get; set; } = string.Empty;
            public bool IsRead { get; set; }
        }
    }
}