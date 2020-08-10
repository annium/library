using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Tests
{
    public class ObjectContainerTest : TestBase
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
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
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
            var otherValue = new User
            {
                Name = "Lex",
            };
            var state = factory.Create(initialValue);

            // act
            state.Set(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();

            // act
            state.Set(otherValue);

            // assert
            state.Value.IsEqual(otherValue);
            state.At(x => x.Name).Value.IsEqual(otherValue.Name);
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Reset_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var otherValue = new User
            {
                Name = "Lex",
            };
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
            state.At(x => x.Name).Value.IsEqual(initialValue.Name);
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

        private User Arrange() => new User
        {
            Name = "Max",
            Age = 20,
        };

        private class User
        {
            public string Name { get; set; } = string.Empty;
            public uint Age { get; set; }
        }
    }
}