using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Users.Library.Database;
using Users.Library.Services;

namespace Users.Test;

[TestClass]
public class UserTest
{
    private readonly SqliteConnection _sqliteConnection;

    private readonly UsersDbContext _usersDbContext;

    private readonly UserService _userService;

    public UserTest()
    {
        _sqliteConnection = new SqliteConnection("Filename=:memory:");
        _sqliteConnection.Open();
        _usersDbContext = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseSqlite(_sqliteConnection).Options);
        _userService = new UserService(_usersDbContext);
    }

    [TestInitialize]
    public async Task ClassInitialize()
    {
        if (!await _usersDbContext.Database.EnsureCreatedAsync())
        {
            throw new Exception("Cannot create database!");
        }
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*")]
    public async Task Create_User_ReturnsCorrectData(string name, string email, string password)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        Assert.AreEqual(name, user.Name);
        Assert.AreEqual(email, user.Email);
        Assert.AreEqual(Library.Constants.Role.User, user.Role);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*")]
    public async Task Create_DuplicateUsers_ThrowsError(string name, string email, string password)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        async Task<Library.Models.User> action() => await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        _ = await Assert.ThrowsExceptionAsync<InvalidOperationException>((Func<Task<Library.Models.User>>)action);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*")]
    public async Task Verify_UserPassword_ReturnsPassed(string name, string email, string password)
    {
        _ = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        var loginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = password
        });

        Assert.IsTrue(loginResult.Success);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp", "Newing McTester", "new-tester@gmail.com")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*", "Better Dee", "jung_lei@gmail.com")]
    public async Task Update_User_CanLoginAndQueryNewInfo(string name, string email, string password, string newName, string newEmail)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        var loginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = password
        });

        Assert.IsTrue(loginResult.Success);

        bool updated = await _userService.UpdateUserAsync(user.Id, new()
        {
            Name = newName,
            Email = newEmail
        });

        Assert.IsTrue(updated);

        var newLoginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = newEmail,
            Password = password
        });

        Assert.IsTrue(newLoginResult.Success);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp", "SuperStrongPassword")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*", "wE@k P@ssw0rd")]
    public async Task Update_UserPassword_CanLogin(string name, string email, string password, string newPassword)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        var loginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = password
        });

        Assert.IsTrue(loginResult.Success);

        bool updated = await _userService.UpdateUserPasswordAsync(user.Id, new()
        {
            Password = newPassword
        });

        Assert.IsTrue(updated);

        var newLoginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = newPassword
        });

        Assert.IsTrue(newLoginResult.Success);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*")]
    public async Task Filter_User_ReturnsCorrectData(string name, string email, string password)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        Assert.AreEqual(name, user.Name);
        Assert.AreEqual(email, user.Email);
        Assert.AreEqual(Library.Constants.Role.User, user.Role);

        var userFindById = (await _userService.FilterUsersAsync([user.Id])).Single();

        Assert.AreEqual(name, userFindById.Name);
        Assert.AreEqual(email, userFindById.Email);
        Assert.AreEqual(Library.Constants.Role.User, userFindById.Role);

        var userFindByName = (await _userService.FilterUsersAsync(null, user.Name)).Single();

        Assert.AreEqual(name, userFindByName.Name);
        Assert.AreEqual(email, userFindByName.Email);
        Assert.AreEqual(Library.Constants.Role.User, userFindByName.Role);
    }

    [TestMethod]
    [DataRow("Kotlin McTester", "mc-tester@gmail.com", "P;lfj9-402384rp")]
    [DataRow("Dee Jung Lei", "jung_lei@gmail.com", "12349f23l8d3(*")]
    public async Task Delete_User_ShouldNotLoginOrFilter(string name, string email, string password)
    {
        var user = await _userService.CreateUserAsync(new()
        {
            Name = name,
            Email = email,
            Password = password
        });

        var loginResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = password
        });

        Assert.IsTrue(loginResult.Success);

        bool deleted = await _userService.DeleteUserAsync(user.Id);

        Assert.IsTrue(deleted);

        var loginAfterResult = await _userService.VerifyUserPasswordAsync(new()
        {
            Email = email,
            Password = password
        });

        Assert.IsFalse(loginAfterResult.Success);

        var users = await _userService.FilterUsersAsync([user.Id]);

        Assert.IsFalse(users.Any());
    }
}
