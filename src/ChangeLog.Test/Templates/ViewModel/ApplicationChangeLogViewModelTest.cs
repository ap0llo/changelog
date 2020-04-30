using System;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="ApplicationChangeLogViewModel"/>
    /// </summary>
    public class ApplicationChangeLogViewModelTest : TestBase
    {
        [Fact]
        public void Model_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ApplicationChangeLogViewModel(null!));
        }

        [Fact]
        public void Versions_includes_a_view_model_for_all_versions_in_the_model()
        {
            // ARRANGE

            var model = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3"),
                GetSingleVersionChangeLog("2.3.4")
            };


            // ACT 
            var viewModel = new ApplicationChangeLogViewModel(model);

            // ASSERT
            Assert.NotNull(viewModel.Versions);
            Assert.Equal(2, viewModel.Versions.Count);
        }

        [Fact]
        public void Versions_are_sorted_by_versions_descending()
        {
            // ARRANGE
            var model = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3"),
                GetSingleVersionChangeLog("2.3.4")
            };

            // ACT 
            var viewModel = new ApplicationChangeLogViewModel(model);

            // ASSERT
            // ordered descending => newest version first
            Assert.Equal("2.3.4", viewModel.Versions[0].VersionDisplayName);
            Assert.Equal("1.2.3", viewModel.Versions[1].VersionDisplayName);
        }
    }
}
