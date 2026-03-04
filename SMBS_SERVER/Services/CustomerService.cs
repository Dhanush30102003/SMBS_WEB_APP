using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Repositories;

namespace SMBS_SERVER.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo;
        private readonly SalesInvoiceRepository _invoiceRepository;
        public CustomerService(CustomerRepository repo, SalesInvoiceRepository invoiceRepository)
        {
            _repo = repo;
            _invoiceRepository = invoiceRepository;
        }

        public List<CustomerMaster> GetAll(int tenantId)
            => _repo.GetAll(tenantId);

        public void Create(CustomerMaster c)
            => _repo.Insert(c);

        public CustomerMaster Get(int id)
            => _repo.GetById(id);

        public void Update(CustomerMaster c)
            => _repo.Update(c);

        public void Delete(int id)
            => _repo.Delete(id);
        public void ToggleStatus(int id)
        {
            _repo.ToggleStatus(id);
        }
        public List<SalesInvoice> GetInvoicesByCustomer(int customerId)
        {
            return _invoiceRepository.GetByCustomerId(customerId);
        }
        public CustomerMaster GetByEmail(string email)
        {
            return _repo.GetByUsernameOrEmail(email);
        }
        public void UpdateProfile(int customerId, UpdateCustomerProfileDto dto)
        {
            _repo.UpdateProfile(customerId, dto);
        }
        public void UpdatePassword(string email, string newHash)
        {
            _repo.UpdatePassword(email, newHash);
        }
        public void UpdateFcmToken(int userId, string token)
        {
            _repo.UpdateFcmToken(userId, token);
        }
        public CustomerMaster Authenticate(string email, string password)
        {
            return _repo.Authenticate(email, password);
        }

        public CustomerMaster GetByPhone(string phone)
        {
            return _repo.GetByPhone(phone);
        }

        public List<CustomerMaster> GetAllWithToken(int tenantId)
        {
            return _repo.GetAllWithToken(tenantId);
        }

    }
}
