using LGMS.Dto;
using LGMS.Interface;

namespace LGMS.Data.Model
{
    public class PagedData<T>
    {
        public PagedDataDTO<T> GetPagedData(List<T> unpagedData, PagedDataRequestModel requestModel)
        {
            var totalRocords = unpagedData.Count;
            var a = decimal.Divide(totalRocords, requestModel.PageSize);
            var pageCount = (int)Math.Ceiling(a);
            var pagedResult = unpagedData.Skip(requestModel.PageSize * (requestModel.PageNumber - 1)).Take(requestModel.PageSize).ToList();
            return new PagedDataDTO<T> { CurrentPage = requestModel.PageNumber, TotalPages = pageCount, TotalRecords = totalRocords, Data = pagedResult };
        }
    }
}
