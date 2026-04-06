using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Helpers
{
    public class PagedList<T> : List<T>
    {
            //the <T> means this can work for products, orders, users, or anything else
            public int CurrentPage { get; private set; }
            public int TotalPages { get; private set; }
            public int PageSize { get; private set; }
            public int TotalCount { get; private set; }

            public PagedList(List<T> items, int count, int pageNumber, int pageSize)
            {
                TotalCount = count;
                PageSize = pageSize;
                CurrentPage = pageNumber;
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                AddRange(items);
            }
            // This is the magic method that does the Skip and Take math for us
            public static async Task<PagedList<T>> CreateAsync (IQueryable<T> source, int pageNumber, int pageSize)
            {
                var count = await source.CountAsync(); //count total items in database
                var items = await source.Skip((pageNumber - 1)*pageSize).Take(pageSize).ToListAsync();
                return new PagedList<T>(items, count, pageNumber, pageSize);

            }
        
    }
}
