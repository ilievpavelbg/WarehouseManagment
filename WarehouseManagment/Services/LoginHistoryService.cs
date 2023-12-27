using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly IRepository _repository;

        public LoginHistoryService(IRepository repository)
        {
                _repository = repository;
        }
        public async Task UserLoginTime(string userId)
        {

                var model = new LoginHistory
                {
                    UserId = userId,
                    LoginTime = DateTime.Now,
                    LogoutTime = null,
                };

            await _repository.AddAsync(model);
            await _repository.SaveChangesAsync();

        }

        public async Task UserLogoutTime(string userId)
        {
            var model =  _repository.All<LoginHistory>().Where(x => x.UserId == userId && x.LogoutTime.HasValue == false).First();

            model.LogoutTime = DateTime.Now;
            await _repository.SaveChangesAsync();
        }
    }
}
