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
Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd
";


            var res = idx.LoadFromText(text);
            Assert.Equal(4, res.TotalPlatforms);


            Assert.Contains("Яндекс.Директ", idx.FindPlatforms("/ru"));
            Assert.DoesNotContain("Крутая реклама", idx.FindPlatforms("/ru"));


            var svrd = idx.FindPlatforms("/ru/svrd");
            Assert.Contains("Яндекс.Директ", svrd);
            Assert.Contains("Крутая реклама", svrd);
            Assert.DoesNotContain("Ревдинский рабочий", svrd);


            var revda = idx.FindPlatforms("/ru/svrd/revda");
            Assert.Contains("Яндекс.Директ", revda);
            Assert.Contains("Крутая реклама", revda);
            Assert.Contains("Ревдинский рабочий", revda);


            var msk = idx.FindPlatforms("/ru/msk");
            Assert.Contains("Газета уральских москвичей", msk);
            Assert.Contains("Яндекс.Директ", msk);
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