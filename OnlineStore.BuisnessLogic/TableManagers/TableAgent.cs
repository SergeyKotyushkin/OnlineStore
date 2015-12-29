using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;

namespace OnlineStore.BuisnessLogic.TableManagers
{
    public class TableAgent<TData, TRepository> : ITableManager<TData, TRepository>
    {
        private readonly IStorageRepository<TRepository> _storageRepository;

        public TableAgent(IStorageRepository<TRepository> storageRepository)
        {
            _storageRepository = storageRepository;
        }

        public int GetPagesCount(long count, int pageSize)
        {
            return (int)Math.Ceiling((double)count / pageSize);
        }

        public void SetNewPageIndexToRepository(TRepository repository, string nameInRepository, int newPageIndex)
        {
            _storageRepository.Set(repository, nameInRepository, newPageIndex);
        }

        public int GetOldPageIndexFromRepository(TRepository repository, string nameInRepository)
        {
            return (int) (_storageRepository.Get(repository, nameInRepository) ?? 1);
        }

        public int GetNewPageIndex(int newIndex, int oldIndex, int pagesCount)
        {
            if (newIndex <= 1) return 1;
            return newIndex > pagesCount ? pagesCount : newIndex;
        }

        public int[] GetPages(int newIndex, int pagesCount, int visiblePagesCount)
        {
            var indexies = new List<int>();
            for (var i = 0; i < visiblePagesCount; i++)
                indexies.Add(newIndex + i);

            for (var i = indexies[indexies.Count - 1]; i > newIndex; i--)
            {
                if (i <= pagesCount) break;

                indexies.RemoveAt(indexies.Count - 1);
                if (indexies[0] - 1 > 0)
                    indexies.Insert(0, indexies[0] - 1);
            }

            return indexies.ToArray();
        }

        public TData[] GetPageData(List<TData> data, int pageIndex, int pageSize)
        {
            var start = (pageIndex - 1) * pageSize;
            return data.GetRange(start, data.Count - start > pageSize ? pageSize : data.Count - start).ToArray();
        }
    }
}