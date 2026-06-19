namespace NdlovuBetting.Models.ViewModels
{
    // One row in the admin users table.
    public class UserRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string IdNumber { get; set; } = "";
        public string Role { get; set; } = "Client";   // "Client" or "Admin"
        public int AccountCount { get; set; }
        public decimal TotalBalance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // The whole users-list page: rows, search box, paging, and stats.
    public class UsersListViewModel
    {
        public List<UserRow> Users { get; set; } = new();

        // Search term the admin typed.
        public string? Search { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Stats bar
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalAccounts { get; set; }
        public decimal GrandTotalBalance { get; set; }
    }
}