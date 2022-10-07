﻿using ProjectDK.Models.Models.Users;

namespace ProjectDK.DL.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetEmployeeDetails();

        Task<Employee?> GetEmployeeDetails(int id);

        Task AddEmployee(Employee employee);

        Task UpdateEmployee(Employee employee);

        Task DeleteEmployee(int id);

        Task<bool> CheckEmployee(int id);

        Task<UserInfo?> GetUserInfoAsync(string email, string password);

    }
}
