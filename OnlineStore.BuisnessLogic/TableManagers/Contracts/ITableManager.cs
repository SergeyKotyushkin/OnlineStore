using System.Collections.Generic;

namespace OnlineStore.BuisnessLogic.TableManagers.Contracts
{
    public interface ITableManager<TData>
    {
        //void SetCultureForPriceColumns(GridView table, CultureInfo cultureTo, bool performConvert, params int[] columnsIndexes);

        //void SavePageIndex(TV repository, string name, int index);

        //int RestorePageIndex(TV repository, string name);

        //bool CheckIsPageIndexNeedToRefresh(TV repository, string name, GridView table);

        int GetPageIndex(string newIndex, int oldIndex, int pagesCount);

        int[] GetPages(int newIndex, int pagesCount, int visiblePagesCount);

        List<TData> GetPageData(List<TData> data, int pageIndex, int pageSize);
    }
}