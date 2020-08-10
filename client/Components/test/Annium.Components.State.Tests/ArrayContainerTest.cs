using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Tests
{
    public class ArrayContainerTest : TestBase
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
            var i = 0;
            state.At(x => x[i]).Value.IsEqual(initialValue.At(0));
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
            var otherValue = new[] { 4, 2 };
            var state = factory.Create(initialValue);

            // act
            state.Set(initialValue.ToArray());

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x[0]).Value.IsEqual(initialValue.At(0));
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();

            // act
            state.Set(otherValue);

            // assert
            state.Value.IsEqual(otherValue);
            state.At(x => x[0]).Value.IsEqual(otherValue.At(0));
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(initialValue.ToArray());

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x[0]).Value.IsEqual(initialValue.At(0));
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Reset_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var otherValue = new[] { 4, 2 };
            var state = factory.Create(initialValue);

            // act
            state.Set(otherValue);
            state.At(x => x[0]).SetStatus(Status.Validating);

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
            state.At(x => x[0]).Value.IsEqual(initialValue.At(0));
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
            state.At(x => x[0]).SetStatus(Status.Validating);

            // assert
            state.IsStatus(Status.None, Status.Validating).IsTrue();
            state.IsStatus(Status.Validating).IsFalse();
            state.HasStatus(Status.None, Status.Validating).IsTrue();
            state.HasStatus(Status.None, Status.Error).IsTrue();
            state.HasStatus(Status.Error).IsFalse();
        }

        [Fact]
        public void Add_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.Add(10);

            // assert
            state.Value.IsEqual(new[] { 2, 8, 10 });
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Insert_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.Insert(0, 10);

            // assert
            state.Value.IsEqual(new[] { 10, 2, 8 });
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void RemoveAt_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.RemoveAt(1);

            // assert
            state.Value.IsEqual(new[] { 2 });
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void HasChanged_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.RemoveAt(0);
            state.Add(1);
            state.Set(initialValue.ToArray());

            // assert
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        private IReadOnlyCollection<int> Arrange() => new[] { 2, 8 };
    }
}