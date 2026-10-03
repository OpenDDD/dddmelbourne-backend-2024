using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using TestStack.BDDfy;
using TestStack.BDDfy.Configuration;
using Xunit;

namespace DDD.Sessionize.Tests.TestHelpers
{
    public abstract class Scenario
    {
        static Scenario()
        {
            Configurator.Processors.Add(() => Xunit2BddfyTextReporter.Instance);
        }

        protected void Approve(string textToApprove, string extension = "txt", [CallerMemberName] string testMethod = "")
        {
            var name = $"{GetType().Name}_{testMethod}";
            textToApprove.ShouldMatchApproved(b => b
                .UseCallerLocation()
                .WithFilenameGenerator((_, _, fileType, fileExtension) => $"{name}.{fileType}.{fileExtension}")
                .WithFileExtension(extension)
                .WithScrubber(_guidScrubber.Scrub)
                .NoDiff()
            );
        }

        public string GetResource(string fileName)
        {
            using (var sr = new StreamReader(GetType().Assembly
                .GetManifestResourceStream(GetType(), fileName)))
            {
                return sr.ReadToEnd();
            }
        }

        [Fact]
        public virtual void Run()
        {
            this.BDDfy(GetType().Name);
        }

        private readonly GuidScrubber _guidScrubber = new GuidScrubber();
    }
}
