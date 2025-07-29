namespace SmartAgro.Models.DTOs.Users
{
    public class PaginatedUsersDto
    {
        public List<UserListDto> Users { get; set; } = new List<UserListDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}