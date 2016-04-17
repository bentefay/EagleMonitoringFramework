using EqualityComparers;
using Newtonsoft.Json;

namespace Emf.Web.Ui.Models
{
    public class TestRun : EquatableBase<TestRun>
    {
        static TestRun()
        {
            DefaultComparer = EqualityCompare<TestRun>.EquateBy(t => t.Id);
        }

        [JsonConstructor]
        public TestRun(int id, int incompleteTests, int passedTests, int notApplicableTests, int totalTests, string errorMessages)
        {
            Id = id;
            IncompleteTests = incompleteTests;
            PassedTests = passedTests;
            NotApplicableTests = notApplicableTests;
            TotalTests = totalTests;
            ErrorMessages = errorMessages;
        }

        public TestRun(Microsoft.TeamFoundation.TestManagement.WebApi.TestRun testRun)
        {
            Id = testRun.Id;
            PassedTests = testRun.PassedTests;
            IncompleteTests = testRun.IncompleteTests;
            NotApplicableTests = testRun.NotApplicableTests;
            TotalTests = testRun.TotalTests;
            ErrorMessages = testRun.ErrorMessage;
        }

        public int Id { get; }
        public int IncompleteTests { get; }
        public int PassedTests { get; }
        public int NotApplicableTests { get; set; }
        public int TotalTests { get; }
        public string ErrorMessages { get; }
    }
}