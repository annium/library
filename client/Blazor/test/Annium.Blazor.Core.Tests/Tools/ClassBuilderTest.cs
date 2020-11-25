using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Core.Primitives;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Core.Tests.Tools
{
    public class ClassBuilderTest
    {
        [Fact]
        public void ClassBuilderT_Works()
        {
            // arrange
            var genderClasses = new Dictionary<Gender, string?> { { Gender.Male, "male" }, { Gender.Female, "female" } };
            var cb = ClassBuilder<User>
                .With("plain")
                .With(() => true, "plain-if")
                .With(x => !x.Name.IsNullOrWhiteSpace(), "plain-if-value")
                .With(() => "get")
                .With(() => true, () => "get-if")
                .With(x => !x.Name.IsNullOrWhiteSpace(), () => "get-if-value")
                .With(x => $"{x.Name}_val")
                .With(() => true, x => $"{x.Name}_val-if")
                .With(x => !x.Name.IsNullOrWhiteSpace(), x => $"{x.Name}_val-if-value")
                .With(x => x.Gender, genderClasses);

            // act
            var unnamed = cb.Build(new User());
            var named = cb.Build(new User { Gender = Gender.Female, Name = "x" });

            // assert
            unnamed.IsEqual("plain plain-if get get-if _val _val-if male");
            named.IsEqual("plain plain-if plain-if-value get get-if get-if-value x_val x_val-if x_val-if-value female");
        }

        [Fact]
        public void ClassBuilderT_Clone_Works()
        {
            // arrange
            var cb = ClassBuilder<User>.With("plain");

            // act
            var one = cb.Clone().With("one").Build(new User());
            var two = cb.Clone().With("two").Build(new User());

            // assert
            one.IsEqual("plain one");
            two.IsEqual("plain two");
        }

        [Fact]
        public void ClassBuilder_Works()
        {
            // arrange
            var genderClasses = new Dictionary<Gender, string?> { { Gender.Male, "male" }, { Gender.Female, "female" } };
            var cb = ClassBuilder
                .With("plain")
                .With(() => true, "plain-if")
                .With(() => "get")
                .With(() => false, () => "get-if")
                .With(Gender.Male, genderClasses);

            // act
            var className = cb.Build();

            // assert
            className.IsEqual("plain plain-if get male");
        }

        [Fact]
        public void ClassBuilder_Clone_Works()
        {
            // arrange
            var cb = ClassBuilder.With("plain");

            // act
            var one = cb.Clone().With("one").Build();
            var two = cb.Clone().With("two").Build();

            // assert
            one.IsEqual("plain one");
            two.IsEqual("plain two");
        }

        private class User
        {
            public Gender Gender { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private enum Gender : byte
        {
            Male,
            Female,
        }
    }
}