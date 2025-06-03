using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Repository.Components
{
    public class BrandsViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public BrandsViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //Lấy danh sách Brand từ database trả về View
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Brands.ToListAsync());
    }
}
