using Microsoft.AspNetCore.Identity;
using NuGet.Protocol.Core.Types;
using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The bas person service.
    /// </summary>
    /// <typeparam name="TDto">The dto type.</typeparam>
    public abstract class BasePersonService<TEntity, TDto>
        where TEntity : class, IPerson
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Func<IPersonRepository<TEntity>> repositoryFactory;
        private readonly Func<TEntity, TDto> toDto;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePersonService{TDto}" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="toDto">The function to convert a model to a DTO.</param>
        /// <param name="userManager">The user manager.</param>
        public BasePersonService(
            IUnitOfWork unitOfWork,
            Func<IPersonRepository<TEntity>> repositoryFactory,
            Func<TEntity, TDto> toDto,
            UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.repositoryFactory = repositoryFactory;
            this.toDto = toDto;
            this.userManager = userManager;
        }

        protected abstract TEntity CreateEntity(RegisterUserDto registerData, string userId);

        /// <summary>
        /// Gets all the persons.
        /// </summary>
        /// <returns>The persons.</returns>
        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var persons = await this.repositoryFactory().GetAllAsync();
            var dtos = persons.Select(x => this.toDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The person.</returns>
        public async Task<TDto> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var person = await this.repositoryFactory().GetAsync(id);
            if (person == null)
            {
                throw new PersonNotFoundException<TEntity>(id);
            }

            return this.toDto(person);
        }

        /// <summary>
        /// Gets the person by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The person.</returns>
        public async Task<TDto> GetByUserIdAsync(string userId)
        {
            var person = await this.repositoryFactory().GetByUserIdAsync(userId);
            if (person == null)
            {
                throw new PersonNotFoundException<TEntity>(userId);
            }

            return this.toDto(person);
        }

        /// <summary>
        /// Registers a new person.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <param name="role">The role.</param>
        /// <returns>The person.</returns>
        public async Task<TDto> RegisterAsync(RegisterUserDto registerData, RoleType role)
        {
            ArgumentNullException.ThrowIfNull(registerData);

            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    TDto? newPerson;
                    var userId = await this.RegisterUserAsync(registerData.Name, registerData.Email, registerData.Password, registerData.PhoneNumber, role);
                    var repository = this.repositoryFactory();
                    var personByPhone = await repository.GetByPhoneAsync(registerData.PhoneNumber, true);
                    var personByEmail = await repository.GetByEmailAsync(registerData.Email);

                    if (personByPhone != null && personByEmail != null && personByPhone.Id == personByEmail.Id)
                    {
                        if (role == RoleType.Customer && personByPhone.UserId == null)
                        {
                            // Link existing guest customer to the new user account
                            personByPhone.UserId = userId;
                            await this.unitOfWork.SaveChangesAsync();
                            newPerson = this.toDto(personByPhone);
                        }
                        else
                        {
                            // The person is already linked to another user account
                            throw new RegistrationFailedException("A user with the same email and phone number already exists.");
                        }
                    }
                    else if (personByPhone != null || personByEmail != null)
                    {
                        throw new RegistrationFailedException("A user with the same email or phone number already exists.");
                    }
                    else
                    {
                        var personToCreate = this.CreateEntity(registerData, userId);
                        repository.Add(personToCreate);
                        await this.unitOfWork.SaveChangesAsync();
                        newPerson = this.toDto(personToCreate);
                    }

                    await transaction.CommitAsync();
                    return newPerson;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private async Task<string> RegisterUserAsync(string name, string email, string password, string phoneNumber, RoleType role)
        {
            var existingUser = await this.userManager.FindByNameAsync(email);
            if (existingUser != null && !existingUser.IsDeleted)
            {
                throw new RegistrationFailedException("A user with the same email already exists.");
            }

            var user = new ApplicationUser(name)
            {
                UserName = email,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await this.userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new RegistrationFailedException(string.Join("\n", result.Errors.Select(e => e.Description)));
            }

            await this.userManager.AddToRoleAsync(user, role.ToString());
            return user.Id;
        }

        /// <summary>
        /// Updates a person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAsync(Guid id, UpdateContactInfoDto updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            // Dont track the entity here to avoid double tracking in UpdateAccountAsync method.
            var person = await this.repositoryFactory().GetAsync(id);
            if (person == null)
            {
                throw new PersonNotFoundException<TEntity>(id);
            }

            if (person.UserId != null)
            {
                await this.UpdateAccountAsync(person.UserId, updateData);
            }
            else
            {
                // This is for guest customers who have no associated user account.
                person = await this.repositoryFactory().GetAsync(id, true);
                await UpdateProperties(updateData, person!, this.repositoryFactory());
                await this.unitOfWork.SaveChangesAsync();
            }
        }

        private async static Task UpdateProperties(UpdateContactInfoDto updateData, TEntity person, IPersonRepository<TEntity> repository)
        {
            var personByPhone = string.IsNullOrEmpty(updateData.PhoneNumber) ? null : await repository.GetByPhoneAsync(updateData.PhoneNumber);
            if (personByPhone != null && personByPhone.Id != person.Id)
            {
                throw new UpdatePersonFailedException(person.Id, "A user with the same phone number already exists.");
            }

            if (!string.IsNullOrEmpty(updateData.Name))
            {
                person.Name = updateData.Name;
            }

            if (!string.IsNullOrEmpty(updateData.PhoneNumber))
            {
                person.PhoneNumber = updateData.PhoneNumber;
            }
        }

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="updateData">The update data.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAccountAsync(string userId, UpdateContactInfoDto updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var user = await this.userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        throw new PersonNotFoundException<TEntity>(userId);
                    }

                    var repository = this.repositoryFactory();
                    var person = await repository.GetByUserIdAsync(userId, true);
                    if (person == null)
                    {
                        throw new PersonNotFoundException<TEntity>(userId);
                    }
                    await UpdateProperties(updateData, person, repository);

                    if (!string.IsNullOrEmpty(updateData.Name))
                    {
                        user.Name = updateData.Name;
                    }
                    if (!string.IsNullOrEmpty(updateData.PhoneNumber))
                    {
                        user.PhoneNumber = updateData.PhoneNumber;
                    }

                    var result = await this.userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        throw new UpdatePersonFailedException(userId,string.Join("\n", result.Errors.Select(e => e.Description)));
                    }

                    await this.unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="passwordData">The password data.</param>
        /// <returns>A value indicating whether the password update was successful.</returns>
        public async Task UpdatePasswordAsync(string userId, PasswordDto passwordData)
        {
            ArgumentNullException.ThrowIfNull(nameof(userId));
            ArgumentNullException.ThrowIfNull(nameof(passwordData));
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new PersonNotFoundException<TEntity>(userId);
            }

            var result = await this.userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
            if (!result.Succeeded)
            {
                throw new UpdatePasswordFailedException(userId, string.Join("\n", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Deletes a person.
        /// </summary>
        /// <param name="id">The id.</param>=
        /// <returns>A task that represents the delete operation.</returns>
        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var person = await this.repositoryFactory().GetAsync(id, true);
            if (person == null)
            {
                throw new PersonNotFoundException<TEntity>(id);
            }

            if (person.UserId != null)
            {
                await this.DeleteAccountAsync(person.UserId, person);
            }
            else
            {
                // This is for guest customers who have no associated user account.
                person.IsDeleted = true;
                await this.unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="person">The person associated with the user account.</param>
        /// <returns>The result of the delete operation.</returns>
        public async Task DeleteAccountAsync(string userId, IPerson? person = null)
        {
            ArgumentNullException.ThrowIfNull(userId);
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var user = await this.userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        throw new PersonNotFoundException<TEntity>(userId);
                    }

                    user.IsDeleted = true;
                    user.UserName = $"deleted_{user.Id}"; // To avoid future registration conflicts with the same email.
                    await this.userManager.UpdateAsync(user);
                    person ??= await this.repositoryFactory().GetByUserIdAsync(userId, true);
                    if (person == null)
                    {
                        throw new PersonNotFoundException<TEntity>(userId);
                    }
                    
                    person.IsDeleted = true;

                    this.unitOfWork.RefreshTokens.DeleteTokensByUserId(userId);
                    await this.unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Finds persons that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The persons.</returns>
        public async Task<IEnumerable<TDto>> FindPersonsAsync(PersonSearchRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var persons = await this.repositoryFactory().FindAsync(request.Name, request.PhoneNumber, request.Email);
            return persons.Select(this.toDto).ToList();
        }
    }
}