// PROMPT v4.2.0: Юніт-тести для ProductRepositoryProxy (NUnit, Moq)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.Infrastructure.Data;
using Moq;
using NUnit.Framework;

namespace LiteWebApp.Tests
{
  [TestFixture]
  public class ProductRepositoryProxyTests
  {
    private List<Product> _testProducts;
    private Mock<IProductRepository> _mockRepo;
    private ProductRepositoryProxy _proxy;
    private TimeSpan _ttl;

    [SetUp]
    public void Setup()
    {
      _testProducts = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Test1" },
                new Product { Id = Guid.NewGuid(), Name = "Test2" }
            };
      _mockRepo = new Mock<IProductRepository>();
      _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(_testProducts);
      _ttl = TimeSpan.FromSeconds(1);
      _proxy = new ProductRepositoryProxy(_mockRepo.Object, _ttl);
    }

    // 1. Перевірка кешування (TTL)
    [Test]
    public async Task GetAllAsync_ReturnsFromCache_OnRepeatedCalls()
    {
      IEnumerable<Product> first = await _proxy.GetAllAsync();
      IEnumerable<Product> second = await _proxy.GetAllAsync();
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
      Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public async Task GetAllAsync_RefreshesCache_AfterTTL()
    {
      await _proxy.GetAllAsync();
      // Імітація TTL: вручну зменшуємо _cacheTime
      typeof(ProductRepositoryProxy)
          .GetField("_cacheTime", BindingFlags.NonPublic | BindingFlags.Instance)
          ?.SetValue(_proxy, DateTime.Now - _ttl - TimeSpan.FromMilliseconds(100));
      await _proxy.GetAllAsync();
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    // 2. Інвалідція кешу
    [Test]
    public async Task AddAsync_ResetsCache()
    {
      await _proxy.GetAllAsync();
      await _proxy.AddAsync(new Product { Id = Guid.NewGuid(), Name = "New" });
      await _proxy.GetAllAsync();
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task UpdateAsync_ResetsCache()
    {
      await _proxy.GetAllAsync();
      await _proxy.UpdateAsync(_testProducts[0]);
      await _proxy.GetAllAsync();
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task DeleteAsync_ResetsCache()
    {
      await _proxy.GetAllAsync();
      await _proxy.DeleteAsync(_testProducts[0].Id);
      await _proxy.GetAllAsync();
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    // 3. Асинхронність та потокобезпечність
    [Test]
    public async Task GetAllAsync_Parallel_NoRaceCondition()
    {
      List<Task<IEnumerable<Product>>> tasks = new List<Task<IEnumerable<Product>>>();
      for (int i = 0; i < 10; i++)
      {
        tasks.Add(_proxy.GetAllAsync());
      }
      await Task.WhenAll(tasks);
      _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    // 4. Fallback TTL
    [Test]
    public void Proxy_UsesDefaultTTL_IfZero()
    {
      ProductRepositoryProxy proxy = new ProductRepositoryProxy(_mockRepo.Object, TimeSpan.Zero);
      FieldInfo? field = typeof(ProductRepositoryProxy)
          .GetField("_cacheTTL", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That(field, Is.Not.Null, "_cacheTTL field not found");
      object? value = field!.GetValue(proxy);
      Assert.That(value, Is.Not.Null, "_cacheTTL value is null");
      TimeSpan actual = (TimeSpan)value!;
      Assert.That(actual, Is.EqualTo(TimeSpan.FromMinutes(Helpers.Constants.DefaultProductCacheTTLMinutes)));
    }

    // 5. Коректність роботи з порожнім репозиторієм
    [Test]
    public async Task GetAllAsync_ReturnsEmptyList_IfRepoEmpty()
    {
      _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
      ProductRepositoryProxy proxy = new ProductRepositoryProxy(_mockRepo.Object, _ttl);
      IEnumerable<Product> result = await proxy.GetAllAsync();
      Assert.That(result, Is.Empty);
    }

    // 6. Перевірка GetByIdAsync
    [Test]
    public async Task GetByIdAsync_ReturnsCorrectProduct()
    {
      Product expected = _testProducts[0];
      Product? actual = await _proxy.GetByIdAsync(expected.Id);
      Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_IfNotFound()
    {
      Product? actual = await _proxy.GetByIdAsync(Guid.NewGuid());
      Assert.That(actual, Is.Null);
    }
  }
}
