using System;
using GenericCachedRepository;
using Moq;
using NUnit.Framework;

namespace GenericCacheUnitTests
{
    [TestFixture]
    public class CacheUnitTests
    {
        private Mock<IReadRepository> _primaryReadRepo;
        private Mock<IReadRepository> _secondaryReadRepo;
        private Mock<IWriteRepository> _primaryWriteRepo;
        private GenericCache<Guid, Value, IReadRepository, IWriteRepository> _cache;

        [SetUp]
        public void TestSetUp()
        {
            _primaryReadRepo = new Mock<IReadRepository>(MockBehavior.Strict);
            _secondaryReadRepo = new Mock<IReadRepository>(MockBehavior.Strict);
            _primaryWriteRepo = new Mock<IWriteRepository>(MockBehavior.Strict);
            _cache = new GenericCache<Guid, Value, IReadRepository, IWriteRepository>
                        (_primaryReadRepo.Object,
                        _secondaryReadRepo.Object,
                        _primaryWriteRepo.Object,
                        (repo, k) => repo.GetValue(k),
                        (repo, k, v) => repo.SetValue(k, v));
        }

        [TestCase("name1")]
        [TestCase("name2")]
        [Test]
        public void GetData_FromPrimarySource_ExpectsData(string name)
        {
            //Arrange
            var key = Guid.NewGuid();
            var value = new Value() { Name = name };
            _primaryReadRepo.Setup(r => r.GetValue(key)).Returns(value);
            _secondaryReadRepo.Setup(r => r.GetValue(key)).Returns(value);

            //Act
            Value result = _cache.GetDataByKey(key);

            //Assert
            Assert.AreEqual(name, result.Name);
        }

        [TestCase("name3")]
        [TestCase("name4")]
        [Test]
        public void GetData_FromSecondarySource_ExpectsDataAndUpdatePrimary(string name)
        {
            //Arrange
            var key = Guid.NewGuid();
            var value = new Value() { Name = name };
            _primaryReadRepo.Setup(r => r.GetValue(key)).Returns((Value)null);
            _secondaryReadRepo.Setup(r => r.GetValue(key)).Returns(value);
            _primaryWriteRepo.Setup(r => r.SetValue(key, value));

            //Act
            Value result = _cache.GetDataByKey(key);

            //Assert
            Assert.AreEqual(name, result.Name);
            _primaryWriteRepo.Verify(r => r.SetValue(key, value), Times.Once);
        }

        [Test]
        public void GetData_NotFoundInEitherSource_ExpectsNull()
        {
            //Arrange
            var key = Guid.NewGuid();
            _primaryReadRepo.Setup(r => r.GetValue(key)).Returns((Value)null);
            _secondaryReadRepo.Setup(r => r.GetValue(key)).Returns((Value)null);

            //Act
            Value result = _cache.GetDataByKey(key);

            //Assert
            Assert.IsNull(result);
            _primaryWriteRepo.Verify(r => r.SetValue(It.IsAny<Guid>(), It.IsAny<Value>()), Times.Never);
        }
    }

    public interface IReadRepository
    {
        Value GetValue(Guid key);
    }

    public interface IWriteRepository
    {
        void SetValue(Guid key, Value value);
    }


    public class Value
    {
        public string Name { get; set; }
    }
}
