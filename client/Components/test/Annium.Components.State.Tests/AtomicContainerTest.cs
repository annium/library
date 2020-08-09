using Annium.Testing;
using Xunit;

namespace Annium.Components.Forms.Tests
{
    public class AtomicContainerTest : TestBase
    {
        [Fact]
        public void Init_Ok()
        {
            // arrange
            var factory = GetFactory();

            // act
            var state = factory.Create(5);

            // assert
            state.Value.IsEqual(5);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();
        }

        [Fact]
        public void Change_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initial = 5;
            var other = 10;
            var state = factory.Create(initial);

            // act
            state.Set(other);

            // assert
            state.Value.IsEqual(other);
            state.HasChanged.IsTrue();
            state.HasBeenTouched.IsTrue();

            // act
            state.Set(initial);

            // assert
            state.Value.IsEqual(initial);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsTrue();
        }

        [Fact]
        public void Reset_Ok()
        {
            // arrange
            var factory = GetFactory();
            var initial = 5;
            var other = 10;
            var state = factory.Create(initial);
            state.Set(other);

            // act
            state.Reset();

            // assert
            state.Value.IsEqual(initial);
            state.HasChanged.IsFalse();
            state.HasBeenTouched.IsFalse();
        }
    }
}