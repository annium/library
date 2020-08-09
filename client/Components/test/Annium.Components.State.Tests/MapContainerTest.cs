using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Tests
{
    public class MapContainerTest : TestBase
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
            var key = initialValue.Keys.First();
            state.At(x => x[key]).Value.IsEqual(initialValue.At(key));
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
            state.At(x => x["a"]).Value.IsEqual(initialValue.At("a"));
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(otherValue);

            // assert
            state.Value.IsEqual(otherValue);
            state.At(x => x["c"]).Value.IsEqual(otherValue.At("c"));
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(initialValue);

            // assert
            state.Value.IsEqual(initialValue);
            state.At(x => x["a"]).Value.IsEqual(initialValue.At("a"));
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
            state.At(x => x["c"]).SetStatus(Status.Validating);

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
            state.At(x => x["c"]).Value.IsEqual(initialValue.At("c"));
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
            state.At(x => x["a"]).SetStatus(Status.Validating);

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
            state.Add("d", 7);

            // assert
            state.Value.IsEqual(new Dictionary<string, int>
            {
                { "a", 2 },
                { "b", 4 },
                { "c", 8 },
                { "d", 7 },
            });
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Remove_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initialValue = Arrange();
            var state = factory.Create(initialValue);

            // act
            state.Remove("b");

            // assert
            state.Value.IsEqual(new Dictionary<string, int>
            {
                { "a", 2 },
                { "c", 8 },
            });
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
            state.Remove("a");
            state.Add("e", 9);
            state.Set(initialValue);

            // assert
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        private IReadOnlyDictionary<string, int> Arrange() => new Dictionary<string, int>
        {
            { "a", 2 },
            { "b", 4 },
            { "c", 8 },
        };

        private IReadOnlyDictionary<string, int> ArrangeOther() => new Dictionary<string, int>
        {
            { "a", 2 },
            { "c", 4 }
        };
    }
}