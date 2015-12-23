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

        public int GetPagesCount(int count, int pageSize)
        {
            return (int)Math.Ceiling((double)count / pageSize);
        }

        public int GetNewPageIndex(TRepository repository, string nameInStorage, string newIndex, int pagesCount)
        {
            var oldPageIndex = (int) (_storageRepository.Get(repository, nameInStorage) ?? 1);
            var newPageIndex = GetPageIndex(newIndex, oldPageIndex, pagesCount);
            _storageRepository.Set(repository, nameInStorage, newPageIndex);

            return newPageIndex;
        }

        public int GetPageIndex(string newIndex, int oldIndex, int pagesCount)
        {
            int result;
            switch (newIndex)
            {
                case null:
                    result = 1;
                    break;
                case "prev":
                    result = oldIndex > 1 ? oldIndex - 1 : 1;
                    break;
                case "next":
                    result = oldIndex < pagesCount ? oldIndex + 1 : pagesCount;
                    break;
                default:
                    {
                        int i;
                        if (!int.TryParse(newIndex, out i)) return oldIndex;

                        if (i > 0 && i <= pagesCount) return i;
                        result = i < 1 ? 1 : pagesCount;
                        break;
                    }
            }

            return result > 0 && result <= pagesCount ? result : 1;
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