// PROMPT v4.0: Реалізація ProductRepositoryProxy (Proxy з кешем та TTL)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Infrastructure.Data
{
  // PROMPT v4.0: ProductRepositoryProxy (Proxy з кешем та TTL, async)
  public class ProductRepositoryProxy : IProductRepository
  {
    private readonly IProductRepository _realRepository;
    private List<Product>? _cache;
    private DateTime _cacheTime;
    private readonly TimeSpan _cacheTTL;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // PROMPT v4.2.1: Fallback TTL через константу
    public ProductRepositoryProxy(IProductRepository realRepository, TimeSpan cacheTTL)
    {
      _realRepository = realRepository;
      if (cacheTTL <= TimeSpan.Zero)
      {
        _cacheTTL = TimeSpan.FromMinutes(Helpers.Constants.DefaultProductCacheTTLMinutes);
      }
      else
      {
        _cacheTTL = cacheTTL;
      }
      _cache = null;
      _cacheTime = DateTime.MinValue;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
      await _semaphore.WaitAsync();
      try
      {
        if (_cache == null || DateTime.Now - _cacheTime > _cacheTTL)
        {
          IEnumerable<Product> products = await _realRepository.GetAllAsync();
          _cache = products.ToList();
          _cacheTime = DateTime.Now;
        }
        return _cache;
      }
      finally
      {
        _semaphore.Release();
      }
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
      IEnumerable<Product> products = await GetAllAsync();
      return products.FirstOrDefault(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
      await _realRepository.AddAsync(product);
      await InvalidateCacheAsync();
    }

    public async Task UpdateAsync(Product product)
    {
      await _realRepository.UpdateAsync(product);
      await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      await _realRepository.DeleteAsync(id);
      await InvalidateCacheAsync();
    }

    private async Task InvalidateCacheAsync()
    {
      await _semaphore.WaitAsync();
      try
      {
        _cache = null;
        _cacheTime = DateTime.MinValue;
      }
      finally
      {
        _semaphore.Release();
      }
    }
  }
}
