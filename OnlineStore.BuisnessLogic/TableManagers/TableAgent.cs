using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;

namespace OnlineStore.BuisnessLogic.TableManagers
{
    public class TableAgent<TData> : ITableManager<TData>
    {
        public int GetPageIndex(string newIndex, int oldIndex, int pagesCount)
        {
            switch (newIndex)
            {
                case null:
                    return 1;
                case "prev":
                    return oldIndex > 1 ? oldIndex - 1 : 1;
                case "next":
                    return oldIndex < pagesCount ? oldIndex + 1 : pagesCount;
                default:
                    {
                        int i;
                        if (!int.TryParse(newIndex, out i)) return oldIndex;

                        if (i > 0 && i <= pagesCount) return i;
                        return i < 1 ? 1 : pagesCount;
                    }
            }
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

        public List<TData> GetPageData(List<TData> data, int pageIndex, int pageSize)
        {
            var start = (pageIndex - 1) * pageSize;
            return data.GetRange(start, data.Count - start > pageSize ? pageSize : data.Count - start);
        }
    }
}