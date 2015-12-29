using System.Collections.Generic;

namespace OnlineStore.BuisnessLogic.TableManagers.Contracts
{
    public interface ITableManager<TData, in TRepository>
    {
        int GetPagesCount(long count, int pageSize);

        int GetNewPageIndex(int newIndex, int oldIndex, int pagesCount);

        int GetOldPageIndexFromRepository(TRepository repository, string nameInRepository);

        void SetNewPageIndexToRepository(TRepository repository, string nameInRepository, int newPageIndex);

        int[] GetPages(int newIndex, int pagesCount, int visiblePagesCount);

        TData[] GetPageData(List<TData> data, int pageIndex, int pageSize);
    }
}