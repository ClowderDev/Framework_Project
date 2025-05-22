using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Repository.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public CategoriesViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //Lấy categories từ db và truyền cho view component
        public async Task<IViewComponentResult> InvokeAsync() => View( await _dataContext.Categories.ToListAsync());
    }
}
