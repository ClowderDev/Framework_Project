using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public FooterViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        //lấy thông tin Contact từ database rồi trả về view
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Contact.FirstOrDefaultAsync());
    }
}
