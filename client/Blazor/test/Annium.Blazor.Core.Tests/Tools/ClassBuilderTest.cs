using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Extensions.Primitives;
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
            var genderClasses = new Dictionary<Gender, string> { { Gender.Male, "male" }, { Gender.Female, "female" } };
            var cb = new ClassBuilder<User>()
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

            // assert
            var unnamed = cb.Build(new User());
            unnamed.IsEqual("plain plain-if get get-if _val _val-if male");
            var named = cb.Build(new User { Gender = Gender.Female, Name = "x" });
            named.IsEqual("plain plain-if plain-if-value get get-if get-if-value x_val x_val-if x_val-if-value female");
        }

        [Fact]
        public void ClassBuilder_Works()
        {
            // arrange
            var genderClasses = new Dictionary<Gender, string> { { Gender.Male, "male" }, { Gender.Female, "female" } };
            var cb = new ClassBuilder()
                .With("plain")
                .With(() => true, "plain-if")
                .With(() => "get")
                .With(() => false, () => "get-if")
                .With(Gender.Male, genderClasses);

            // assert
            var className = cb.Build();
            className.IsEqual("plain plain-if get male");
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