namespace Framework_Project.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; } //tổng số items
        public int PageSize { get; private set; } //tổng số item/trang
        public int CurrentPage { get; private set; } //trang hiện tại

        public int TotalPages { get; private set; } //tổng trang
        public int StartPage { get; private set; } //trang bắt đầu
        public int EndPage { get; private set; } //trang kết thúc
        public Paginate()
        {

        }
        public Paginate(int totalItems, int page, int pageSize = 10) 
        {
            //làm tròn tổng items/10 items trên 1 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize); //33/10 = 3.3 4 trang

            int currentPage = page; 

            int startPage = currentPage - 5;
            int endPage = currentPage + 4; 

            if (startPage <= 0)
            {
                
                endPage = endPage - (startPage - 1);
                startPage = 1;

            }
            if (endPage > totalPages)
            {
                endPage = totalPages; 
                if (endPage > 10) 
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
