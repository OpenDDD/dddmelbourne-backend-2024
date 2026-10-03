using System;
using Shouldly;
using Xunit;

namespace DDD.Sessionize.Tests.Encryption
{
    public class EncryptorTests
    {
        // Produced by the AesManaged and SHA256Managed implementation, so this proves that existing payloads still decrypt.
        private const string LegacyCipherText = "x7Sr3gSNU1xTjYA+KLg2nXFZFcXIXbs81NPOsDke+LgUsMe842g3PoYlEAM0toIsUSg1uk2BNhR9pc66hfOzcrXaQgSe/DxKPDd5CEn210U5lFdiyeqHHxmBbQwXGH5iLWHMc6FEMDrticfbGciRKGM4ts/jw8hLaDCtj6xxjeWveZHMNL59HtpfuB6ScwIE";
        private const string LegacyPassword = "correct horse battery staple";

        private const string LegacySubmissionCipherText = "+rLXBILtYyuKvuKS+Gj2oUT0n3POuKWYFNiI50Z9XAFutYjk2FIMXb9uZWAWpxVe5+rjzWNHV5ISXrqizOCuER1omcL6d5Di9plZXFG9BmaAtXGBZDVQi1VwTo0dRF6C";
        private const string LegacySubmissionPassword = "pässwörd";

        [Fact]
        public void DecryptsCipherTextFromTheLegacyImplementation()
        {
            var (voteId, submissionId, unixTime) = Encryptor.DecryptSubmissionId(LegacyCipherText, LegacyPassword);

            voteId.ShouldBe("11111111-aaaa-bbbb-cccc-222222222222");
            submissionId.ShouldBe("33333333-dddd-eeee-ffff-444444444444");
            unixTime.ShouldBe(1717200000);
        }

        [Fact]
        public void DecryptsSubmissionIdFromTheLegacyImplementation()
        {
            var (voteId, submissionId, unixTime) = Encryptor.DecryptSubmissionId(LegacySubmissionCipherText, LegacySubmissionPassword);

            voteId.ShouldBe("vote-id");
            submissionId.ShouldBe("submission-id");
            unixTime.ShouldBe(1717200000);
        }

        [Fact]
        public void RoundTripsSubmissionId()
        {
            var encrypted = Encryptor.EncryptSubmissionId("vote-id", "submission-id", LegacyPassword, 1717200000);

            Encryptor.DecryptSubmissionId(encrypted, LegacyPassword).ShouldBe(("vote-id", "submission-id", 1717200000L).ToTuple());
        }

        [Fact]
        public void UsesAFreshIvForEachEncryption()
        {
            var first = Encryptor.EncryptSubmissionId("vote-id", "submission-id", LegacyPassword, 1717200000);
            var second = Encryptor.EncryptSubmissionId("vote-id", "submission-id", LegacyPassword, 1717200000);

            first.ShouldNotBe(second);
        }
    }
}
