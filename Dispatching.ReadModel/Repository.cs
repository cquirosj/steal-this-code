﻿using Dispatching.ReadModel.PersistenceModel;
using System;
using System.Threading.Tasks;

namespace Dispatching.ReadModel
{
    internal abstract class Repository<T> where T : Entity, new()
    {
        private readonly DispatchingReadDbContext _dbContext;

        protected Repository(DispatchingReadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Save(T input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var item = await FindById(input.Id);
            if (item == null) 
            {
                item = new T();
                await Add(item);
            }

            CopyValues(item, input);
            await _dbContext.SaveChangesAsync();
        }

        public abstract Task<T> FindById(Guid id);

        protected abstract Task Add(T newItem);

        protected abstract void CopyValues(T currentItem, T updatedItem);
    }
}
