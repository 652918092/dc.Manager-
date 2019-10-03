using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj.Dto
{
    public class PagedAndSortedInputDto : PagedInputDto, ISortedResultRequest
    {
        public string Sorting { get; set; }

        public PagedAndSortedInputDto()
        {
            MaxResultCount = AppConsts.DefaultPageSize;
        }
    }
}
