using AdPlatformLocatorApi.Services;
using Xunit;

namespace AdPlatformLocatorApiTests
{
    public class IndexTests
    {
        [Fact]
        public void Load_And_Query_Basics()
        {
            var idx = new InMemoryPlatformIndex();
            var text = @"
������.������:/ru
���������� �������:/ru/svrd/revda,/ru/svrd/pervik
������ ��������� ���������:/ru/msk,/ru/permobl,/ru/chelobl
������ �������:/ru/svrd
";


            var res = idx.LoadFromText(text);
            Assert.Equal(4, res.TotalPlatforms);


            Assert.Contains("������.������", idx.FindPlatforms("/ru"));
            Assert.DoesNotContain("������ �������", idx.FindPlatforms("/ru"));


            var svrd = idx.FindPlatforms("/ru/svrd");
            Assert.Contains("������.������", svrd);
            Assert.Contains("������ �������", svrd);
            Assert.DoesNotContain("���������� �������", svrd);


            var revda = idx.FindPlatforms("/ru/svrd/revda");
            Assert.Contains("������.������", revda);
            Assert.Contains("������ �������", revda);
            Assert.Contains("���������� �������", revda);


            var msk = idx.FindPlatforms("/ru/msk");
            Assert.Contains("������ ��������� ���������", msk);
            Assert.Contains("������.������", msk);
        }


        [Fact]
        public void Tolerates_Bad_Lines_And_Normalizes()
        {
            var idx = new InMemoryPlatformIndex();
            var text = @"bad line without colon
# comment
NameOnly: 
Good:/a//b/
";
            var res = idx.LoadFromText(text);
            Assert.True(res.TotalPlatforms >= 1);
            var list = idx.FindPlatforms("a/b");
            Assert.Contains("Good", list);
        }
    }
}