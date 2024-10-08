﻿using LGMS.Data.Model;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class EquipmentsSearchModel
    {
        public EquipmentsSearchModel()
        {
            SortDetails = new SortRequestModel();
            PaginationDetails = new PagedDataRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public IPagedDataRequestModel PaginationDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
        public List<EquipmentStatus> Statuses { get; set; }
    }
}
