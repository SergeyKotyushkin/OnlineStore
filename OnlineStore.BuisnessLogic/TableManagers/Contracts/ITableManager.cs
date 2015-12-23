using System.Collections.Generic;

namespace OnlineStore.BuisnessLogic.TableManagers.Contracts
{
    public interface ITableManager<TData, in TRepository>
    {
        int GetPagesCount(int count, int pageSize);

        int GetNewPageIndex(TRepository repository, string nameInStorage, string newIndex, int pagesCount);

        int GetPageIndex(string newIndex, int oldIndex, int pagesCount);

        int[] GetPages(int newIndex, int pagesCount, int visiblePagesCount);

        TData[] GetPageData(List<TData> data, int pageIndex, int pageSize);
    }
}